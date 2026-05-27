using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Commands;

namespace PowerTools.Core.Controls
{
    /// <summary>
    /// Interaction logic for TextPlaceHolder.xaml
    /// </summary>
    public partial class TextPlaceHolder : UserControl
    {
        public string PlaceHolder
        {
            get { return (string)GetValue(PlaceHolderProperty); }
            set { SetValue(PlaceHolderProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public ICommand CmdClearText
        {
            get { return (ICommand)GetValue(CmdClearTextProperty); }
            set { SetValue(CmdClearTextProperty, value); }
        }

        public ICommand CmdEnterKeyPressed { get; set; }

        public static readonly DependencyProperty PlaceHolderProperty = DependencyProperty.Register("PlaceHolder", typeof(string), typeof(TextPlaceHolder), new PropertyMetadata(null));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextPlaceHolder), new PropertyMetadata(null));
        public static readonly DependencyProperty CmdClearTextProperty = DependencyProperty.Register("CmdClearText", typeof(ICommand), typeof(TextPlaceHolder), new PropertyMetadata(null));

        public TextPlaceHolder()
        {
            CmdEnterKeyPressed = new DelegateCommand(OnCmdEnterKeyPressed);
            InitializeComponent();
        }

        private void OnCmdEnterKeyPressed()
        {
            SetValue(TextProperty, Text);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Text = string.Empty;
            CmdClearText?.Execute(this);
        }
    }
}
