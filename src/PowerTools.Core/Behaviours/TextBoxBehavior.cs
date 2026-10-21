using System.Windows;
using System.Windows.Controls;
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
        #region InjectTextAtCursor Property

        public static readonly DependencyProperty InjectTextAtCursorProperty =
            DependencyProperty.RegisterAttached(
                "InjectTextAtCursor",
                typeof(string),
                typeof(TextBoxBehavior),
                new PropertyMetadata(null, OnInjectTextAtCursorChanged));

        public static string GetInjectTextAtCursor(DependencyObject obj) => (string)obj.GetValue(InjectTextAtCursorProperty);
        public static void SetInjectTextAtCursor(DependencyObject obj, string value) => obj.SetValue(InjectTextAtCursorProperty, value);

        private static void OnInjectTextAtCursorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox && e.NewValue is string textToInsert && !string.IsNullOrEmpty(textToInsert))
            {
                // Force focus back to textbox so selection placement remains accurate
                textBox.Focus();

                // SelectedText acts as a shortcut. It overwrites highlighted text,
                // or safely places the new text exactly where the blinking cursor is.
                int caretIndex = textBox.CaretIndex;
                textBox.SelectedText = textToInsert;

                // Reposition the cursor right at the end of your newly inserted string
                textBox.CaretIndex = caretIndex + textToInsert.Length;

                // Clear the behavior property so it's ready to accept the next insertion
                SetInjectTextAtCursor(textBox, null);
            }
        }

        #endregion
    }
}
