using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Helpers;
using DDCInteligence.Utilities.Helpers.Validation;
using DDCInteligence.Utilities.Services;
using DDCInteligence.View;
using DDCInteligence.ViewModel;
using HandyControl.Interactivity;
using SweetAlertSharp;
using SweetAlertSharp.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WindowsInput;
using WindowsInput.Native;
using static DDCInteligence.DataTypes.DatabaseTypes;
using static System.Net.Mime.MediaTypeNames;

namespace DDCInteligence.Utilities.Behaviors
{
    public class PreviewTextInputBehevior : Behavior<Control>
    {
        private readonly LogServices _logServices;
        private readonly PromptHelperService _promptHelper;
        private readonly IEnumerable<string> itemColumns = new string[] { "WEIGHT", "PIECES", "PALLETS" };

        public PreviewTextInputBehevior()
        {
            _logServices = new LogServices();
            _promptHelper = new PromptHelperService();
        }
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewTextInput += AssociatedObject_PreviewTextInput;
        }

        private void AssociatedObject_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                if (dataGrid.CurrentCell.Column != null)
                {
                    int currentColumnIndex = dataGrid.CurrentCell.Column.DisplayIndex; // Get the current column index
                    DataGridColumn currentColumn = dataGrid.Columns[currentColumnIndex];
                    DataGridTextColumn textColumn = currentColumn as DataGridTextColumn;

                    switch (dataGrid.Name)
                    {
                        case "DgvItems":
                            if (textColumn != null && itemColumns.Any(h => h.Equals(textColumn.Header.ToString().ToUpper())))
                                e.Handled = !IsValidInputForNumeric(e.Text);
                            break;
                        case "DgvReference":
                            break;
                    }
                }                
            } else if (sender is TextBox textBox)
            {
                switch (textBox.Name)
                {
                    case "TextBoxTag":
                        e.Handled = !IsAlphaNumeric(e.Text);
                        break;
                    case "TextBoxTerms":
                        e.Handled  = !new string[]{ "P", "C", ""}.Contains(e.Text.ToUpper());
                        // Handle prompt for terms input

                        string currentText = textBox.Text + e.Text;

                        _ = ViewModelLocator.Instance.Process.HandleTermsPromptAsync(currentText, ((Control)sender), textBox.Name);
                        break;
                }
            }
        }
        
        private bool IsValidInputForNumeric(string inputText)
        {
            // Allow valid integers, the '#' symbol, or a period ('.') as valid input
            return int.TryParse(inputText, out _) || inputText == "#" || inputText == ".";
        }

        private readonly Regex _regex = new Regex("^[a-zA-Z0-9]*$"); // Regex for alphanumeric characters
        bool IsAlphaNumeric(string input)
        {
            return _regex.IsMatch(input);
        }
        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewTextInput -= AssociatedObject_PreviewTextInput;
        }
    }
}
