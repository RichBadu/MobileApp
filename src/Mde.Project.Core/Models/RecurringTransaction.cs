
using Mde.Project.Mobile.Core.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Core.Models
{
    public partial class RecurringTransaction
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string CategoryId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }    
        public string Name { get; set; }
        public string Frequency { get; set; }
        public int? DayOfMonth { get; set; }
        public int? DayOfWeek { get; set; }
      
        public bool IsActive { get; set; }
        public DateTime? LastProcessedDate { get; set; }
        public DateTime? NextDueDate { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
