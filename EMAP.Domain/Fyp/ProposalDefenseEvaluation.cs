using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAP.Domain.Fyp
{
    public class ProposalDefenseEvaluation
    {
        public int Id { get; set; }

        public int ProposalSubmissionId { get; set; }
        public ProposalSubmission ProposalSubmission { get; set; } = null!;

        public bool IsPresent { get; set; } = true;

        public DefenseDecision Decision { get; set; } = DefenseDecision.Accepted;

        public string? Feedback { get; set; }

        public DateTime EvaluatedAt { get; set; } = DateTime.UtcNow;

        public string EvaluatedById { get; set; } = string.Empty;
    }
}

