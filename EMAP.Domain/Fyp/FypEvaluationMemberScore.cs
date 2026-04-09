namespace EMAP.Domain.Fyp
{
    public class FypEvaluationMemberScore
    {
        public int Id { get; set; }

        public int EvaluationMemberId { get; set; }
        public FypEvaluationMember EvaluationMember { get; set; } = default!;

        public int CriterionId { get; set; }
        public FypEvaluationCriterion Criterion { get; set; } = default!;

        public decimal AwardedMarks { get; set; }

        public string? Comment { get; set; }
    }
}
