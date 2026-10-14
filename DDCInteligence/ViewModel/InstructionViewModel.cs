using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.ViewModel
{
    public class InstructionViewModel : InstructionModel
    {
        public InstructionViewModel(IProcess process) : base(process) { }
    }
}
