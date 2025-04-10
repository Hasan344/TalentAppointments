using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ForQab.DataAccess.Models;
using Monitor = ForQab.DataAccess.Models.Monitor;

namespace ForQab.Models;

[Table("monitors_professions")]
public partial class MonitorsProfession
{
    [Key]
    [Column("Monitor_id")]
    public int MonitorId { get; set; }

    [Key]
    [Column("sub_profession_id")]
    public int SubProfessionId { get; set; }

    [ForeignKey("MonitorId")]
    [InverseProperty("MonitorsProfessions")] 
    public virtual Monitor Monitor { get; set; } = null!;

    [ForeignKey("SubProfessionId")]
    [InverseProperty("MonitorsProfessions")]
    public virtual SubProfession SubProfession { get; set; } = null!;

}
