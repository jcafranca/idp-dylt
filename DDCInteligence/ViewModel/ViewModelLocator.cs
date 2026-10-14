/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:DDCInteligence"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using CommonServiceLocator;
using DDCInteligence.DataTypes;
using DDCInteligence.Utilities.Events;
using DDCInteligence.Utilities.Services;
using DDCInteligence.ViewModel.UserControl;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Windows;

namespace DDCInteligence.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}
            ///

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<NonClientAreaViewModel>();
            SimpleIoc.Default.Register<LoginViewModel>();
            SimpleIoc.Default.Register<IProcess, ProcessViewModel>();

            SimpleIoc.Default.Register<ConsigneeViewModel>();
            SimpleIoc.Default.Register<ShipperViewModel>();
            SimpleIoc.Default.Register<BillToViewModel>();
            SimpleIoc.Default.Register<CODRateToViewModel>();
            SimpleIoc.Default.Register<BillingInfoViewModel>();
            SimpleIoc.Default.Register<ImageViewerViewModel>();
            SimpleIoc.Default.Register<ReferenceViewModel>();
            SimpleIoc.Default.Register<ItemsViewModel>();
            SimpleIoc.Default.Register<AccessorialViewModel>();
            SimpleIoc.Default.Register<InstructionViewModel>();
            SimpleIoc.Default.Register<IdleViewModel>();
            SimpleIoc.Default.Register<ReferenceNumberViewModel>();
            SimpleIoc.Default.Register<SharedEvent>();

        }

        public MainViewModel Main => ServiceLocator.Current.GetInstance<MainViewModel>();
        public NonClientAreaViewModel NonClientArea => ServiceLocator.Current.GetInstance<NonClientAreaViewModel>();
        public LoginViewModel Login => ServiceLocator.Current.GetInstance<LoginViewModel>();
        public IProcess Process => ServiceLocator.Current.GetInstance<IProcess>();
        public ConsigneeViewModel Consignee => ServiceLocator.Current.GetInstance<ConsigneeViewModel>();
        public ShipperViewModel Shipper => ServiceLocator.Current.GetInstance<ShipperViewModel>();
        public BillToViewModel BillTo => ServiceLocator.Current.GetInstance<BillToViewModel>();
        public CODRateToViewModel RateTo => ServiceLocator.Current.GetInstance<CODRateToViewModel>();
        public BillingInfoViewModel BillingInfo => ServiceLocator.Current.GetInstance<BillingInfoViewModel>();
        public ImageViewerViewModel ImageViewer => ServiceLocator.Current.GetInstance<ImageViewerViewModel>();
        public ReferenceViewModel Reference => ServiceLocator.Current.GetInstance<ReferenceViewModel>();
        public ItemsViewModel Items => ServiceLocator.Current.GetInstance<ItemsViewModel>();
        public AccessorialViewModel Accessorials => ServiceLocator.Current.GetInstance<AccessorialViewModel>();
        public InstructionViewModel Instructions => ServiceLocator.Current.GetInstance<InstructionViewModel>();
        public IdleViewModel IdleVM => ServiceLocator.Current.GetInstance<IdleViewModel>();
        public SharedEvent SharedEvent => ServiceLocator.Current.GetInstance<SharedEvent>();
        public ReferenceNumberViewModel RefNumberVM => ServiceLocator.Current.GetInstance<ReferenceNumberViewModel>();
        public static ViewModelLocator Instance => new Lazy<ViewModelLocator>(() => Application.Current.TryFindResource("Locator") as ViewModelLocator).Value;
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}