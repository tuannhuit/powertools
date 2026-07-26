using Prism.Services.Dialogs;
using System.Windows;
using System.Windows.Controls;
using PowerTools.Core.SharedServices;

namespace PowerTools.Views.UserControls
{
    /// <summary>
    /// Interaction logic for DialogView.xaml
    /// </summary>
    public partial class DialogView : UserControl
    {
        public DialogView()
        {
            InitializeComponent();
            this.Loaded += OnUserControlLoaded;
        }

        private void OnUserControlLoaded(object sender, RoutedEventArgs e)
        {
            // 1. Grab Prism's dynamically generated Window wrapper host instance
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow == null) return;

            // 2. Extract values from your ViewModel's DataContext parameters safely
            if (this.DataContext is IDialogAware dialogAwareViewModel &&
                dialogAwareViewModel is ViewModels.UserControls.DialogViewModel vm)
            {
                // Set the sizes directly on the parent native window container frame
                parentWindow.Width = vm.Width;
                parentWindow.Height = vm.Height;
            }

            // 3. FORCE RE-CENTERING COMPILATION LOOP
            // Check if the dialog has an active application Owner window set
            if (ApplicationService.Instance.MainWindow != null)
            {
                // Check if the application background dashboard is maximized
                if (ApplicationService.Instance.MainWindow.WindowState == WindowState.Maximized)
                {
                    // Use the precise physical monitor work area bounds (ignores negative overflow coordinates)
                    Rect workArea = SystemParameters.WorkArea;

                    parentWindow.Left = workArea.Left + (workArea.Width - parentWindow.Width) / 2;
                    parentWindow.Top = workArea.Top + (workArea.Height - parentWindow.Height) / 2;
                }
                else
                {
                    // Use standard bounding box logic if the parent is floating/normal size
                    double ownerWidth = ApplicationService.Instance.MainWindow.ActualWidth;
                    double ownerHeight = ApplicationService.Instance.MainWindow.ActualHeight;
                    double ownerLeft = ApplicationService.Instance.MainWindow.Left;
                    double ownerTop = ApplicationService.Instance.MainWindow.Top;

                    parentWindow.Left = ownerLeft + (ownerWidth - parentWindow.Width) / 2;
                    parentWindow.Top = ownerTop + (ownerHeight - parentWindow.Height) / 2;
                }
            }
            else
            {
                // Fallback: If no owner is assigned, snap it to the center of the active desktop screen
                parentWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
    }
}
