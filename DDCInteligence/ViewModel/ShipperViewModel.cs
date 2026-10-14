using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.ViewModel.UserControl
{
    public class ShipperViewModel : AccountsModel
    {
        protected override int Type => 0;
        protected override string Identifier => "Shipper";
        public ShipperViewModel(IProcess process) : base(process) { }
    }
}
