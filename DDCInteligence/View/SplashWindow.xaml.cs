using DDCInteligence.DataTypes;
using DDCInteligence.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using HandyControl.Data;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow
    {
        private DispatcherTimer _timer;
        private int _currentStep = 0;
        private readonly string[] _loadingSteps = new string[]
        {
            "Initializing application...",
            "Loading configuration...",
            "Connecting to database...",
            "Loading user preferences...",
            "Preparing workspace...",
            "Finalizing setup...",
            "Starting application..."
        };

        public SplashWindow(IProcess process)
        {
            InitializeComponent();
            StartLoadingProcess();
            LoadCurrentSkin();
            lblVersion.Text = string.Format("Version {0}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
        private void StartLoadingProcess()
        {
            // Initialize timer
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(200); // Change step every 200ms
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_currentStep < _loadingSteps.Length)
            {
                // Update the status text
                UpdateStatusText(_loadingSteps[_currentStep]);
                _currentStep++;
            }
            else
            {
                // All steps completed, finish loading
                FinishLoading();
            }
        }
        private void LoadCurrentSkin()
        {
            var folder = System.IO.Path.GetTempPath();
            var filename = System.IO.Directory.GetFiles(folder, $"*ApexDataEntrySkin*")?.FirstOrDefault();
            if (filename != null)
            {
                var ext = System.IO.Path.GetExtension(filename)?.Replace(".", "");
                if (!string.IsNullOrEmpty(ext) && Enum.TryParse(ext, out SkinType skin))
                {
                    if (skin == (SkinType)ViewModelLocator.Instance.Process.CurrentSkin) return;
                    ViewModelLocator.Instance.Process.SetCurrentSkin(skin);
                    ((App)Application.Current).UpdateTheme(skin);
                    Messenger.Default.Send(skin, "SkinUpdated");
                }
            }
        }
        private void UpdateStatusText(string statusText)
        {
            // Find the TextBlock that shows "Finishing..." and update it
            var statusTextBlock = FindName("StatusTextBlock") as System.Windows.Controls.TextBlock;
            if (statusTextBlock != null)
            {
                statusTextBlock.Text = statusText;
            }
        }

        private void FinishLoading()
        {
            _timer.Stop();
            _timer = null;

            // Optional: Add a small delay before closing
            Task.Delay(500).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    // Close splash screen and show main window
                    // Transition to login window on UI thread
                    var loginWindow = new LoginView();
                    Application.Current.MainWindow = loginWindow;

                    loginWindow.Show();
                    this.Close();
                });
            });
        }

        // Optional: Handle window loaded event for additional initialization
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Perform any additional initialization here
            // For example, preload resources, check for updates, etc.
        }

        // Optional: Allow clicking to skip splash screen
        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                FinishLoading();
            }
        }

        // Clean up resources when window is closing
        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            _timer = null;
            base.OnClosed(e);
        }
    }
}
