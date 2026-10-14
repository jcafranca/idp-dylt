using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.Utilities.Helpers
{
    public class WindowRequestMessage
    {
        public Action<MainWindow> Callback { get; }
        public WindowRequestMessage(Action<MainWindow> callback)
        {
            Callback = callback;
        }
    }
}
