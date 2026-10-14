using DDCInteligence.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static DDCInteligence.DataTypes.DocumentTypes;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for QueryView.xaml
    /// </summary>
    public partial class RemarksView 
    {
        public DialogResult Result { get; set; } = System.Windows.Forms.DialogResult.Cancel;
        public EStatus eStatus { get; set; } = EStatus.QUERY;
        public string Remarks { get; set; } = "";
        public string Shipper { get; set; } = "";
        public string Consignee { get; set; } = "";
        public RemarksView()
        {
            InitializeComponent();
        }
   
        public new DialogResult ShowDialog()
        {
            base.ShowDialog();
            return Result;
        }
    }
}
