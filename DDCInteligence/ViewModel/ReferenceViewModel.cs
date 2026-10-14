using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.ViewModel
{
    public class ReferenceViewModel : ReferenceModel
    {
        protected override string Identifier => "Reference";
        public ReferenceViewModel(IProcess process) : base(process) { }
    }
}
