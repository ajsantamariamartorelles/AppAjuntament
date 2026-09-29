using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Text.Json;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using AppAjuntament.Models.Base.AcceptacioTermes;
using AppAjuntament.Models.Base.Comarca;
using AppAjuntament.Models.Base.Contacte;
using AppAjuntament.Models.Base.Ens;
using AppAjuntament.Models.Base.Entitat;
using AppAjuntament.Models.Base.Estat;
using AppAjuntament.Models.Base.Geo;
using AppAjuntament.Models.Base.Imatge;
using AppAjuntament.Models.Base.Log;
using AppAjuntament.Models.Base.Provincia;
using AppAjuntament.Models.Base.Regidor;
using AppAjuntament.Models.Base.Rol;
using AppAjuntament.Models.Base.Sexe;
using AppAjuntament.Models.Base.Usuari;
using AppAjuntament.Models.Tercers;
using AppAjuntament.Models.Voluntariat;
using AppAjuntament.Models.Conveni;
using AppAjuntament.Models.Municipi;
using MunicipiModel = AppAjuntament.Models.Municipi.Municipi;
using AppAjuntament.Models.CIDO;
using AppAjuntament.Models.Electoral;
using AppAjuntament.Models.Establiments;
using AppAjuntament.Models.Subvencions;
using AppAjuntament.Models.Documents;
using AppAjuntament.Models.Armes;
using AppAjuntament.Models.IngressosExterns;
using AppAjuntament.Models.Cursets;

