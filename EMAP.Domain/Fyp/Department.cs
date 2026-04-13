using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAP.Domain.Fyp
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty; // e.g. SE, CS, AI
        public bool IsActive { get; set; } = true;

        public ICollection<FypCall> FypCalls { get; set; } = new List<FypCall>();
        public ICollection<FypSupervisor> Supervisors { get; set; } = new List<FypSupervisor>();
        public ICollection<StudentGroup> StudentGroups { get; set; } = new List<StudentGroup>();
    }
}
