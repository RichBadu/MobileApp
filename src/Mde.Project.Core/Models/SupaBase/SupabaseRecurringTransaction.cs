using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mde.Project.Mobile.Core.Models.SupaBase
{
    [Table("recurring_transactions")]
    public class SupabaseRecurringTransaction : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("category_id")]
        public string CategoryId { get; set; } = string.Empty;

        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("type")]
        public string Type { get; set; } = string.Empty;

        [Column("frequency")]
        public string Frequency { get; set; } = string.Empty;

        [Column("day_of_month")]
        public int? DayOfMonth { get; set; }

        [Column("day_of_week")]
        public int? DayOfWeek { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("last_processed_date")]
        public DateTime? LastProcessedDate { get; set; }

        [Column("next_due_date")]
        public DateTime? NextDueDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
