using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.Utilities.Events
{
    public class PathChangedEventArgs : EventArgs
    {
        public string Path { get; }
        public PathChangedEventArgs(string path)
        {
            Path = path;
        }
    }
}
