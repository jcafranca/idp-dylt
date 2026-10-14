using DDCInteligence.Utilities.Services;
using System.Windows;
using static DDCInteligence.DataTypes.DatabaseTypes;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for ResourcesLookupWindow.xaml
    /// </summary>
    public partial class ReferenceLookupView : Window
    {
        private readonly UtilityServices utilityServices = new UtilityServices();

        // Replace 'YourClassType' with the actual class you want to pass
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(ReferenceInfo), typeof(ReferenceLookupView), new PropertyMetadata(null));

        public ReferenceInfo SelectedItem
        {
            get => (ReferenceInfo)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
        public ReferenceLookupView(Window owner)
        {
            InitializeComponent();
            if (owner != null)
            {
                this.Owner = owner;
            }
            this.Loaded += ReferenceLookupView_Loaded;
        }

        private void ReferenceLookupView_Loaded(object sender, RoutedEventArgs e)
        {
            utilityServices.SetWindowOnScreen(this);
        }
    }
}
