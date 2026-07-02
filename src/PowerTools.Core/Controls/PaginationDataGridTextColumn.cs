using System.Windows;
using System.Windows.Controls;

namespace PowerTools.Core.Controls
{
    public class PaginationDataGridTextColumn: DataGridTextColumn
    {
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public object FilterModel
        {
            get { return (object)GetValue(FilterModelProperty); }
            set { SetValue(FilterModelProperty, value); }
        }

        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(PaginationDataGridTextColumn), new PropertyMetadata(null));
        public static readonly DependencyProperty FilterModelProperty = DependencyProperty.Register("FilterModel", typeof(object), typeof(PaginationDataGridTextColumn), new PropertyMetadata(null));
    }
}
