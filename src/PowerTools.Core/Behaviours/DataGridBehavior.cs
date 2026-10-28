using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PowerTools.Core.Behaviours
{
    public static class DataGridBehavior
    {
        #region Header Property

        public static object GetHeader(DependencyObject obj)
        {
            return (object)obj.GetValue(HeaderProperty);
        }

        public static void SetHeader(DependencyObject obj, object value)
        {
            obj.SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.RegisterAttached(
            "Header",
            typeof(object),
            typeof(DataGridBehavior),
            new FrameworkPropertyMetadata(null));

        #endregion

        #region SelectedItem Property

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.RegisterAttached(
            "SelectedItem",
            typeof(object),
            typeof(DataGridBehavior),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedCellValuesChanged));

        public static object GetSelectedItem(DependencyObject obj)
        {
            return (object)obj.GetValue(SelectedItemProperty);
        }

        public static void SetSelectedItem(DependencyObject obj, object value)
        {
            obj.SetValue(SelectedItemProperty, value);
        }

        #endregion

        #region SelectedCellValues Property

        public static readonly DependencyProperty SelectedCellValuesProperty = DependencyProperty.RegisterAttached(
           "SelectedCellValues",
           typeof(IList),
           typeof(DataGridBehavior),
           new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
               OnSelectedCellValuesChanged));

        public static IList GetSelectedCellValues(DependencyObject obj)
        {
            return (IList)obj.GetValue(SelectedCellValuesProperty);
        }

        public static void SetSelectedCellValues(DependencyObject obj, IList value)
        {
            obj.SetValue(SelectedCellValuesProperty, value);
        }

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

        #endregion

        #region FixAutoExpansion Property
        public static readonly DependencyProperty FixAutoExpansionProperty =
            DependencyProperty.RegisterAttached(
                "FixAutoExpansion",
                typeof(bool),
                typeof(DataGridBehavior),
                new PropertyMetadata(false, OnFixAutoExpansionChanged));

        public static void SetFixAutoExpansion(DependencyObject obj, bool value)
        {
            obj.SetValue(FixAutoExpansionProperty, value);
        }

        public static bool GetFixAutoExpansion(DependencyObject obj)
        {
            return (bool)obj.GetValue(FixAutoExpansionProperty);
        }

        private static void OnFixAutoExpansionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGridRow row)
            {
                if ((bool)e.NewValue)
                {
                    // Hook into Preview Mouse down to catch the manual drag initialization
                    row.PreviewMouseLeftButtonDown += Row_PreviewMouseLeftButtonDown;
                }
                else
                {
                    row.PreviewMouseLeftButtonDown -= Row_PreviewMouseLeftButtonDown;
                }
            }
        }

        private static void Row_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row)
            {
                // Verify the user clicked on the Row Header or a Resize Grip area
                DependencyObject originalSource = e.OriginalSource as DependencyObject;
                if (IsVisualChildOf<DataGridRowHeader>(originalSource) || IsVisualChildOf<Thumb>(originalSource))
                {
                    // 1. Lift the MaxHeight constraint immediately so WPF layout permits stretching larger
                    row.MaxHeight = double.PositiveInfinity;

                    // 2. Set Height to its current rendered actual height so it doesn't snap to "Auto"
                    row.Height = row.ActualHeight;

                    // 3. Listen for when they release the mouse to lock the final chosen height
                    Window parentWindow = Window.GetWindow(row);
                    if (parentWindow != null)
                    {
                        MouseButtonEventHandler releaseHandler = null;
                        releaseHandler = (s, args) =>
                        {
                            // Permanently lock the height to where they dropped it
                            row.Height = row.ActualHeight;

                            // Unsubscribe from global window mouse up
                            parentWindow.PreviewMouseLeftButtonUp -= releaseHandler;
                        };
                        parentWindow.PreviewMouseLeftButtonUp += releaseHandler;
                    }
                }
            }
        }

        // Helper to check if the clicked visual element is inside the Row Header / Grip
        private static bool IsVisualChildOf<T>(DependencyObject element) where T : DependencyObject
        {
            while (element != null)
            {
                if (element is T) return true;
                element = VisualTreeHelper.GetParent(element);
            }
            return false;
        }
        #endregion
    }
}
