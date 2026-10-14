using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DDCInteligence.Utilities.Services
{
    public static class TextBoxAttachServices
    {
        public static readonly DependencyProperty TextBoxControllerProperty = DependencyProperty.RegisterAttached(
            "TextBoxController", typeof(ITextBoxController), typeof(TextBoxAttachServices),
            new FrameworkPropertyMetadata(null, OnTextBoxControllerChanged));
        public static void SetTextBoxController(UIElement element, ITextBoxController value)
        {
            element.SetValue(TextBoxControllerProperty, value);
        }
        public static ITextBoxController GetTextBoxController(UIElement element)
        {
            return (ITextBoxController)element.GetValue(TextBoxControllerProperty);
        }

        private static readonly Dictionary<ITextBoxController, TextBox> elements = new Dictionary<ITextBoxController, TextBox>();
        private static void OnTextBoxControllerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as TextBox;
            if (element == null)
                throw new ArgumentNullException("d");

            var oldController = e.OldValue as ITextBoxController;
            if (oldController != null)
            {
                elements.Remove(oldController);
                oldController.SelectAll -= SelectAll;
            }

            var newController = e.NewValue as ITextBoxController;
            if (newController != null)
            {
                if (!elements.ContainsKey(newController))
                {
                    elements.Add(newController, element);
                    newController.SelectAll += SelectAll;
                }
            }
        }
        private static void SelectAll(ITextBoxController sender)
        {
            TextBox element;
            if (!elements.TryGetValue(sender, out element))
                throw new ArgumentException("sender");
            element.Focus();
            element.SelectAll();
        }
    }

    public interface ITextBoxController
    {
        event SelectAllEventHandler SelectAll;
    }

    public delegate void SelectAllEventHandler(ITextBoxController sender);
}
