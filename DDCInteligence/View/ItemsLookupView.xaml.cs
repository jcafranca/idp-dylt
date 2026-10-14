using DDCInteligence.Utilities.Services;
using System.Windows;
using static DDCInteligence.DataTypes.DatabaseTypes;

namespace DDCInteligence.View
{
    /// <summary>
    /// Interaction logic for ItemsLookupView.xaml
    /// </summary>
    public partial class ItemsLookupView
    {
        private readonly UtilityServices utilityServices = new UtilityServices();

        // Replace 'YourClassType' with the actual class you want to pass
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(ItemsInfo), typeof(ItemsLookupView), new PropertyMetadata(null));

        public ItemsInfo SelectedItem
        {
            get => (ItemsInfo)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
        public ItemsLookupView(Window owner)
        {
            InitializeComponent();
            if (owner != null)
            {
                this.Owner = owner;
            }
            this.Loaded += ItemsLookupView_Loaded;
        }

        private void ItemsLookupView_Loaded(object sender, RoutedEventArgs e)
        {
            utilityServices.SetWindowOnScreen(this);
        }
    }
}
