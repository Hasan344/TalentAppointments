using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models;

[Table("Financial_Rates")]
public partial class FinancialRate
{
    [Key]
    [Column("Id")]
    public int Id { get; set; }

    [Column("Article")]
    [StringLength(20)]
    public string Article { get; set; } = null!;
    [Column(TypeName = "decimal(18,2)")]
    public decimal Rate { get; set; }
}