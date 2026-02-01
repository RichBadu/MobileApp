using SQLite;


namespace Mde.Project.Mobile.Core.Models
{
    public class Category
    {
        [PrimaryKey]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name  { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public string Type { get; set; }
        public decimal? MonthlyBudget { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
