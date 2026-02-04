using EMAP.Domain.Fyp;

namespace EMAP.Web.ViewModels.Fyp
{
    public class SubmitProposalViewModel
    {
        public int GroupId { get; set; }

        public string SupervisorName { get; set; } = "";

        public ProposalStatus? ExistingStatus { get; set; }

        public string? ExistingFeedback { get; set; }

        public string Title { get; set; } = "";
    }
}
