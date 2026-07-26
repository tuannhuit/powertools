using System.Collections;
using System.Windows;

namespace PowerTools.Core.Behaviours
{
    public static class ExpanderBehavior
    {
        #region Header Property

        public static IList GetActions(DependencyObject obj)
        {
            return (IList)obj.GetValue(ActionsProperty);
        }

        public static void SetActions(DependencyObject obj, IList value)
        {
            obj.SetValue(ActionsProperty, value);
        }

        public static readonly DependencyProperty ActionsProperty = DependencyProperty.RegisterAttached(
            "Actions",
            typeof(IList),
            typeof(ExpanderBehavior),
            new FrameworkPropertyMetadata(null));

        #endregion
    }
}