namespace AppAjuntament.Models
{
    public class GestorSubvencionsContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public GestorSubvencionsContext(DbContextOptions<GestorSubvencionsContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Any_> Anys { get; set; }
        public DbSet<Area> Arees { get; set; }
        public DbSet<Comarca> Comarques { get; set; }
        public DbSet<Contacte> Contactes { get; set; }
        public DbSet<DadesGeografiques> DadesGeografiques { get; set; }
        public DbSet<Ens> Ens { get; set; }
        public DbSet<Entitat> Entitats { get; set; }
        public DbSet<Estat> Estats { get; set; }
        public DbSet<Imatge> Imatges { get; set; }
        public DbSet<Provincia> Provincies { get; set; }
        public DbSet<Regidor> Regidors { get; set; }
        public DbSet<Rol> Rols { get; set; }
        public DbSet<Sexe> Sexes { get; set; }
        public DbSet<Subvencio_> Subvencions { get; set; }
        public DbSet<RegidorsSubvencions> RegidorsSubvencions { get; set; }
        public DbSet<Expedient> Expedients { get; set; }
        public DbSet<Pagament> Pagaments { get; set; }
        public DbSet<Usuari> Usuaris { get; set; }
        public DbSet<UsuariRol> UsuarisRols { get; set; }

        // Tercers
        public DbSet<Tercer> Tercers { get; set; }

        // DbSets per als voluntaris
        public DbSet<AmbitVoluntari> AmbitsVoluntaris { get; set; }
        public DbSet<SubambitVoluntari> SubambitsVoluntaris { get; set; }
        public DbSet<VoluntariAmbit> VoluntarisAmbits { get; set; }
        public DbSet<EstatVoluntari> EstatsVoluntaris { get; set; }
        public DbSet<Voluntari> Voluntaris { get; set; }
        public DbSet<TipologiaVoluntari> TipologiesVoluntaris { get; set; }
        public DbSet<Activitat> Activitats { get; set; }
        public DbSet<ActitatVoluntari> ActivitatsVoluntaris { get; set; }
        public DbSet<Formacio> Formacions { get; set; }
        public DbSet<FormacioVoluntari> FormacionsVoluntaris { get; set; }
        public DbSet<ConvenisLocals> ConvenisLocals { get; set; }
        public DbSet<ConvenisAuxiliars> ConvenisAuxiliars { get; set; }

        // Documents genèrics (polimòrfics)
        public DbSet<Document> Documents { get; set; }
        public DbSet<TipusDocument> TipusDocuments { get; set; }
        public DbSet<Tramit> Tramits { get; set; }
        public DbSet<TramitTipusDocument> TramitTipusDocuments { get; set; }

        // ARMES
        public DbSet<AuxTipusArma> AuxTipusArmes { get; set; }
        public DbSet<AuxTipusLicencia> AuxTipusLicencies { get; set; }
        public DbSet<AuxEstatLicencia> AuxEstatLicencies { get; set; }
        public DbSet<AuxResultatInspeccio> AuxResultatsInspeccio { get; set; }
        public DbSet<Arma> Armes { get; set; }
        public DbSet<ArmaLlicencia> ArmesLlicencies { get; set; }
        public DbSet<ArmaInspeccio> ArmesInspeccions { get; set; }
        
        // Patrimoni
        public DbSet<SubtipusPatrimoni> SubtipusPatrimonis { get; set; }

        // Municipis
        public DbSet<MunicipiModel> Municipis { get; set; }
        public DbSet<IngressosExternsFonsCooperacio> IngressosExternsFonsCooperacio { get; set; }
        public DbSet<IngressosExternsPendentPagament> IngressosExternsPendentsPagament { get; set; }

        // DbSets per als terminis
        public DbSet<Termini> Terminis { get; set; }
        public DbSet<TerminisSubvencio> TerminisSubvencions { get; set; }

        // DbSets per al sistema de logging
        public DbSet<Log> Logs { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AcceptacioTermes> AcceptacionsTermes { get; set; }

        // DbSets per al sistema de patrimoni
        public DbSet<TipusPatrimoni> TipusPatrimonis { get; set; }
        public DbSet<EstatConservacio> EstatsConservacio { get; set; }
        public DbSet<Ubicacio> Ubicacions { get; set; }
        public DbSet<Patrimoni> Patrimonis { get; set; }
        public DbSet<TipusArxiu> TipusArxius { get; set; }
        public DbSet<ArxiuPatrimoni> ArxiusPatrimoni { get; set; }
        public DbSet<TipusNotaPatrimoni> TipusNotesPatrimoni { get; set; }
        public DbSet<NotaPatrimoni> NotesPatrimoni { get; set; }
        public DbSet<Inspeccio> Inspeccions { get; set; }
        public DbSet<Intervencio> Intervencions { get; set; }
        public DbSet<PatrimoniIdentificador> PatrimoniIdentificadors { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PatrimoniColeccio> PatrimoniColeccions { get; set; }
        public DbSet<PatrimoniLocalitzacio> PatrimoniLocalitzacions { get; set; }

        // Cursets (pilates, ioga, zumba... qualsevol tipus de curset)
        public DbSet<TipusCurset> TipusCursets { get; set; }
        public DbSet<Alumne> CursetsAlumnes { get; set; }
        public DbSet<Curset> Cursets { get; set; }
        public DbSet<AlumneCurset> AlumnesCursets { get; set; }
        public DbSet<Sessio> CursetsSessions { get; set; }
        public DbSet<Assistencia> CursetsAssistencies { get; set; }
        public DbSet<Liquidacio> CursetsLiquidacions { get; set; }
        public DbSet<LiquidacioRemesa> CursetsLiquidacionsRemeses { get; set; }
        public DbSet<LiquidacioConfig> CursetsLiquidacionsConfig { get; set; }
        public DbSet<LiquidacioComptador> CursetsLiquidacionsComptador { get; set; }
        public DbSet<EmailAbsenciesConfig> CursetsEmailAbsenciesConfig { get; set; }

        // La configuració de connexió es fa per injecció de dependències a
        // Program.cs (AGENTS.md, regla 3: cap connection string al codi).

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DadesGeografiques>(entity =>
            {
                entity.Property(e => e.LocalitzacioLatitud).HasColumnName("LocalitzacioLatitud");
                entity.Property(e => e.LocalitzacioLongitud).HasColumnName("LocalitzacioLongitud");
                entity.Property(e => e.CentreMunicipalLatitud).HasColumnName("CentreMunicipalLatitud");
                entity.Property(e => e.CentreMunicipalLongitud).HasColumnName("CentreMunicipalLongitud");
            });

            // Configuració de la taula Comarca
            modelBuilder.Entity<Comarca>(entity =>
            {
                // EF usarà Codi com a clau primària per convenció (amb [Key])
                entity.Property(e => e.Coordenades).HasColumnName("Coordenades");
                entity.Property(e => e.NomDBPedia).HasColumnName("NomDBPedia");
                entity.Property(e => e.CCAdrecaCompleta).HasColumnName("CCAdrecaCompleta");
                entity.Property(e => e.CCAdreca).HasColumnName("CCAdreca");
                entity.Property(e => e.CCCodiPostal).HasColumnName("CCCodiPostal");
                entity.Property(e => e.CCEmail).HasColumnName("CCEmail");
                entity.Property(e => e.CCFax).HasColumnName("CCFax");
                entity.Property(e => e.CCWeb).HasColumnName("CCWeb");
            });
            
            // Configuració de les relacions i claus foranes
            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("arees");
                entity.HasOne(a => a.Ens)
                    .WithMany()
                    .HasForeignKey(a => a.EnsId);
            });

            // Any_: la columna Nom no existeix a la taula 'anys' (només Id i Any)
            modelBuilder.Entity<Any_>(entity =>
            {
                entity.ToTable("anys");
                entity.Ignore(a => a.Nom);
            });

            // Relació Ens -> Comarca (ara que els tipus coincideixen)
            modelBuilder.Entity<Ens>()
                .HasOne(e => e.Comarca)
                .WithMany()
                .HasForeignKey(e => e.ComarcaId);

            modelBuilder.Entity<Ens>()
                .HasOne(e => e.Contacte)
                .WithMany()
                .HasForeignKey(e => e.ContacteId);

            modelBuilder.Entity<Ens>()
                .HasOne(e => e.Provincia)
                .WithMany()
                .HasForeignKey(e => e.ProvinciaId);

            modelBuilder.Entity<Ens>()
                .HasOne(e => e.Imatge)
                .WithMany()
                .HasForeignKey(e => e.ImatgeId);

            modelBuilder.Entity<Ens>()
                .HasOne(e => e.DadesGeografiques)
                .WithMany()
                .HasForeignKey(e => e.DadesGeografiquesId);

            modelBuilder.Entity<Regidor>()
                .HasOne(r => r.Ens)
                .WithMany()
                .HasForeignKey(r => r.EnsId);

            modelBuilder.Entity<Regidor>()
                .HasOne(r => r.Sexe)
                .WithMany()
                .HasForeignKey(r => r.SexeId);

            // Configuracions taules bàsiques de subvencions
            modelBuilder.Entity<Contacte>(entity =>
            {
                entity.ToTable("SUBV_contactes");
            });

            modelBuilder.Entity<Estat>(entity =>
            {
                entity.ToTable("SUBV_estats");
            });

            modelBuilder.Entity<Subvencio_>()
                .HasOne(s => s.Entitat)
                .WithMany()
                .HasForeignKey(s => s.EntitatId);

            modelBuilder.Entity<Subvencio_>()
                .HasOne(s => s.Any)
                .WithMany()
                .HasForeignKey(s => s.AnyId);

            modelBuilder.Entity<Subvencio_>()
                .HasOne(s => s.Area)
                .WithMany()
                .HasForeignKey(s => s.AreaId);

            modelBuilder.Entity<Subvencio_>()
                .HasOne(s => s.Estat)
                .WithMany()
                .HasForeignKey(s => s.EstatId);

            // Eliminar relació directa RegidorId si ja no s'utilitza
            // modelBuilder.Entity<Subvencio_>()
            //     .HasOne(s => s.Regidor)
            //     .WithMany()
            //     .HasForeignKey(s => s.RegidorId);

            modelBuilder.Entity<Subvencio_>()
                .HasOne(s => s.Ens)
                .WithMany()
                .HasForeignKey(s => s.EnsId);

            // La columna FontResponsable no existeix a la taula SUBV_subvencions
            modelBuilder.Entity<Subvencio_>()
                .Ignore(s => s.FontResponsable);

            // Configuració many-to-many RegidorsSubvencions
            modelBuilder.Entity<RegidorsSubvencions>(entity =>
            {
                entity.ToTable("SUBV_regidorssubvencions");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Subvencio)
                    .WithMany(s => s.RegidorsSubvencions)
                    .HasForeignKey(e => e.SubvencioId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Regidor)
                    .WithMany(r => r.RegidorsSubvencions)
                    .HasForeignKey(e => e.RegidorId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => new { e.SubvencioId, e.RegidorId }).IsUnique();
            });

