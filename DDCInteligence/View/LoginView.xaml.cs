using DDCInteligence.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView
    {
        public LoginView()
        {
            InitializeComponent();

            Assembly assembly = Assembly.GetExecutingAssembly();
            lblProjectCode.Text = GetAssemblyAttribute<AssemblyTrademarkAttribute>(assembly)?.Trademark;
        }
        private T GetAssemblyAttribute<T>(Assembly assembly) where T : Attribute
        {
            return assembly.GetCustomAttribute<T>();
        }
        private void TextBoxPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ViewModelLocator.Instance.Login.Password = TextBoxPassword.Password;        
        }
    }
}
