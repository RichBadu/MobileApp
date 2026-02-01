using Mde.Project.Mobile.Core.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Core.Models
{
    public class TransactionRecord
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string UserId { get; set; } 
        public string CategoryId { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
        public string? PhotoPath { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? NotificationId { get; set; }

    }
}
