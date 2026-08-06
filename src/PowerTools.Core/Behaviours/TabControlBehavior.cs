using System.Windows;

namespace PowerTools.Core.Behaviours
{
    public static class TabControlBehavior
    {
        #region CornerRadius Property

        public static bool GetCanCloseTabItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(CanCloseTabItemProperty);
        }

        public static void SetCanCloseTabItem(DependencyObject obj, bool value)
        {
            obj.SetValue(CanCloseTabItemProperty, value);
        }

        public static readonly DependencyProperty CanCloseTabItemProperty = DependencyProperty.RegisterAttached(
            "CanCloseTabItem",
            typeof(bool),
            typeof(TabControlBehavior),
            new FrameworkPropertyMetadata(true));

        #endregion
    }
}
