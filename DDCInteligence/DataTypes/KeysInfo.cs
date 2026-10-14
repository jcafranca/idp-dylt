using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DDCInteligence.DataTypes
{
    public class KeysInfo
    {
        public class DisableKeysMessage
        {
            public Key[] Keys { get; set; }
            public DisableKeysMessage(params Key[] keys) { Keys = keys; }
        }

        public class EnableKeysMessage
        {
            public Key[] Keys { get; set; }
            public EnableKeysMessage(params Key[] keys) { Keys = keys; }
        }
    }
}
