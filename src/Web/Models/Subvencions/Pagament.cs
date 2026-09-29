using System.ComponentModel.DataAnnotations.Schema;

namespace AppAjuntament.Models.Subvencions
{
    [Table("SUBV_pagaments")]
    public class Pagament
    {
        public int Id { get; set; }
        public int SubvencioId { get; set; }
        public Subvencio_? Subvencio { get; set; }

        // Amount information
        public decimal? Amount { get; set; }
        public string? Currency { get; set; } = "€";

        // Propietat no mapejada per a la formatació de l'import
        [NotMapped]
        public string AmountFormatted { get; set; } = string.Empty;

        // Date information
        public DateTime? Date { get; set; }

        // Additional information
        public string? Comment { get; set; }
        public int? PaymentPercentage { get; set; }
        public bool IsNoPayment { get; set; }
        public string? ServiceDescription { get; set; }
        public bool IsGrantDate { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
