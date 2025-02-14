using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ForQab.DataAccess.Models;

[Keyless]
[Table("Exam_SubCommissions")]
public partial class ExamSubCommission
{
    [Column("Exam_Id")]
    public int ExamId { get; set; }

    [Column("SubCommission_Id")]
    public int SubCommissionId { get; set; }
}
