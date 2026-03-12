using System;
using EMAP.Domain.Fyp;

namespace EMAP.Web.ViewModels.Fyp
{
    public class FypStudentMilestoneViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public FypStage Stage { get; set; }
        public FypMilestoneType Type { get; set; }
        public int? ChapterNumber { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsOptional { get; set; }
        public bool IsActive { get; set; }

        public bool IsCompleted { get; set; }
        public string StatusText { get; set; } = "Pending";
    }
}