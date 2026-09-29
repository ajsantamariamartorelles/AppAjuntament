using System.Globalization;
using AppAjuntament.Models.Cursets;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AppAjuntament.Services.Cursets
{
    /// <summary>
    /// Genera el PDF d'autoliquidació / carta de pagament seguint la plantilla municipal
    /// (Plantilles/Plantilla_Autoliquidacio_Ajuntament_Santa_Maria_de_Martorelles.docx).
    /// Un full per <see cref="Liquidacio"/>; el PDF d'una alumna en pot tenir més d'un.
    ///
    /// IMPORTANT: aquesta classe NO implementa cap interfície de QuestPDF ni exposa
    /// tipus de QuestPDF en membres públics. Així, si <c>QuestPDF.dll</c> no arriba al
    /// servidor en un desplegament incremental, només falla la generació de PDFs (es
    /// llança l'excepció quan es crida <see cref="Genera"/>) i no l'arrencada de tota
    /// l'aplicació (el descobriment de controllers fa <c>Assembly.GetTypes()</c>).
    /// </summary>
    public static class LiquidacioPdfDocument
    {
        private static readonly CultureInfo Ca = CultureInfo.GetCultureInfo("ca-ES");
        private static int _licenciaOk;

        /// <summary>PDF amb un full per liquidació.</summary>
        public static byte[] Genera(
            IReadOnlyList<Liquidacio> liquidacions,
            LiquidacioConfig config,
            string expedient,
            byte[]? escut)
        {
            // Llicència Community de QuestPDF (gratuïta per a administracions públiques).
            if (System.Threading.Interlocked.CompareExchange(ref _licenciaOk, 1, 0) == 0)
                QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                foreach (var liq in liquidacions)
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(1.6f, Unit.Centimetre);
                        page.DefaultTextStyle(t => t.FontSize(9));
                        page.Header().Element(h => Header(h, config, escut));
                        page.Content().Element(c => Body(c, liq, config, expedient));
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span($"{config.CapcaleraMunicipi}  ·  ").FontSize(7).FontColor(Colors.Grey.Medium);
                            x.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Medium);
                            x.Span(" / ").FontSize(7).FontColor(Colors.Grey.Medium);
                            x.TotalPages().FontSize(7).FontColor(Colors.Grey.Medium);
                        });
                    });
                }
            });

            return document.GeneratePdf();
        }

        private static void Header(IContainer container, LiquidacioConfig config, byte[]? escut)
        {
            container.PaddingBottom(6).Row(row =>
            {
                if (escut is { Length: > 0 })
                    row.ConstantItem(46).AlignTop().Image(escut).FitWidth();
                row.RelativeItem().PaddingLeft(8).AlignMiddle().Text(config.CapcaleraMunicipi ?? "")
                    .FontSize(11).SemiBold();
            });
        }

        private static void Body(IContainer container, Liquidacio liq, LiquidacioConfig config, string expedient)
        {
            container.Column(col =>
            {
                col.Spacing(7);

                col.Item().Border(1).Background(Colors.Grey.Lighten3).Padding(4)
                    .AlignCenter().Text("AUTOLIQUIDACIÓ / CARTA DE PAGAMENT").Bold().FontSize(11);

                if (liq.Estat == EstatLiquidacio.Anullada)
                    col.Item().AlignCenter().Text("— ANUL·LADA —").Bold().FontColor(Colors.Red.Medium);

                // 1. Dades de l'autoliquidació
                Seccio(col, "1. Dades de l'autoliquidació", t =>
                {
                    Fila(t, "Data de l'autoliquidació", liq.DataEmissio.ToString("dd/MM/yyyy", Ca));
                    Fila(t, "Núm. d'autoliquidació / liquidació", liq.Numero);
                    Fila(t, "Expedient", expedient);
                    Fila(t, "Període / exercici", liq.PeriodeEtiqueta);
                });

                // 2. Persona interessada
                Seccio(col, "2. Persona interessada", t =>
                {
                    Fila(t, "Nom i cognoms / Raó social", liq.AlumneNomComplet);
                    Fila(t, "NIF / CIF", liq.AlumneNif ?? "");
                    Fila(t, "Adreça", liq.AlumneAdreca ?? "");
                    Fila(t, "Representant (si escau)", "");
                });

                // 3. Concepte de l'autoliquidació
                Seccio(col, "3. Concepte de l'autoliquidació", t =>
                {
                    Fila(t, "Concepte / activitat", liq.ConcepteText);
                    Fila(t, "Ordenança / tarifa", liq.OrdenancaText ?? config.OrdenancaTarifa);
                    Fila(t, "Període", liq.PeriodeEtiqueta);
                    Fila(t, "Dates i horari", liq.DatesHorariText ?? "");
                    Fila(t, "Observacions", "");
                });

                // 4. Import
                col.Item().Column(sc =>
                {
                    sc.Item().Text("4. Import").Bold();
                    sc.Item().Border(1).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(5);
                            c.RelativeColumn(1.3f);
                            c.RelativeColumn(1.6f);
                            c.RelativeColumn(1.6f);
                        });
                        HeaderCell(table, "Concepte");
                        HeaderCell(table, "Unitats");
                        HeaderCell(table, "Import unitari");
                        HeaderCell(table, "Import");

                        BodyCell(table, liq.ConcepteText);
                        BodyCell(table, liq.NumSessions.ToString(Ca), Align.Center);
                        BodyCell(table, Eur(liq.PreuPerSessio), Align.Right);
                        BodyCell(table, Eur(liq.Import), Align.Right);
                    });
                    sc.Item().PaddingTop(3).AlignRight().Text($"IMPORT TOTAL: {Eur(liq.Import)}").Bold().FontSize(10);
                });

                // 5. Diligència cobratòria
                Seccio(col, "5. Diligència cobratòria", t =>
                {
                    Fila(t, "Data de pagament", liq.DataCobrament?.ToString("dd/MM/yyyy", Ca) ?? "");
                    Fila(t, "Entitat / mitjà de pagament", "");
                    Fila(t, "Segell i rúbrica", "");
                });

                // 6-10. Textos
                BlocText(col, "6. Entitats col·laboradores i mitjans de pagament",
                    string.Join("\n", new[] { config.EntitatsColaboradoresText, config.MitjansPagamentText }
                        .Where(s => !string.IsNullOrWhiteSpace(s))));
                BlocText(col, "7. Instruccions de recursos", config.TextRecursos);
                BlocText(col, "8. Important", config.TextImportant);
                BlocText(col, "9. Terminis cobratoris", config.TextTerminis);
                BlocText(col, "10. Oficina cobratòria", config.OficinaCobratoriaText);
            });
        }

        // ---- helpers de maquetació ----

        private static void Seccio(ColumnDescriptor col, string titol, Action<TableDescriptor> files)
        {
            col.Item().Column(sc =>
            {
                sc.Item().Text(titol).Bold();
                sc.Item().Border(1).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1);
                        c.RelativeColumn(2);
                    });
                    files(table);
                });
            });
        }

        private static void Fila(TableDescriptor table, string etiqueta, string? valor)
        {
            table.Cell().Background(Colors.Grey.Lighten4).BorderRight(1).BorderBottom(1).Padding(3)
                .Text(etiqueta).SemiBold();
            table.Cell().BorderBottom(1).Padding(3).Text(string.IsNullOrWhiteSpace(valor) ? " " : valor);
        }

        private static void HeaderCell(TableDescriptor table, string text) =>
            table.Cell().Background(Colors.Grey.Lighten3).BorderRight(1).BorderBottom(1).Padding(3)
                .Text(text).SemiBold();

        private enum Align { Left, Center, Right }

        private static void BodyCell(TableDescriptor table, string text, Align align = Align.Left)
        {
            IContainer cell = table.Cell().BorderRight(1).BorderBottom(1).Padding(3);
            cell = align switch
            {
                Align.Center => cell.AlignCenter(),
                Align.Right => cell.AlignRight(),
                _ => cell
            };
            cell.Text(string.IsNullOrWhiteSpace(text) ? " " : text);
        }

        private static void BlocText(ColumnDescriptor col, string titol, string? text)
        {
            col.Item().Column(sc =>
            {
                sc.Item().Text(titol).Bold();
                sc.Item().Border(1).Padding(4).Text(string.IsNullOrWhiteSpace(text) ? " " : text).FontSize(8);
            });
        }

        private static string Eur(decimal v) => v.ToString("#,##0.00", Ca) + " €";
    }
}
