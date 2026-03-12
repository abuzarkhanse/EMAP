using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAP.Domain.Fyp
{
    public class FypEvaluationScore
    {
        public int Id { get; set; }

        public int EvaluationId { get; set; }
        public FypEvaluation Evaluation { get; set; } = default!;

        public int CriterionId { get; set; }
        public FypEvaluationCriterion Criterion { get; set; } = default!;

        public decimal AwardedMarks { get; set; }

        public string? Comment { get; set; }
    }
}