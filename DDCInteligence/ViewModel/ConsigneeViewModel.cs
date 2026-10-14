using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.ViewModel
{
    public class ConsigneeViewModel : AccountsModel
    {
        protected override int Type => 1;
        protected override string Identifier => "Consignee";
        public ConsigneeViewModel(IProcess process) : base(process) { }
    }
}
