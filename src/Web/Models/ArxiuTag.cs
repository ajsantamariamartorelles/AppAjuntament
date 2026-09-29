using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AppAjuntament.Models;

[Table("PATR_arxius_tags")]
[PrimaryKey(nameof(ArxiuId), nameof(TagId))]
public class ArxiuTag
{
    [Column("arxiu_id")]
    public int ArxiuId { get; set; }

    [Column("tag_id")]
    public int TagId { get; set; }
}
