using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForQab.DataAccess.Models
{
    [Table("assignment_seed_log")]
    public class AssignmentSeedLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("exam_id")]
        public int ExamId { get; set; }

        // 1 = Ekspert, 2 = Nəzarətçi, 3 = İmtahan rəhbəri
        [Column("assignment_type")]
        public byte AssignmentType { get; set; }

        [Column("seed1")] public int Seed1 { get; set; }
        [Column("seed2")] public int Seed2 { get; set; }
        [Column("seed3")] public int Seed3 { get; set; }
        [Column("seed4")] public int Seed4 { get; set; }

        // Replay üçün: tam ekvivalent nəticə vermək üçün neçə nəfər götürülməli idi
        [Column("number_requested")]
        public int NumberRequested { get; set; }

        // Audit: federation, subprofessions, gender, maxDate, roomId (sərbəst mətn / JSON)
        [Column("parameters")]
        [StringLength(1000)]
        public string? Parameters { get; set; }

        // Atama anındakı süzülmüş aday hovuzu (snapshot): "id:count,id:count,..."
        [Column("candidate_pool")]
        public string? CandidatePool { get; set; }

        // Deterministik sıraya görə təyin edilənlər (əsas + ehtiyat): "id,id,..."
        [Column("selected_ids")]
        public string? SelectedIds { get; set; }

        [Column("user_name")]
        [StringLength(256)]
        public string? UserName { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}