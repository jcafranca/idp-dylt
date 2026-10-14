using DDCInteligence.Utilities.Helpers;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Interactivity;

namespace DDCInteligence.Utilities.Behaviors
{
    public class TextChangedBehavior : Behavior<TextBox>
    {
        private readonly RubberBandServices _rubberBand;
        private readonly PromptHelperService _promptHelper;

        public TextChangedBehavior()
        {
            _rubberBand = new RubberBandServices();
            _promptHelper = new PromptHelperService();
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextChanged += AssociatedObject_TextChanged;
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Name.Equals("TextBoxName", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        _promptHelper.ClearPrompt();
                    }

                    return;
                }

                _ = ViewModelLocator.Instance.Process.HandleTermsPromptAsync(textBox.Text, (Control)sender, textBox.Name);
            }
        }
        
    }
}
