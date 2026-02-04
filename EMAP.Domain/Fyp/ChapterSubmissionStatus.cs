using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAP.Domain.Fyp
{
    public enum ChapterSubmissionStatus
    {
        Submitted = 1,           // Student submitted
        ChangesRequested = 2,    // Supervisor wants changes
        Resubmitted = 3,         // Student resubmitted
        SupervisorApproved = 4,  // Supervisor approved
        CoordinatorApproved = 5, // Coordinator approved
        Accepted = 6             // Final acceptance
    }
}
