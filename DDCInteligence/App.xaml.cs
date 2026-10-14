using AvalonDock.Themes;
using DDCInteligence.Utilities.Services;
using DDCInteligence.View;
using DDCInteligence.ViewModel;
using HandyControl.Data;
using HandyControl.Tools;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DDCInteligence
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected LogServices log = new LogServices();
        protected override void OnStartup(StartupEventArgs e)
        {

            ConfigHelper.Instance.SetLang("en");

            base.OnStartup(e);

            // Show splash window immediately
            var splashWindow = new SplashWindow(ViewModelLocator.Instance.Process);
            Application.Current.MainWindow = splashWindow;
            splashWindow.Show();

        }
        internal void UpdateTheme(SkinType skin)
        {
            try
            {
                var skins0 = Resources.MergedDictionaries[0];
                skins0.MergedDictionaries.Clear();
                skins0.MergedDictionaries.Add(ResourceHelper.GetSkin(skin));
                skins0.MergedDictionaries.Add(ResourceHelper.GetSkin(typeof(Application).Assembly, "../Resources/Themes", skin));

                var skins1 = Resources.MergedDictionaries[1];
                skins1.MergedDictionaries.Clear();
                skins1.MergedDictionaries.Add(new ResourceDictionary
                {
                    Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
                });
                if (skin == SkinType.Default)
                {
                    ViewModelLocator.Instance.Main.AvalonDockTheme = new Vs2013LightTheme();
                }
                else if (skin == SkinType.Violet)
                {
                    ViewModelLocator.Instance.Main.AvalonDockTheme = new Vs2013BlueTheme();
                }
                else
                {
                    ViewModelLocator.Instance.Main.AvalonDockTheme = new Vs2013DarkTheme();
                }

                Current.MainWindow?.OnApplyTemplate();
                SaveThemeToTemporary(skin);

            }
            catch (Exception ex)
            {
                this.log.ErrorLogToFile(ex.Message, ex.StackTrace, nameof(UpdateTheme));
            }
        }

        internal void SaveThemeToTemporary(SkinType skin)
        {
            try
            {
                string folder = System.IO.Path.GetTempPath();
                string filename = System.IO.Path.Combine(folder, $"ApexDataEntrySkin.{skin.ToString()}");
                var files = System.IO.Directory.GetFiles(folder, $"*ApexDataEntrySkin*");
                if (files != null)
                    files.ToList().ForEach(f => {
                        try { System.IO.File.Delete(f); } catch { }
                    });
                System.IO.File.Create(filename).Close();
            }
            catch { }
        }
    }
}
