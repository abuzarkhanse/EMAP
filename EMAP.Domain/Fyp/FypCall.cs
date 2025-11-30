namespace EMAP.Domain.Fyp;

public class FypCall
{
    public int Id { get; set; }
    public string Batch { get; set; } = string.Empty;       // e.g. "F22"
    public string Title { get; set; } = "FYP Call";
    public DateTime AnnouncementDate { get; set; }
    public DateTime ProposalDeadline { get; set; }
    public bool IsActive { get; set; } = true;
}
