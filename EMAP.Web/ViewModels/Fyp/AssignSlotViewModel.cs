using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EMAP.Web.ViewModels.Fyp
{
    public class AssignSlotViewModel
    {
        public int ProposalId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string GroupLeader { get; set; } = string.Empty;

        public string Batch { get; set; } = string.Empty;
        public string SupervisorName { get; set; } = string.Empty;

        public DateTime DefenseDate { get; set; }

        public string SelectedSlot { get; set; } = string.Empty;
        public int DurationMinutes { get; set; } = 10;

        public string? Venue { get; set; }
        public string? Instructions { get; set; }

        public List<SelectListItem> AvailableSlots { get; set; }
            = new List<SelectListItem>();
    }
}
