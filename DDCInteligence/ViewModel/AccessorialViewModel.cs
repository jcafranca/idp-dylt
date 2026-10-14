using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.ViewModel
{
    public class AccessorialViewModel : AccessorialModel
    {
        protected override string Identifier => "Accessorials";
        public AccessorialViewModel(IProcess process) : base(process) { }
    }
}
