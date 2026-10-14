using DDCInteligence.DataTypes;
using DDCInteligence.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDCInteligence.ViewModel
{
    public class ItemsViewModel : ItemsModel
    {
        protected override string Identifier => "Items";
        public ItemsViewModel(IProcess process) : base(process) { }
    }
}
