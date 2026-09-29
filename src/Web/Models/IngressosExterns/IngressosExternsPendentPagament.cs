using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.IngressosExterns;

[Table("ingressos_externs_pendents_pagament")]
public class IngressosExternsPendentPagament
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required, MaxLength(60), Column("tipus_ajut")]
    public string TipusAjut { get; set; } = string.Empty;

    [Column("exercici")]
    public int Exercici { get; set; }

    [Column("import", TypeName = "decimal(18,2)")]
    public decimal Import { get; set; }

    [Column("retingut")]
    public bool Retingut { get; set; }

    [Column("data_publicacio")]
    public DateTime? DataPublicacio { get; set; }

    [Column("sincronitzat_utc")]
    public DateTime SincronitzatUtc { get; set; }
}
