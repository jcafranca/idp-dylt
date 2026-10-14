using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.ViewModel
{
    public class CODRateToViewModel : AccountsModel
    {
        protected override int Type => 3;
        protected override string Identifier => "RateTo";
        public CODRateToViewModel(IProcess process) : base(process) { }
    }
}
