using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.Utilities.Helpers.Validation
{
    public delegate Task<IEnumerable<string>> ValidatorDelegate(object sender, string propertyName, params object[] args);
}
