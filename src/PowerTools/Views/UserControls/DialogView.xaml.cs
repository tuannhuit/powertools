using PowerTools.Core.SharedServices;
using Prism.Services.Dialogs;
using System.Windows;
using System.Windows.Controls;

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
            this.DialogContentHost.Content = ApplicationService.Instance.DialogView.Content;

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
            var mainWindow = ApplicationService.Instance.MainWindow;
            if (mainWindow != null)
            {
                // Check if the application background dashboard is maximized
                if (mainWindow.WindowState == WindowState.Maximized)
                {
                    var presentationSource = PresentationSource.FromVisual(mainWindow);
                    if (presentationSource != null && presentationSource.CompositionTarget != null)
                    {
                        // 3. Extract the active DPI scaling matrix from the target secondary monitor
                        var transformMatrix = presentationSource.CompositionTarget.TransformFromDevice;

                        // 6. Center your custom dialog window frame matching the calculated screen values
                        // Because the Owner is set, setting Left and Top positions it relative to that secondary monitor screen!
                        parentWindow.Left = mainWindow.Left + ((mainWindow.Width - parentWindow.Width) * transformMatrix.M11) / (2 * transformMatrix.M11);
                        parentWindow.Top = mainWindow.Top + ((mainWindow.Height - parentWindow.Height) * transformMatrix.M22) / (2 * transformMatrix.M22);
                    }

                }
                else
                {
                    // Use standard bounding box logic if the parent is floating/normal size
                    double ownerWidth = mainWindow.ActualWidth;
                    double ownerHeight = mainWindow.ActualHeight;
                    double ownerLeft = mainWindow.Left;
                    double ownerTop = mainWindow.Top;

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
