using System.ComponentModel.DataAnnotations;

namespace EMAP.Domain.Fyp
{
    public class FypFinalRecordEvaluationMember
    {
        public int Id { get; set; }

        public int FinalRecordEvaluationId { get; set; }
        public FypFinalRecordEvaluation FinalRecordEvaluation { get; set; } = null!;

        public string StudentUserId { get; set; } = string.Empty;

        [StringLength(200)]
        public string StudentName { get; set; } = string.Empty;

        [StringLength(100)]
        public string RegistrationNo { get; set; } = string.Empty;

        public decimal TotalMarks { get; set; }

        public decimal WeightedMarks { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }
    }
}
