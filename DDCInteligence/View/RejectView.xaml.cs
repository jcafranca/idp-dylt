using System.Windows.Forms;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for RejectView.xaml
    /// </summary>
    public partial class RejectView
    {
        public DialogResult Result { get; set; } = System.Windows.Forms.DialogResult.Cancel;
        public string Remarks { get; private set; }
        public RejectView()
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
