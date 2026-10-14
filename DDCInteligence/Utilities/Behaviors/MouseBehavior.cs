using HandyControl.Interactivity;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DDCInteligence.Utilities.Behaviors
{
    public class MouseBehavior : Behavior<Control>
    {

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseWheel += AssociatedObject_MouseWheel;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewMouseWheel -= AssociatedObject_MouseWheel;
        }

        private void AssociatedObject_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (AssociatedObject is DataGrid dgv)
            {
                e.Handled = true; // Prevent DataGrid from handling the scroll

                // Find the ScrollViewer parent dynamically
                ScrollViewer scrollViewer = FindParentScrollViewer(dgv);
                if (scrollViewer != null)
                {
                    scrollViewer.CanContentScroll = false;
                    dgv.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        // Calculate the target scroll offset
                        double targetOffset = scrollViewer.VerticalOffset - e.Delta / 2.0;
                        scrollViewer.ScrollToVerticalOffset(targetOffset);
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }

        private ScrollViewer FindParentScrollViewer(DependencyObject element)
        {
            while (element != null)
            {
                if (element is ScrollViewer sv)
                    return sv;

                element = VisualTreeHelper.GetParent(element);
            }
            return null;
        }

    }
}
