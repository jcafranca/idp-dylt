using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.ViewModel
{
    public class BillToViewModel : AccountsModel
    {
        protected override int Type => 2;
        protected override string Identifier => "BillTo";
        public BillToViewModel(IProcess process) : base(process) { }
    }
}
