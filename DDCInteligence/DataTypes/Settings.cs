using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.DataTypes
{
    public class Settings
    {
        public List<string> Server { get; set; } = new List<string>();
        public int Port { get; set; } = 3306;
        public List<string> Databases = new List<string>();
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
