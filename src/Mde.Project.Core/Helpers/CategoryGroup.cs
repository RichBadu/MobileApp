using Mde.Project.Mobile.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Core.Helpers
{
    public class CategoryGroup : List<Category>
    {
        public string TypeGroup { get; set; }

        public CategoryGroup(string typeGroup, List<Category> categories) : base(categories)
        {
            TypeGroup = typeGroup;
        }
    }
}