            // Configuració Expedients
            modelBuilder.Entity<Expedient>(entity =>
            {
                entity.ToTable("SUBV_expedients");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroExpedient).IsRequired().HasMaxLength(20);
                entity.HasOne(e => e.Subvencio)
                    .WithMany(s => s.Expedients)
                    .HasForeignKey(e => e.SubvencioId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.SubvencioId);
            });

            // Configuració Pagaments
            modelBuilder.Entity<Pagament>(entity =>
            {
                entity.ToTable("SUBV_pagaments");
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Subvencio)
                    .WithMany(s => s.Pagaments)
                    .HasForeignKey(e => e.SubvencioId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.SubvencioId);
                entity.Property(e => e.Currency).HasMaxLength(10);
                entity.Property(e => e.Comment).HasMaxLength(1000);
                entity.Property(e => e.ServiceDescription).HasMaxLength(1000);
                // CreatedAt i UpdatedAt no existeixen a la taula SUBV_pagaments (no es van incloure a CreatePagamentsTable.sql)
                entity.Ignore(e => e.CreatedAt);
                entity.Ignore(e => e.UpdatedAt);
            });

            modelBuilder.Entity<Usuari>(entity =>
            {
                // La columna Cognoms no existeix a la taula 'usuaris'
                entity.Ignore(u => u.Cognoms);
                // Actiu: columna afegida per la gestió de professores del mòdul Cursets
                // (SQL/alter_usuaris_add_actiu.sql). Permet donar de baixa una professora
                // sense esborrar l'usuari.
                entity.Property(u => u.Actiu).HasColumnName("Actiu").HasDefaultValue(true);
                // A la taula 'usuaris' la columna de la FK es diu 'EnsId'.
                entity.Property(u => u.EntitatId).HasColumnName("EnsId");
                entity.HasOne(u => u.Entitat)
                    .WithMany()
                    .HasForeignKey(u => u.EntitatId);
            });

            modelBuilder.Entity<UsuariRol>()
                .HasKey(ur => new { ur.UsuariId, ur.RolId });

            modelBuilder.Entity<UsuariRol>()
                .HasOne(ur => ur.Usuari)
                .WithMany()
                .HasForeignKey(ur => ur.UsuariId);

            modelBuilder.Entity<UsuariRol>()
                .HasOne(ur => ur.Rol)
                .WithMany()
                .HasForeignKey(ur => ur.RolId);

            // ======== TERCERS ========

            modelBuilder.Entity<Tercer>(entity =>
            {
                entity.ToTable("Tercers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Cognoms).HasMaxLength(255);
                entity.Property(e => e.DNI).HasMaxLength(20);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.Telefon).HasMaxLength(20);
                entity.Property(e => e.Adreca).HasMaxLength(500);
                entity.Property(e => e.CodiPostal).HasMaxLength(10);
                entity.Property(e => e.Poblacio).HasMaxLength(255);

                // Columns amb snake_case a la BD
                entity.Property(e => e.MunicipiIne).HasColumnName("municipi_ine").HasMaxLength(10);
                entity.Property(e => e.ProvinciaId).HasColumnName("provincia_id").HasMaxLength(10);
                entity.Property(e => e.ComarcaCodi).HasColumnName("comarca_codi");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

                // Relació amb Sexe
                entity.HasOne(e => e.Sexe)
                    .WithMany()
                    .HasForeignKey(e => e.SexeId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Relació amb Usuari (created_by / updated_by)
                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.UpdatedBy)
                    .OnDelete(DeleteBehavior.SetNull);

                // Índexs
                entity.HasIndex(e => e.DNI).IsUnique();
                entity.HasIndex(e => e.Email);
                entity.HasIndex(e => e.MunicipiIne);
            });

            // ======== CONFIGURACIÓ VOLUNTARIS ========

            // Configuració AuditLog
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("audit_logs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TableName).HasColumnName("table_name");
                entity.Property(e => e.RecordId).HasColumnName("record_id");
                entity.Property(e => e.Action).HasColumnName("action");
                entity.Property(e => e.OldValues).HasColumnName("old_values");
                entity.Property(e => e.NewValues).HasColumnName("new_values");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Timestamp).HasColumnName("timestamp");
                entity.Property(e => e.IpAddress).HasColumnName("ip_address");
                entity.Property(e => e.SessionId).HasColumnName("session_id");
            });

            // Configuració Log
            modelBuilder.Entity<Log>(entity =>
            {
                entity.ToTable("logs");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Timestamp).HasColumnName("timestamp");
                entity.Property(e => e.Level).HasColumnName("level");
                entity.Property(e => e.Message).HasColumnName("message");
                entity.Property(e => e.Exception).HasColumnName("exception");
                entity.Property(e => e.UserId).HasColumnName("user_id");
                entity.Property(e => e.Action).HasColumnName("action");
                entity.Property(e => e.IpAddress).HasColumnName("ip_address");
                entity.Property(e => e.SessionId).HasColumnName("session_id");
                entity.Property(e => e.AdditionalData).HasColumnName("additional_data");
            });

            // Configuració AcceptacioTermes: una fila per correu (índex únic).
            modelBuilder.Entity<AcceptacioTermes>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Configuració AmbitVoluntari
            modelBuilder.Entity<AmbitVoluntari>(entity =>
            {
                entity.ToTable("VOLU_ambits_voluntaris");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(191); // Reduït per evitar problemes d'índex
                entity.Property(e => e.Descripcio).HasMaxLength(500);
                entity.HasIndex(e => e.Nom).IsUnique();
            });

            // Configuració SubambitVoluntari
            modelBuilder.Entity<SubambitVoluntari>(entity =>
            {
                entity.ToTable("VOLU_subambits_voluntaris");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(191);
                entity.Property(e => e.Descripcio).HasMaxLength(500);
                entity.HasIndex(e => new { e.AmbitVoluntariId, e.Nom }).IsUnique();
            });

            // Configuració VoluntariAmbit
            modelBuilder.Entity<VoluntariAmbit>(entity =>
            {
                entity.ToTable("VOLU_voluntaris_ambits");
                entity.HasKey(e => e.Id);

                // Relacions
                entity.HasOne(va => va.Voluntari)
                    .WithMany(v => v.VoluntarisAmbits)
                    .HasForeignKey(va => va.VoluntariId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(va => va.AmbitVoluntari)
                    .WithMany(a => a.VoluntarisAmbits)
                    .HasForeignKey(va => va.AmbitVoluntariId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(va => va.SubambitVoluntari)
                    .WithMany(s => s.VoluntarisAmbits)
                    .HasForeignKey(va => va.SubambitVoluntariId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.VoluntariId, e.AmbitVoluntariId }).IsUnique();
            });

            // Configuració EstatVoluntari
            modelBuilder.Entity<EstatVoluntari>(entity =>
            {
                entity.ToTable("VOLU_estats_voluntaris");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(191); // Reduït per evitar problemes d'índex
                entity.Property(e => e.Descripcio).HasMaxLength(500);
                entity.Property(e => e.Color).HasMaxLength(7);
                entity.HasIndex(e => e.Nom).IsUnique();
            });

            // Configuració Voluntari
            modelBuilder.Entity<Voluntari>(entity =>
            {
                entity.ToTable("VOLU_voluntaris");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TelefonEmergencia).HasMaxLength(20);
                entity.Property(e => e.MotiuBaixa).HasMaxLength(500);
                entity.Property(e => e.Habilitats).HasMaxLength(1000);
                entity.Property(e => e.Disponibilitat).HasMaxLength(500);
                entity.Property(e => e.Observacions).HasMaxLength(1000);
                entity.Property(e => e.RutaAcordSignat).HasMaxLength(500);
                entity.Property(e => e.DataIncorporacio).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relació amb Tercer (dades personals genèriques)
                entity.HasOne(v => v.Tercer)
                    .WithMany(t => t.Voluntaris)
                    .HasForeignKey(v => v.TercerId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Relació amb EstatVoluntari
                entity.HasOne(v => v.EstatVoluntari)
                    .WithMany(e => e.Voluntaris)
                    .HasForeignKey(v => v.EstatVoluntariId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relació amb Ens
                entity.HasOne(v => v.Ens)
                    .WithMany()
                    .HasForeignKey(v => v.EnsId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relació auto-referenciada per a enllaç amb ajuntament
                entity.HasOne(v => v.EnllacAjuntament)
                    .WithMany(v => v.VoluntarisEnllacats)
                    .HasForeignKey(v => v.EnllacAjuntamentId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Relació auto-referenciada per a coordinador
                entity.HasOne(v => v.Coordinador)
                    .WithMany(v => v.VoluntarisCoordinats)
                    .HasForeignKey(v => v.CoordinadorId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Índexs per millorar el rendiment
                entity.HasIndex(v => v.Actiu);
                entity.HasIndex(v => new { v.EstatVoluntariId, v.Actiu });
            });

            // Configuració TipologiaVoluntari
            modelBuilder.Entity<TipologiaVoluntari>(entity =>
            {
                entity.ToTable("VOLU_tipologies_voluntaris");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Descripcio).HasMaxLength(1000);
                entity.HasIndex(e => e.Nom).IsUnique();
                entity.HasIndex(e => e.Actiu);
            });

            // Configuració Activitat
            modelBuilder.Entity<Activitat>(entity =>
            {
                entity.ToTable("VOLU_activitats");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Descripcio).HasMaxLength(1000);
                entity.Property(e => e.Ubicacio).HasMaxLength(500);
                entity.Property(e => e.DataCreacio).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relació amb Ens
                entity.HasOne(a => a.Ens)
                    .WithMany()
                    .HasForeignKey(a => a.EnsId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relació amb Area
                entity.HasOne(a => a.Area)
                    .WithMany()
                    .HasForeignKey(a => a.AreaId)
                    .OnDelete(DeleteBehavior.SetNull);
                    
                // Índexs per millorar el rendiment
                entity.HasIndex(a => new { a.DataInici, a.DataFi });
                entity.HasIndex(a => a.Activa);
            });

            // Configuració ActitatVoluntari (many-to-many)
            modelBuilder.Entity<ActitatVoluntari>(entity =>
            {
                entity.ToTable("VOLU_activitats_voluntaris");
                entity.HasKey(av => new { av.ActivitatId, av.VoluntariId });
                entity.Property(e => e.Observacions).HasMaxLength(500);
                entity.Property(e => e.DataInscripcio).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(av => av.Activitat)
                    .WithMany(a => a.Voluntaris)
                    .HasForeignKey(av => av.ActivitatId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(av => av.Voluntari)
                    .WithMany(v => v.Activitats)
                    .HasForeignKey(av => av.VoluntariId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Índex per millorar el rendiment
                entity.HasIndex(av => av.Actiu);
            });

            // Configuració Formacio
            modelBuilder.Entity<Formacio>(entity =>
            {
                entity.ToTable("VOLU_formacions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Descripcio).HasMaxLength(1000);
                entity.Property(e => e.Formador).HasMaxLength(255);
                entity.Property(e => e.Ubicacio).HasMaxLength(500);
                entity.Property(e => e.DataCreacio).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relació amb Ens
                entity.HasOne(f => f.Ens)
                    .WithMany()
                    .HasForeignKey(f => f.EnsId)
                    .OnDelete(DeleteBehavior.Restrict);
                    
                // Índexs per millorar el rendiment
                entity.HasIndex(f => new { f.DataInici, f.DataFi });
                entity.HasIndex(f => f.Activa);
            });

            // Configuració FormacioVoluntari (many-to-many)
            modelBuilder.Entity<FormacioVoluntari>(entity =>
            {
                entity.ToTable("VOLU_formacions_voluntaris");
                entity.HasKey(fv => new { fv.FormacioId, fv.VoluntariId });
                entity.Property(e => e.Observacions).HasMaxLength(500);
                entity.Property(e => e.DataInscripcio).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(fv => fv.Formacio)
                    .WithMany(f => f.Voluntaris)
                    .HasForeignKey(fv => fv.FormacioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(fv => fv.Voluntari)
                    .WithMany(v => v.Formacions)
                    .HasForeignKey(fv => fv.VoluntariId)
                    .OnDelete(DeleteBehavior.Cascade);
                    
                // Índexs per millorar el rendiment
                entity.HasIndex(fv => fv.Actiu);
                entity.HasIndex(fv => fv.Superat);
            });

            // ======== CONFIGURACIÓ TERMINIS ========

            // Configuració Termini
                modelBuilder.Entity<Termini>(entity =>
                {
                    entity.ToTable("SUBV_terminis");
                    entity.HasKey(e => e.Id);
                    entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                    entity.Property(e => e.Descripcio).HasMaxLength(500);
                    entity.HasIndex(e => e.Nom).IsUnique();
                });

            // Configuració TerminisSubvencio (many-to-many amb dates i notes)
            modelBuilder.Entity<TerminisSubvencio>(entity =>
            {
                entity.ToTable("SUBV_terminis_subvencions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                // Relació amb Subvencio_
                entity.HasOne(ts => ts.Subvencio)
                    .WithMany(s => s.TerminisSubvencions)
                    .HasForeignKey(ts => ts.SubvencioId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relació amb Termini
                entity.HasOne(ts => ts.Termini)
                    .WithMany(t => t.TerminisSubvencions)
                    .HasForeignKey(ts => ts.TerminiId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Índexs per millorar el rendiment
                entity.HasIndex(ts => ts.SubvencioId);
                entity.HasIndex(ts => ts.TerminiId);
                entity.HasIndex(ts => new { ts.DataInici, ts.DataFi });
            });

            // Configuració ConvenisLocals
            modelBuilder.Entity<ConvenisLocals>(entity =>
            {
                entity.ToTable("CONV_convenis_locals");
                entity.HasKey(c => c._Id);
                // Ignorar propietats de la classe base que no existeixen a la taula CONV_convenis_locals
                entity.Ignore(c => c.Id);
                entity.Ignore(c => c.Nom);
                entity.Ignore(c => c.Descripcio);
                entity.Ignore(c => c.DataInici);
                entity.Ignore(c => c.DataFi);
                entity.Ignore(c => c.Municipi);
                entity.Ignore(c => c.CODI_ENS);
            });

            // Configuració ConvenisAuxiliars
            modelBuilder.Entity<ConvenisAuxiliars>(entity =>
            {
                entity.ToTable("CONV_convenis_auxiliars");
                entity.HasKey(c => c.Id);
            });

            // ======== DOCUMENTS (polimòrfics) ========
            modelBuilder.Entity<TipusDocument>(entity =>
            {
                entity.ToTable("Aux_TipusDocument");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descripcio).HasMaxLength(500);
                entity.HasIndex(e => e.Nom).IsUnique();
            });

            modelBuilder.Entity<Tramit>(entity =>
            {
                entity.ToTable("Aux_Tramit");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codi).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(191);
                entity.Property(e => e.Descripcio).HasColumnType("text");
                entity.HasIndex(e => e.Codi).IsUnique();
                entity.HasIndex(e => e.Nom).IsUnique();
            });

            modelBuilder.Entity<TramitTipusDocument>(entity =>
            {
                entity.ToTable("Rel_TramitTipusDocument");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Ordre).HasDefaultValue(100);

                entity.HasIndex(e => new { e.TramitId, e.TipusDocumentId })
                    .IsUnique()
                    .HasDatabaseName("uq_rel_tramit_tipusdocument");

                entity.HasOne(e => e.Tramit)
                    .WithMany()
                    .HasForeignKey(e => e.TramitId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.TipusDocument)
                    .WithMany()
                    .HasForeignKey(e => e.TipusDocumentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("Documents");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityId).IsRequired();
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PathMinio).IsRequired().HasMaxLength(500);
                entity.Property(e => e.MimeType).HasMaxLength(100);
                entity.Property(e => e.Observacions).HasColumnType("text");
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.CreatedBy).HasColumnName("created_by");
                entity.Property(e => e.UpdatedBy).HasColumnName("updated_by");

                entity.HasIndex(e => new { e.EntityType, e.EntityId })
                    .HasDatabaseName("idx_doc_entity");

                entity.HasOne(d => d.TipusDocument)
                    .WithMany()
                    .HasForeignKey(d => d.TipusDocumentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.CreatedBy)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.UpdatedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ======== ARMES ========
            modelBuilder.Entity<AuxTipusArma>(entity =>
            {
                entity.ToTable("Aux_TipusArma");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Nom).IsUnique();
            });

            modelBuilder.Entity<AuxTipusLicencia>(entity =>
            {
                entity.ToTable("Aux_TipusLicencia");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Nom).IsUnique();
            });

            modelBuilder.Entity<AuxEstatLicencia>(entity =>
            {
                entity.ToTable("Aux_EstatLicencia");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Nom).IsUnique();
            });

            modelBuilder.Entity<AuxResultatInspeccio>(entity =>
            {
                entity.ToTable("Aux_ResultatInspeccio");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Nom).IsUnique();
            });

            modelBuilder.Entity<Arma>(entity =>
            {
                entity.ToTable("ARMES_armes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumSerie).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Marca).HasMaxLength(100);
                entity.Property(e => e.Model).HasMaxLength(100);
                entity.Property(e => e.Calibre).HasMaxLength(50);
                entity.Property(e => e.MotiuBaixa).HasMaxLength(500);
                entity.Property(e => e.Observacions).HasColumnType("text");
                entity.HasIndex(e => e.NumSerie).IsUnique();

                entity.HasOne(e => e.TipusArma)
                    .WithMany()
                    .HasForeignKey(e => e.TipusArmaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Tercer)
                    .WithMany()
                    .HasForeignKey(e => e.TercerId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Ens)
                    .WithMany()
                    .HasForeignKey(e => e.EnsId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.UpdatedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ArmaLlicencia>(entity =>
            {
                entity.ToTable("ARMES_licencies");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumLicencia).HasMaxLength(100);
                entity.Property(e => e.Observacions).HasColumnType("text");

                entity.HasOne(e => e.Arma)
                    .WithMany()
                    .HasForeignKey(e => e.ArmaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Tercer)
                    .WithMany()
                    .HasForeignKey(e => e.TercerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.TipusLicencia)
                    .WithMany()
                    .HasForeignKey(e => e.TipusLicenciaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EstatLicencia)
                    .WithMany()
                    .HasForeignKey(e => e.EstatLicenciaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.UpdatedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<ArmaInspeccio>(entity =>
            {
                entity.ToTable("ARMES_inspeccions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Observacions).HasColumnType("text");

                entity.HasOne(e => e.Arma)
                    .WithMany()
                    .HasForeignKey(e => e.ArmaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Resultat)
                    .WithMany()
                    .HasForeignKey(e => e.ResultatId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.CreatedBy)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.UpdatedBy)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ======== CONFIGURACIÓ PATRIMONI ========
            ConfigurePatrimoni(modelBuilder);
            ConfigurePatrimoniUbicacions(modelBuilder);
            ConfigurePatrimoniArxius(modelBuilder);
            ConfigurePatrimoniInspeccions(modelBuilder);
            ConfigurePatrimoniEnums(modelBuilder);
        }

        static void ConfigurePatrimoni(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Patrimoni>(entity =>
            {
                // Índex únic per codi d'inventari
                entity.HasIndex(p => p.CodiInventari).IsUnique();

                // Configuració de camps específics
                entity.Property(p => p.ValorTotal).HasColumnType("decimal(3,2)");
                entity.Property(p => p.PreuEntrada).HasColumnType("decimal(8,2)");

                // Configuració de relacions
                entity.HasOne(p => p.TipusPatrimoni)
                    .WithMany(t => t.Patrimonis)
                    .HasForeignKey(p => p.TipusPatrimoniId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.EstatConservacio)
                    .WithMany(e => e.Patrimonis)
                    .HasForeignKey(p => p.EstatConservacioId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(p => p.Ubicacio)
                    .WithMany(u => u.Patrimonis)
                    .HasForeignKey(p => p.UbicacioId)
                    .OnDelete(DeleteBehavior.SetNull);

                // Relació amb identificadors externs
                entity.HasMany(p => p.Identificadors)
                    .WithOne(i => i.Patrimoni)
                    .HasForeignKey(i => i.PatrimoniId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.NotesPatrimoni)
                    .WithOne(n => n.Patrimoni)
                    .HasForeignKey(n => n.PatrimoniId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<NotaPatrimoni>(entity =>
            {
                entity.HasOne(n => n.TipusNota)
                    .WithMany(t => t.Notes)
                    .HasForeignKey(n => n.TipusNotaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        static void ConfigurePatrimoniUbicacions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ubicacio>(entity =>
            {
                // Auto-referència per jerarquia
                entity.HasOne(u => u.Parent)
                      .WithMany(u => u.Children)
                      .HasForeignKey(u => u.ParentId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }

        static void ConfigurePatrimoniArxius(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ArxiuPatrimoni>(entity =>
            {
                entity.HasOne(a => a.Patrimoni)
                      .WithMany(p => p.Arxius)
                      .HasForeignKey(a => a.PatrimoniId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.TipusArxiu)
                      .WithMany(t => t.Arxius)
                      .HasForeignKey(a => a.TipusArxiuId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Colleccio)
                      .WithMany(c => c.Arxius)
                      .HasForeignKey(a => a.ColleccioId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(a => a.Localitzacio)
                      .WithMany(l => l.Arxius)
                      .HasForeignKey(a => a.LocalitzacioId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(a => a.Tags)
                      .WithMany(t => t.Arxius)
                      .UsingEntity<ArxiuTag>(
                          j => j.HasOne<Tag>().WithMany().HasForeignKey(at => at.TagId).OnDelete(DeleteBehavior.Cascade),
                          j => j.HasOne<ArxiuPatrimoni>().WithMany().HasForeignKey(at => at.ArxiuId).OnDelete(DeleteBehavior.Cascade));
            });
        }

        static void ConfigurePatrimoniInspeccions(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Inspeccio>(entity =>
            {
                entity.HasOne(i => i.Patrimoni)
                      .WithMany(p => p.Inspeccions)
                      .HasForeignKey(i => i.PatrimoniId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Intervencio>(entity =>
            {
                entity.HasOne(i => i.Patrimoni)
                      .WithMany(p => p.Intervencions)
                      .HasForeignKey(i => i.PatrimoniId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Inspeccio)
                      .WithMany(ins => ins.Intervencions)
                      .HasForeignKey(i => i.InspeccioId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.Property(i => i.PressupostAprovat).HasColumnType("decimal(12,2)");
                entity.Property(i => i.CostReal).HasColumnType("decimal(12,2)");
            });
        }

        static void ConfigurePatrimoniEnums(ModelBuilder modelBuilder)
        {
            // Configuració d'enums com a strings
            modelBuilder.Entity<Ubicacio>()
                .Property(u => u.Tipus)
                .HasConversion<string>();

            modelBuilder.Entity<Patrimoni>()
                .Property(p => p.ProteccioLegal)
                .HasConversion<string>();

            var regimConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<RegimPropietat, string>(
                v => v == RegimPropietat.Public ? "públic"
                    : v == RegimPropietat.Privat ? "privat"
                    : v == RegimPropietat.Mixte ? "mixte"
                    : "desconegut",
                v => v == "públic" ? RegimPropietat.Public
                    : v == "privat" ? RegimPropietat.Privat
                    : v == "mixte" ? RegimPropietat.Mixte
                    : RegimPropietat.Desconegut
            );
            modelBuilder.Entity<Patrimoni>()
                .Property(p => p.RegimPropietat)
                .HasConversion(regimConverter);

            modelBuilder.Entity<Inspeccio>()
                .Property(i => i.TipusInspeccio)
                .HasConversion<string>();

            modelBuilder.Entity<Intervencio>()
                .Property(i => i.TipusIntervencio)
                .HasConversion<string>();

            modelBuilder.Entity<Intervencio>()
                .Property(i => i.EstatIntervencio)
                .HasConversion<string>();

            // ======== CURSETS ========

            modelBuilder.Entity<TipusCurset>(entity =>
            {
                entity.ToTable("curs_tipus_cursets");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CodiLiquidacio).HasMaxLength(16);
                entity.HasIndex(e => e.Nom).IsUnique();
            });

            modelBuilder.Entity<Alumne>(entity =>
            {
                entity.ToTable("curs_alumnes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Notes).HasMaxLength(1000);

                entity.HasOne(a => a.Tercer)
                    .WithMany()
                    .HasForeignKey(a => a.TercerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(a => a.TercerId).IsUnique();
                entity.HasIndex(a => a.Actiu);
            });

            modelBuilder.Entity<Curset>(entity =>
            {
                entity.ToTable("curs_cursets");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nom).IsRequired().HasMaxLength(255);

                entity.HasOne(c => c.TipusCurset)
                    .WithMany(t => t.Cursets)
                    .HasForeignKey(c => c.TipusCursetId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Professora)
                    .WithMany()
                    .HasForeignKey(c => c.ProfessoraId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Regidor)
                    .WithMany()
                    .HasForeignKey(c => c.RegidorId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.Property(c => c.PreuPerSessioEmpadronat).HasColumnType("decimal(8,2)");
                entity.Property(c => c.PreuPerSessioNoEmpadronat).HasColumnType("decimal(8,2)");
                entity.Property(c => c.CodiLiquidacio).HasMaxLength(16);
                entity.Property(c => c.CadenciaLiquidacio).HasMaxLength(20);
                entity.Property(c => c.OrdenancaLiquidacio).HasMaxLength(400);
                entity.Property(c => c.InscripcioInici).HasColumnType("date");
                entity.Property(c => c.InscripcioFi).HasColumnType("date");
                entity.Property(c => c.UrlInscripcio).HasMaxLength(500);
                entity.Property(c => c.SorteigData).HasColumnType("date");

                entity.HasIndex(c => c.TipusCursetId);
                entity.HasIndex(c => c.ProfessoraId);
                entity.HasIndex(c => c.RegidorId);
                entity.HasIndex(c => c.Actiu);
            });

            modelBuilder.Entity<AlumneCurset>(entity =>
            {
                entity.ToTable("curs_alumnes_cursets");
                entity.HasKey(ac => new { ac.AlumneId, ac.CursetId });

                entity.HasOne(ac => ac.Alumne)
                    .WithMany(a => a.Cursets)
                    .HasForeignKey(ac => ac.AlumneId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ac => ac.Curset)
                    .WithMany(c => c.Alumnes)
                    .HasForeignKey(ac => ac.CursetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(ac => ac.DataAlta).HasColumnType("date");
                entity.Property(ac => ac.DataBaixa).HasColumnType("date");
                entity.Property(ac => ac.DataSollicitud).HasColumnType("date");
                entity.Property(ac => ac.Estat).HasConversion<string>().HasMaxLength(20);
                entity.Property(ac => ac.Origen).HasMaxLength(10);
                entity.HasIndex(ac => new { ac.CursetId, ac.Estat });
            });

            modelBuilder.Entity<Sessio>(entity =>
            {
                entity.ToTable("curs_sessions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Data).HasColumnType("date");
                entity.Property(e => e.NotaSessio).HasMaxLength(1000);
                entity.Property(e => e.Estat).HasConversion<int>();

                entity.HasOne(s => s.Curset)
                    .WithMany(c => c.Sessions)
                    .HasForeignKey(s => s.CursetId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Professora)
                    .WithMany()
                    .HasForeignKey(s => s.ProfessoraId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(s => new { s.CursetId, s.Data }).IsUnique();
            });

            modelBuilder.Entity<Assistencia>(entity =>
            {
                entity.ToTable("curs_assistencies");
                entity.HasKey(a => new { a.SessioId, a.AlumneId });
                entity.Property(a => a.Nota).HasMaxLength(500);

                entity.HasOne(a => a.Sessio)
                    .WithMany(s => s.Assistencies)
                    .HasForeignKey(a => a.SessioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Alumne)
                    .WithMany(al => al.Assistencies)
                    .HasForeignKey(a => a.AlumneId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<LiquidacioRemesa>(entity =>
            {
                entity.ToTable("curs_liquidacions_remeses");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Descripcio).HasMaxLength(200);
                entity.Property(e => e.CadenciaTipus).HasMaxLength(20);
                entity.Property(e => e.CodiPeriode).HasMaxLength(16);
                entity.Property(e => e.PeriodeEtiqueta).HasMaxLength(60);
                entity.Property(e => e.Expedient).HasMaxLength(60);
                entity.Property(e => e.PeriodeInici).HasColumnType("date");
                entity.Property(e => e.PeriodeFi).HasColumnType("date");
                entity.Property(e => e.ImportTotal).HasColumnType("decimal(12,2)");
            });

            modelBuilder.Entity<Liquidacio>(entity =>
            {
                entity.ToTable("curs_liquidacions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Numero).IsRequired().HasMaxLength(40);
                entity.Property(e => e.CodiServei).HasMaxLength(16);
                entity.Property(e => e.CodiPeriode).HasMaxLength(16);
                entity.Property(e => e.CodiCurset).HasMaxLength(16);
                entity.Property(e => e.PeriodeEtiqueta).HasMaxLength(60);
                entity.Property(e => e.PeriodeInici).HasColumnType("date");
                entity.Property(e => e.PeriodeFi).HasColumnType("date");
                entity.Property(e => e.PreuPerSessio).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Import).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Estat).HasConversion<string>().HasMaxLength(20);
                entity.Property(e => e.MotiuAnullacio).HasMaxLength(255);
                entity.Property(e => e.AlumneNomComplet).HasMaxLength(200);
                entity.Property(e => e.AlumneNif).HasMaxLength(20);
                entity.Property(e => e.AlumneAdreca).HasMaxLength(300);
                entity.Property(e => e.CursetTitol).HasMaxLength(200);
                entity.Property(e => e.ConcepteText).HasMaxLength(300);
                entity.Property(e => e.OrdenancaText).HasMaxLength(400);
                entity.Property(e => e.DatesHorariText).HasMaxLength(200);

                entity.HasIndex(e => e.Numero).IsUnique();
                entity.HasIndex(e => e.RemesaId);
                entity.HasIndex(e => e.AlumneId);
                entity.HasIndex(e => e.CursetId);
                entity.HasIndex(e => new { e.CodiPeriode, e.Serie });
                entity.HasIndex(e => e.Estat);

                entity.HasOne(e => e.Remesa)
                    .WithMany(r => r.Liquidacions)
                    .HasForeignKey(e => e.RemesaId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Alumne)
                    .WithMany()
                    .HasForeignKey(e => e.AlumneId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Curset)
                    .WithMany()
                    .HasForeignKey(e => e.CursetId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<LiquidacioConfig>(entity =>
            {
                entity.ToTable("curs_liquidacions_config");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.ExpedientPatro).HasMaxLength(120);
                entity.Property(e => e.ConceptePatro).HasMaxLength(300);
                entity.Property(e => e.CapcaleraMunicipi).HasMaxLength(200);
                entity.Property(e => e.OrdenancaTarifa).HasColumnType("text");
                entity.Property(e => e.EntitatsColaboradoresText).HasColumnType("text");
                entity.Property(e => e.MitjansPagamentText).HasColumnType("text");
                entity.Property(e => e.OficinaCobratoriaText).HasColumnType("text");
                entity.Property(e => e.TextRecursos).HasColumnType("text");
                entity.Property(e => e.TextImportant).HasColumnType("text");
                entity.Property(e => e.TextTerminis).HasColumnType("text");
            });

            modelBuilder.Entity<EmailAbsenciesConfig>(entity =>
            {
                entity.ToTable("curs_email_absencies_config");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedNever();
                entity.Property(e => e.SmtpHost).HasMaxLength(200);
                entity.Property(e => e.SmtpUser).HasMaxLength(200);
                entity.Property(e => e.SmtpPassword).HasMaxLength(200);
                entity.Property(e => e.RemitentEmail).HasMaxLength(200);
                entity.Property(e => e.RemitentNom).HasMaxLength(200);
                entity.Property(e => e.Assumpte).HasMaxLength(300);
                entity.Property(e => e.CosHtml).HasColumnType("text");
            });

            modelBuilder.Entity<LiquidacioComptador>(entity =>
            {
                entity.ToTable("curs_liquidacions_comptador");
                entity.HasKey(e => e.Clau);
                entity.Property(e => e.Clau).HasMaxLength(24);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Capturar els canvis abans de guardar (UPDATE/DELETE) i guardar referències INSERT
                var (auditEntriesBeforeSave, insertEntities) = await OnBeforeSaveChanges();

                // Guardar canvis - aquí es generen els Ids
                var result = await base.SaveChangesAsync(cancellationToken);

                // Crear audits INSERT després del save amb RecordId correcte
                var insertAudits = await CreateInsertAudits(insertEntities);

                // Combinar tots els audits
                var allAudits = new List<AuditLog>();
                allAudits.AddRange(auditEntriesBeforeSave);
                allAudits.AddRange(insertAudits);

                // Guardar els logs d'auditoria
                if (allAudits.Any())
                {
                    await SaveAuditLogs(allAudits);
                }

                return result;
            }
            catch (Exception ex)
            {
                // Intentar guardar l'error a la taula logs
                try
                {
                    var log = new Log
                    {
                        Timestamp = DateTime.UtcNow,
                        Level = "Fatal",
                        Message = $"Error en SaveChangesAsync: {ex.Message}",
                        Exception = ex.ToString(),
                        UserId = _httpContextAccessor?.HttpContext?.User?.Identity?.Name,
                        Action = "SaveChangesAsync",
                        IpAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                        SessionId = null
                    };

                    // Crear un nou context per evitar recursivitat
                    using var logContext = new GestorSubvencionsContext(new DbContextOptionsBuilder<GestorSubvencionsContext>()
                        .UseMySql(Database.GetConnectionString(), ServerVersion.AutoDetect(Database.GetConnectionString()))
                        .Options);
                    
                    logContext.Logs.Add(log);
                    await logContext.Database.ExecuteSqlRawAsync(
                        "INSERT INTO logs (timestamp, level, message, exception, user_id, action, ip_address) VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                        new object[] { log.Timestamp, log.Level, log.Message, log.Exception is null ? DBNull.Value : log.Exception, log.UserId is null ? DBNull.Value : log.UserId, log.Action is null ? DBNull.Value : log.Action, log.IpAddress is null ? DBNull.Value : log.IpAddress });
                }
                catch
                {
                    // Si falla guardar a logs, almenys escriure a consola
                    Console.WriteLine($"[SaveChanges ERROR] {ex.Message}");
                }
                
                throw; // Re-throw per mantenir el comportament original
            }
        }

        async Task<List<AuditLog>> CreateInsertAudits(List<(EntityEntry entry, Dictionary<string, object?> values)> insertEntities)
        {
            var insertAudits = new List<AuditLog>();
            var userId = _httpContextAccessor?.HttpContext?.User?.Identity?.Name;

            foreach (var (entry, newValues) in insertEntities)
            {
                try
                {
                    // Obtenir RecordId ara que l'entitat té Id generat
                    var keyProps = Entry(entry.Entity).Properties
                        .Where(p => p.Metadata.IsPrimaryKey())
                        .ToList();

                    string? recordId = null;
                    if (keyProps.Any())
                    {
                        var keyValues = keyProps
                            .Select(p => p.CurrentValue?.ToString() ?? "")
                            .Where(v => !string.IsNullOrEmpty(v) && v != "0")
                            .ToList();

                        if (keyValues.Any())
                        {
                            recordId = string.Join(",", keyValues);
                        }
                    }

                    var auditLog = new AuditLog
                    {
                        TableName = entry.Entity.GetType().Name,
                        Action = "INSERT",
                        RecordId = recordId ?? "",
                        OldValues = null,
                        NewValues = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null,
                        UserId = userId,
                        Timestamp = DateTime.UtcNow,
                        IpAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                        SessionId = GetSessionIdSafe()
                    };

                    insertAudits.Add(auditLog);
                }
                catch
                {
                    // Si hi ha error, saltar aquesta entitat
                }
            }

            return insertAudits;
        }

        async Task<(List<AuditLog> auditEntries, List<(EntityEntry entry, Dictionary<string, object?> values)> insertEntities)> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditLog>();
            var insertEntities = new List<(EntityEntry entry, Dictionary<string, object?> values)>();

            // Obtenir informació de l'usuari si està disponible
            var userId = _httpContextAccessor?.HttpContext?.User?.Identity?.Name;

            foreach (var entry in ChangeTracker.Entries())
            {
                // Ignorar entitats de logging per evitar recursivitat
                if (entry.Entity is Log || entry.Entity is AuditLog)
                    continue;

                // Només processar Added, Modified, Deleted
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                // Per operacions INSERT, només guardar la referència i els valors
                if (entry.State == EntityState.Added)
                {
                    var newValues = new Dictionary<string, object?>();
                    foreach (var prop in entry.Properties)
                    {
                        newValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    insertEntities.Add((entry, newValues));
                    continue; // No crear l'audit ara, es crearà després del save
                }

                // Per UPDATE i DELETE, crear l'audit ara amb RecordId existent
                var auditLog = new AuditLog
                {
                    TableName = entry.Entity.GetType().Name,
                    Action = entry.State == EntityState.Modified ? "UPDATE" : "DELETE",
                    Timestamp = DateTime.UtcNow,
                    UserId = userId,
                    RecordId = "",
                    IpAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                    SessionId = GetSessionIdSafe()
                };

                // Obtenir la clau primària existent
                var keyValues = entry.Properties
                    .Where(p => p.Metadata.IsPrimaryKey())
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue?.ToString() ?? "");

                auditLog.RecordId = keyValues.Count > 0 
                    ? string.Join(",", keyValues.Values) 
                    : "";

                // Capturar valors anteriors (només per Modified i propietats que han canviat, o tots per Deleted)
                if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
                {
                    var oldValues = new Dictionary<string, object?>();
                    foreach (var prop in entry.Properties)
                    {
                        if (entry.State == EntityState.Deleted)
                        {
                            // Per DELETE, guardar tots els valors
                            oldValues[prop.Metadata.Name] = prop.OriginalValue;
                        }
                        else if (prop.IsModified)
                        {
                            // Per UPDATE, només propietats modificades
                            oldValues[prop.Metadata.Name] = prop.OriginalValue;
                        }
                    }
                    auditLog.OldValues = oldValues.Count > 0 
                        ? JsonSerializer.Serialize(oldValues) 
                        : null;
                }

                // Capturar valors nous (només per Modified, no per Added que ja s'ha processat)
                if (entry.State == EntityState.Modified)
                {
                    var newValues = new Dictionary<string, object?>();
                    foreach (var prop in entry.Properties)
                    {
                        if (prop.IsModified)
                        {
                            newValues[prop.Metadata.Name] = prop.CurrentValue;
                        }
                    }
                    auditLog.NewValues = newValues.Count > 0 
                        ? JsonSerializer.Serialize(newValues) 
                        : null;
                    
                    // Només afegir l'audit si realment hi ha canvis
                    if (newValues.Count == 0)
                        continue;
                }

                auditEntries.Add(auditLog);
            }

            return (auditEntries, insertEntities);
        }

        async Task SaveAuditLogs(List<AuditLog> auditEntries)
        {
            try
            {
                // Afegir els logs d'auditoria al context
                await AuditLogs.AddRangeAsync(auditEntries);
                
                // Guardar sense cridar SaveChangesAsync per evitar recursivitat
                await base.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Si falla l'auditoria, escriure a la consola però no fallar la transacció principal
                Console.WriteLine($"[AUDIT ERROR] Failed to save audit logs: {ex.Message}");
                
                // Intentar guardar l'error a la taula logs
                try
                {
                                        var userName = _httpContextAccessor?.HttpContext?.User?.Identity?.Name;
                                        var ipAddress = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                    await Database.ExecuteSqlRawAsync(
                        @"INSERT INTO logs (timestamp, level, message, exception, user_id, action, ip_address) 
                          VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
                        new object[] {
                            DateTime.UtcNow,
                            "Error",
                            $"Failed to save audit logs: {ex.Message}",
                            ex.ToString(),
                                                        (object?)userName ?? DBNull.Value,
                            "SaveAuditLogs",
                                                        (object?)ipAddress ?? DBNull.Value
                        });
                }
                catch
                {
                    // Si també falla guardar a logs, només queda consola
                }
                
                // Netejar el ChangeTracker per evitar que els audits fallits segueixin en memòria
                foreach (var entry in ChangeTracker.Entries<AuditLog>())
                {
                    entry.State = EntityState.Detached;
                }
            }
        }

        string? GetSessionIdSafe()
        {
            try
            {
                var httpContext = _httpContextAccessor?.HttpContext;
                if (httpContext == null) return null;

                // Comprovar si la feature de sessió és disponible ABANS d'accedir-hi
                // per evitar InvalidOperationException quan Session no està configurat
                var sessionFeature = httpContext.Features
                    .Get<Microsoft.AspNetCore.Http.Features.ISessionFeature>();
                if (sessionFeature?.Session == null || !sessionFeature.Session.IsAvailable)
                    return null;

                return httpContext.Session.Id;
            }
            catch
            {
                return null;
            }
        }    }
}
