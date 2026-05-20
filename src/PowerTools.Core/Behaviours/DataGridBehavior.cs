using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace PowerTools.Core.Behaviours
{
    public static class DataGridBehavior
    {
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.RegisterAttached(
            "SelectedItem",
            typeof(object),
            typeof(DataGridBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedCellValuesChanged));

        public static object GetSelectedItem(DependencyObject obj)
            => (object)obj.GetValue(SelectedItemProperty);

        public static void SetSelectedItem(DependencyObject obj, object value)
            => obj.SetValue(SelectedItemProperty, value);

        public static readonly DependencyProperty SelectedCellValuesProperty = DependencyProperty.RegisterAttached(
            "SelectedCellValues",
            typeof(IList),
            typeof(DataGridBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedCellValuesChanged));

        public static IList GetSelectedCellValues(DependencyObject obj)
            => (IList)obj.GetValue(SelectedCellValuesProperty);

        public static void SetSelectedCellValues(DependencyObject obj, IList value)
            => obj.SetValue(SelectedCellValuesProperty, value);

        private static void OnSelectedCellValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (e.OldValue == null && e.NewValue != null)
                {
                    // Subscribe to the event when the property is first set
                    dataGrid.SelectedCellsChanged += DataGrid_SelectedCellsChanged;
                }
                else if (e.OldValue != null && e.NewValue == null)
                {
                    // Unsubscribe if the property is cleared
                    dataGrid.SelectedCellsChanged -= DataGrid_SelectedCellsChanged;
                }
            }
        }

        private static void DataGrid_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            var selectedCellValues = GetSelectedCellValues(dataGrid);

            if (selectedCellValues == null)
            {
                return;
            }

            selectedCellValues.Clear();

            if (!dataGrid.SelectedCells.Any())
            {
                return;
            }

            var cell = dataGrid.SelectedCells.Last();
            if (cell.IsValid)
            {
                var column = cell.Column;
                var item = cell.Item;

                if (column is DataGridBoundColumn boundColumn)
                {
                    if (boundColumn.Binding is System.Windows.Data.Binding binding)
                    {
                        var propertyNames = binding.Path.Path.Split(".");
                        object currentObject = item;
                        PropertyInfo propertyInfo;

                        for (int i = 0; i < propertyNames.Length; i++)
                        {
                            if (currentObject != null)
                            {
                                propertyInfo = currentObject.GetType().GetProperty(propertyNames[i]);
                                if (propertyInfo != null)
                                {
                                    currentObject = propertyInfo.GetValue(currentObject);
                                }
                            }
                        }

                        if (currentObject != null)
                        {
                            selectedCellValues.Add(currentObject);
                        }
                    }
                }

                SetSelectedCellValues(dataGrid, selectedCellValues);
                SetSelectedItem(dataGrid, item);
            }
        }
    }
}
