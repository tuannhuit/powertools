using Prism.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PowerTools.Core.Controls
{
    /// <summary>
    /// Interaction logic for TokenTextEditor.xaml
    /// </summary>
    public partial class TokenTextEditor : UserControl, INotifyPropertyChanged
    {
        private bool _isEnter;
        private string _insertedToken;

        public string InsertedToken
        {
            get { return _insertedToken; }
            set
            {
                if (_insertedToken != value)
                {
                    _insertedToken = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand CmdEnterKeyPressed { get; }
        public ICommand CmdApplyToken { get; }

        #region Text Property

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(TokenTextEditor),
            new PropertyMetadata(null, null));

        #endregion

        #region IsReadOnly Property

        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }
        public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
            "IsReadOnly",
            typeof(bool),
            typeof(TokenTextEditor),
            new PropertyMetadata(false));

        #endregion

        #region DoShowTokens Property

        public bool DoShowTokens
        {
            get { return (bool)GetValue(DoShowTokensProperty); }
            set { SetValue(DoShowTokensProperty, value); }
        }
        public static readonly DependencyProperty DoShowTokensProperty = DependencyProperty.Register(
            "DoShowTokens",
            typeof(bool),
            typeof(TokenTextEditor),
            new PropertyMetadata(true));

        #endregion

        #region HighlightTokens Property
        public ObservableCollection<string> HighlightTokens
        {
            get { return (ObservableCollection<string>)GetValue(HighlightTokensProperty); }
            set { SetValue(HighlightTokensProperty, value); }
        }

        public static readonly DependencyProperty HighlightTokensProperty = DependencyProperty.Register(
            "HighlightTokens",
            typeof(ObservableCollection<string>),
            typeof(TokenTextEditor),
            new PropertyMetadata(new ObservableCollection<string>()));
        #endregion

        #region PaddingHighlighter Property
        public Thickness PaddingHighlighter
        {
            get { return (Thickness)GetValue(PaddingHighlighterProperty); }
            set { SetValue(PaddingHighlighterProperty, value); }
        }

        public static readonly DependencyProperty PaddingHighlighterProperty = DependencyProperty.Register(
            "PaddingHighlighter",
            typeof(Thickness),
            typeof(TokenTextEditor),
            new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        #endregion

        #region PaddingTextEditor Property
        public Thickness PaddingTextEditor
        {
            get { return (Thickness)GetValue(PaddingTextEditorProperty); }
            set { SetValue(PaddingTextEditorProperty, value); }
        }

        public static readonly DependencyProperty PaddingTextEditorProperty = DependencyProperty.Register(
            "PaddingTextEditor",
            typeof(Thickness),
            typeof(TokenTextEditor),
            new FrameworkPropertyMetadata(new Thickness(), FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        #endregion

        #region ForegroundTextEditor Property
        public Brush ForegroundTextEditor
        {
            get { return (Brush)GetValue(ForegroundTextEditorProperty); }
            set { SetValue(ForegroundTextEditorProperty, value); }
        }

        public static readonly DependencyProperty ForegroundTextEditorProperty = DependencyProperty.Register(
            "ForegroundTextEditor",
            typeof(Brush),
            typeof(TokenTextEditor),
            new FrameworkPropertyMetadata(Brushes.Green, FrameworkPropertyMetadataOptions.AffectsParentMeasure));
        #endregion

        public TokenTextEditor()
        {
            CmdEnterKeyPressed = new DelegateCommand(OnCmdEnterKeyPressed);
            CmdApplyToken = new DelegateCommand<string>(OnCmdApplyToken);

            InitializeComponent();
        }

        private void OnCmdEnterKeyPressed()
        {
            _isEnter = true;
            SetValue(TextProperty, Text);
            _isEnter = false;
        }

        private void OnCmdApplyToken(string token)
        {
            InsertedToken = token;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
