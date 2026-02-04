using EMAP.Domain.Fyp;
using System.Collections.Generic;

namespace EMAP.Web.ViewModels
{
    public class SupervisorDashboardViewModel
    {
        public int PendingRequests { get; set; }
        public int ActiveGroups { get; set; }

        public int PendingProposals { get; set; }
        public int ChangesRequestedProposals { get; set; }
        public int ApprovedForDefenseProposals { get; set; }

        public IList<StudentGroup> Groups { get; set; } = new List<StudentGroup>();
        public IList<ProposalSubmission> RecentProposals { get; set; } = new List<ProposalSubmission>();
    }
}
