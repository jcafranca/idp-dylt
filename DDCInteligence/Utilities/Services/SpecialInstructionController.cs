using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.Utilities.Services
{
    internal class SpecialInstructionController : ITextBoxController
    {
        public event SelectAllEventHandler SelectAll;
        public void OnSelectAll()
        {
            SelectAll?.Invoke(this);
        }
    }
}
