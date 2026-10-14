using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.ViewModel
{
    public class BillingInfoViewModel : BillingInfoModel
    {
        protected override string Identifier => "References";
        public BillingInfoViewModel(IProcess process) : base(process) { }
    }
}
