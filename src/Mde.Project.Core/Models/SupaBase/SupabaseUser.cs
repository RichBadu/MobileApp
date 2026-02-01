using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mde.Project.Mobile.Core.Models.SupaBase
{
    [Table("users")]
    public class SupabaseUser : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;
        [Column("email")]
        public string Email { get; set; }

        [Column("display_name")]
        public string? DisplayName { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("last_login_at")]
        public DateTime? LastLoginAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

    }
}
