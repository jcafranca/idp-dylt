using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.DataTypes
{
    public class CityStateZipInfo
    {
        public bool hasValue { get; set; } = false;
        public string City { get; set; }
        public string State { get; set; }
        public string Zip {  get; set; }
        public string CountryCode {  get; set; }
    }
}
