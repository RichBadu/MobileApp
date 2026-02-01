using Mde.Project.Mobile.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mde.Project.Mobile.Core.Helpers
{
    public class TransactionGroup : ObservableCollection<TransactionRecord>
    {
        public string DateGroup { get; set; }

        public TransactionGroup(string dateGroup, IEnumerable<TransactionRecord> transactions) : base(transactions)
        {
          
            DateGroup = dateGroup;
        }
    }
}
