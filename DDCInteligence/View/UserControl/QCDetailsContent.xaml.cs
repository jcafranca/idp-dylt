using HandyControl.Data;
using HandyControl.Tools;
using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDCInteligence.View.UserControl
{
    /// <summary>
    /// Interaction logic for QCDetailsContent.xaml
    /// </summary>
    public partial class QCDetailsContent
    {
        private Dictionary<string, TextEditor> _textEditor;
        public QCDetailsContent()
        {
            InitializeComponent();
            var textEditorCustomStyle = ResourceHelper.GetResource<Style>("TextEditorCustom");
            _textEditor = new Dictionary<string, TextEditor>
            {
                ["TXT"] = new TextEditor()
                {
                    Style = textEditorCustomStyle,
                },
            };
            BorderCode.Child = _textEditor["TXT"];
            _textEditor["TXT"].Text =  @"SHIPPER	  			CONSIGNEE			BILL TO
NAME: 	  NONE		NAME:     NONE		NAME:     NONE	
ADDRESS:  NONE		ADDRESS:  NONE		ADDRESS:  NONE
ADDRESS1: NONE 		ADDRESS1: NONE 		ADDRESS1: NONE
CITY:     NONE  	CITY:     NONE		CITY:     NONE
STATE:    NONE		STATE:    NONE		STATE:    NONE
ZIP:      NONE		ZIP:      NONE		ZIP:      NONE
PHONE:    NONE		PHONE:    NONE		PHONE:    NONE
CONTACT:  NONE		CONTACT:  NONE		CONTACT:  NONE";
        }
    }
}
