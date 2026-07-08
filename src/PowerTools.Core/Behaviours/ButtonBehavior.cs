using System.Windows;
using System.Windows.Controls;

namespace PowerTools.Core.Behaviours
{
    public static class ButtonBehavior
    {
        #region CornerRadius Property

        public static CornerRadius GetCornerRadius(DependencyObject obj)
        {
            return (CornerRadius)obj.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(DependencyObject obj, CornerRadius value)
        {
            obj.SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached(
            "CornerRadius",
            typeof(CornerRadius),
            typeof(ButtonBehavior),
            new FrameworkPropertyMetadata(new CornerRadius(5)));

        #endregion
    }
}
