using System.Collections.Generic;

namespace EMAP.Domain.Fyp
{
    public class FypEvaluationMember
    {
        public int Id { get; set; }

        public int EvaluationId { get; set; }
        public FypEvaluation Evaluation { get; set; } = default!;

        public string StudentUserId { get; set; } = string.Empty;

        public string StudentName { get; set; } = string.Empty;

        public string? RegistrationNo { get; set; }

        public decimal TotalMarks { get; set; }

        public decimal WeightedMarks { get; set; }

        public string? Remarks { get; set; }

        public ICollection<FypEvaluationMemberScore> Scores { get; set; } = new List<FypEvaluationMemberScore>();
    }
}
