using Prism.Mvvm;

namespace PowerTools.Core.Models
{
    public class WindowSettings: BindableBase
    {
        public static readonly double MIN_WIDTH = 50;
        public static readonly double MIN_HEIGHT = 50;

        private double _width;
        public double Width
        {
            get => _width;
            set
            {
                _width=value;
                RaisePropertyChanged();
            }
        }

        private double _height;
        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                RaisePropertyChanged();
            }
        }

        private double _minWidth;
        public double MinWidth
        {
            get => _minWidth;
            set
            {
                _minWidth = value < MIN_WIDTH ? MIN_WIDTH : value;
                RaisePropertyChanged();
            }
        }

        private double _minHeight;
        public double MinHeight
        {
            get => _minHeight;
            set
            {
                _minHeight = value < MIN_HEIGHT ? MIN_HEIGHT : value;
                RaisePropertyChanged();
            }
        }

        private double _maxWidth;
        public double MaxWidth
        {
            get => _maxWidth;
            set
            {
                _maxWidth = value;
                RaisePropertyChanged();
            }
        }

        private double _maxHeight;
        public double MaxHeight
        {
            get => _maxHeight;
            set
            {
                _maxHeight = value;
                RaisePropertyChanged();
            }
        }
    }
}