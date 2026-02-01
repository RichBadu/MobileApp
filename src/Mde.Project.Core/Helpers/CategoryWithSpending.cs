using System;
using System.Collections.Generic;
using System.Text;

namespace Mde.Project.Core.Helpers
{
    public class CategoryWithSpending
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public decimal MonthlyBudget { get; set; }
        public decimal SpentThisMonth { get; set; }
    }
}
