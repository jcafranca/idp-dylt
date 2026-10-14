using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HandyControl.Interactivity;
using DDCInteligence.Utilities.Helpers;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel;
using System.Threading.Tasks;

namespace DDCInteligence.Utilities.Behaviors
{
    public class FocusBehavior : Behavior<Control>
    {
        private readonly RubberBandServices _rubberBand;
        private readonly PromptHelperService _promptHelper;

        public FocusBehavior()
        {
            _rubberBand = new RubberBandServices();
            _promptHelper = new PromptHelperService();
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotFocus += AssociatedObject_GotFocus;
            AssociatedObject.LostFocus += AssociatedObject_LostFocus;
            AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
            AssociatedObject.PreviewMouseDown -= AssociatedObject_PreviewMouseDown;
        }

        private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Control control)
            {
                ApplyLostFocusStyle(control);
                _promptHelper.ClearPrompt();
            }
        }

        private async void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Control control)
            {
                ApplyFocusStyle(control);
                await HandleTextBoxFocus(control, e);
            }
        }

        private void AssociatedObject_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox && !textBox.IsKeyboardFocusWithin)
            {
                textBox.Focus();
                textBox.SelectAll();
                e.Handled = true;
            }
        }

        private void ApplyFocusStyle(Control control)
        {
            if (ViewModelLocator.Instance.Process.CurrentSkin == HandyControl.Data.SkinType.Dark)
                return;

            var backgroundProperty = control.GetType().GetProperty("Background");
            backgroundProperty?.SetValue(control, new SolidColorBrush(Color.FromArgb(50, 225, 225, 1)));
        }

        private void ApplyLostFocusStyle(Control control)
        {
            if (ViewModelLocator.Instance.Process.CurrentSkin == HandyControl.Data.SkinType.Dark)
                return;

            var backgroundProperty = control.GetType().GetProperty("Background");
            backgroundProperty?.SetValue(control, new SolidColorBrush(Colors.Transparent));
        }

        private async Task HandleTextBoxFocus(Control control, RoutedEventArgs e)
        {
            if (!(control is TextBox textBox))
                return;

            var originalTextBox = e.OriginalSource as TextBox;
            originalTextBox?.SelectAll();

            // Handle rubber band functionality
            try
            {
                ViewModelLocator.Instance.ImageViewer.pictureBox.SetRubberBand(originalTextBox?.Text ?? string.Empty);
                _rubberBand.ScrollToText(originalTextBox?.Text ?? string.Empty);
            } catch { }

            // Handle prompt functionality
            //var prompt = await _promptHelper.GetPromptAsync(
            //    control,
            //    textBox.Name,
            //    originalTextBox?.Text);
            //_promptHelper.SetPrompt(prompt ?? string.Empty);
        }
    }
}