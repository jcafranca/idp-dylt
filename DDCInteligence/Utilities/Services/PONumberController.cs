using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.Utilities.Services
{
    public class PONumberController : ITextBoxController
    {
        public event SelectAllEventHandler SelectAll;
        public void OnSelectAll()
        {
            SelectAll?.Invoke(this);
        }
    }
}
