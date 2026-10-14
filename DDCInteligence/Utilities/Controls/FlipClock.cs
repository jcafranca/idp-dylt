using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DDCInteligence.Utilities.Controls
{

    public class FlipClock : Control
    {
        private bool _isDisposed;
        public static readonly DependencyProperty NumberListProperty = DependencyProperty.Register("NumberList", typeof(List<int>), typeof(FlipClock), new PropertyMetadata(new List<int>
        {
            0,
            0,
            0,
            0,
            0,
            0
        }));
        public static readonly DependencyProperty DisplayTimeProperty = DependencyProperty.Register("DisplayTime", typeof(string), typeof(FlipClock), new PropertyMetadata(default(string), OnDisplayTimeChanged));
        public List<int> NumberList
        {
            get
            {
                return (List<int>)GetValue(NumberListProperty);
            }
            set
            {
                SetValue(NumberListProperty, value);
            }
        }
        public string DisplayTime
        {
            get
            {
                return (string)GetValue(DisplayTimeProperty);
            }
            set
            {
                SetValue(DisplayTimeProperty, value);
            }
        }

        private static void OnDisplayTimeChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
        {
            FlipClock obj = (FlipClock)s;
            DateTime dateTime = DateTime.Parse((string)e.NewValue);
            obj.NumberList = new List<int>
            {
                dateTime.Hour / 10,
                dateTime.Hour % 10,
                dateTime.Minute / 10,
                dateTime.Minute % 10,
                dateTime.Second / 10,
                dateTime.Second % 10
            };
        }

        public FlipClock()
        {
            DisplayTime = "00:00:00";
        }

        ~FlipClock()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private void FlipClock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {

        }
    }
}
