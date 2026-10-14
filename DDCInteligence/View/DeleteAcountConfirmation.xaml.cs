using DDCInteligence.Utilities.Services;
using System.Windows;
using System.Windows.Controls;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for CustomInputBox.xaml
    /// </summary>
    public partial class DeleteAcountConfirmation : Window
    {
        UtilityServices utilityServices = new UtilityServices();
        public DeleteAcountConfirmation(Window owner = null)
        {
            InitializeComponent();
            if (owner != null)
            {
                this.Owner = owner;
            }
            this.Loaded += (s, e) => this.utilityServices.SetWindowOnScreen(this);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PlaceholderText.Visibility = string.IsNullOrEmpty(PasswordBox.Password)
            ? Visibility.Visible
            : Visibility.Collapsed;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
