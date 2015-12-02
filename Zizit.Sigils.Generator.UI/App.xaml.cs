using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Squirrel;

namespace Zizit.Sigils.Generator.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length > 0)
            {
                using (var mgr = new UpdateManager("http://zizit.lt"))
                {
                    // Note, in most of these scenarios, the app exits after this method
                    // completes!
                    SquirrelAwareApp.HandleEvents(
                        onInitialInstall: v =>
                        {
                            mgr.CreateShortcutForThisExe();
                            Shutdown();
                        },
                        onAppUpdate: v =>
                        {
                            mgr.CreateShortcutForThisExe();
                            Shutdown();
                        },
                        onAppUninstall: v =>
                        {
                            mgr.RemoveShortcutForThisExe();
                            Shutdown();
                        },
                        onFirstRun: () =>
                        {
                            MessageBox.Show("Success", "Installation successful", MessageBoxButton.OK);
                            Shutdown();
                        });
                }
            }
        }
    }
}
