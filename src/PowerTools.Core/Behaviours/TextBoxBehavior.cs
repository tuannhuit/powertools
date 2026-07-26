using System.Windows;
using System.Windows.Input;

namespace PowerTools.Core.Behaviours
{
    public static class TextBoxBehavior
    {
        #region IsFocusedOnLoad Property

        public static readonly DependencyProperty IsFocusedOnLoadProperty =
            DependencyProperty.RegisterAttached(
                "IsFocusedOnLoad",
                typeof(bool),
                typeof(TextBoxBehavior),
                new PropertyMetadata(false, OnIsFocusedOnLoadChanged));

        public static bool GetIsFocusedOnLoad(DependencyObject obj) => (bool)obj.GetValue(IsFocusedOnLoadProperty);
        public static void SetIsFocusedOnLoad(DependencyObject obj, bool value) => obj.SetValue(IsFocusedOnLoadProperty, value);

        private static void OnIsFocusedOnLoadChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element && (bool)e.NewValue)
            {
                element.Loaded += (s, ev) =>
                {
                    // Force focus on both the visual element thread and the OS input thread
                    element.Focus();
                    Keyboard.Focus(element);
                };
            }
        }

        #endregion
    }
}
