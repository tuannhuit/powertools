using PowerTools.Core.Models;
using Prism.Commands;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PowerTools.Core.Controls
{
    /// <summary>
    /// Interaction logic for TextPlaceHolder.xaml
    /// </summary>
    public partial class TextPlaceHolder : UserControl
    {
        private bool _isEnter;
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

        public object Model
        {
            get { return (object)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }

        public ICommand CmdClearText
        {
            get { return (ICommand)GetValue(CmdClearTextProperty); }
            set { SetValue(CmdClearTextProperty, value); }
        }
        public ICommand CmdEnterKeyPressed { get; set; }

        public static readonly DependencyProperty PlaceHolderProperty = DependencyProperty.Register("PlaceHolder", typeof(string), typeof(TextPlaceHolder), new PropertyMetadata(null));
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextPlaceHolder), new PropertyMetadata(null, null));
        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(object), typeof(TextPlaceHolder), new PropertyMetadata(null, OnModelChanged));
        public static readonly DependencyProperty CmdClearTextProperty = DependencyProperty.Register("CmdClearText", typeof(ICommand), typeof(TextPlaceHolder), new PropertyMetadata(null));

        public TextPlaceHolder()
        {
            _isEnter = false;
            CmdEnterKeyPressed = new DelegateCommand(OnCmdEnterKeyPressed);
            InitializeComponent();
        }

        private static void OnModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var criteria = (Criteria)e.NewValue;
            if (criteria != null)
            {
                criteria.ValueChanged += (s) =>
                {
                    var control = (TextPlaceHolder)d;
                    if (control._isEnter)
                    {
                        s.Reason = ValueChangedEvent.KeyEnter;
                    }
                };
            }
        }

        private void OnCmdEnterKeyPressed()
        {
            _isEnter = true;
            SetValue(TextProperty, Text);
            _isEnter = false;
        }

        private void btnClear_OnClick(object sender, RoutedEventArgs e)
        {
            _isEnter = true;
            SetValue(TextProperty, string.Empty);
            CmdClearText?.Execute(this);
            _isEnter = false;
        }
    }
}
