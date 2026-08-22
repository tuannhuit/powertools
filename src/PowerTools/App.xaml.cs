using PowerTools.Core.Configurations;
using PowerTools.Core.SharedServices;
using PowerTools.Helpers;
using PowerTools.ViewModels.UserControls;
using PowerTools.Views;
using PowerTools.Views.UserControls;
using Prism.Ioc;
using Prism.Unity;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace PowerTools
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        public static readonly string Version = "2.0.0.1";
        public static readonly string AppName = "PowerTools";
        public static readonly string OwnerName = "tuannhuit";
        public static readonly string RepoName = "powertools";
        public static IContainerProvider ContainerProvider;

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //containerRegistry.RegisterDialog<ModuleSettingsView, ModuleSettingsViewModel>();
            containerRegistry.RegisterDialog<DialogView, DialogViewModel>();
        }

        public static Action RegisteredUserUnHandledException;

        protected override Window CreateShell()
        {
            ContainerProvider = Container;
            AssembliesResolver.RegisterDependencyResolver();

            SetupUnhandledExceptionHandling();

            return Container.Resolve<MainWindow>();
        }

        private void SetupUnhandledExceptionHandling()
        {
            // Catch exceptions from all threads in the AppDomain.
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                ShowUnhandledException(args.ExceptionObject as Exception, "AppDomain.CurrentDomain.UnhandledException", false);

            // Catch exceptions from each AppDomain that uses a task scheduler for async operations.
            TaskScheduler.UnobservedTaskException += (sender, args) =>
                ShowUnhandledException(args.Exception, "TaskScheduler.UnobservedTaskException", false);

            // Catch exceptions from a single specific UI dispatcher thread.
            Dispatcher.UnhandledException += (sender, args) =>
            {
                RegisteredUserUnHandledException?.Invoke();

                // If we are debugging, let Visual Studio handle the exception and take us to the code that threw it.
                if (!Debugger.IsAttached)
                {
                    args.Handled = true;
                    ShowUnhandledException(args.Exception, "Dispatcher.UnhandledException", true);
                }
            };
        }

        private void ShowUnhandledException(Exception e, string unhandledExceptionType, bool promptUserForShutdown)
        {
            var messageBoxTitle = $"Unexpected Error Occurred: {unhandledExceptionType}";
            var messageBoxMessage = $"The following exception occurred:\n\n{e.Message.Substring(0, 1000)}";
            var messageBoxButtons = MessageBoxButton.OK;

            if (promptUserForShutdown)
            {
                messageBoxMessage += "\n\nNormally the app would die now. Should we let it die?";
                messageBoxButtons = MessageBoxButton.OK;
            }

            // Let the user decide if the app should die or not (if applicable).
            MessageBox.Show(messageBoxMessage, messageBoxTitle, messageBoxButtons);

            ApplicationService.Instance.Dispose();
            ModuleGlobalSettings.Instance.CurrentModule = null;
            Repositories.Store();

            Application.Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Repositories.Store();
            ApplicationService.Instance.Dispose();
            base.OnExit(e);
        }
    }
}
