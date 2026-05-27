using PowerTools.Core.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

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
            new PropertyMetadata(null));

        #endregion

        #region Header Property

        public static object GetColumnName(DependencyObject obj)
        {
            return (object)obj.GetValue(ColumnNameProperty);
        }

        public static void SetColumnName(DependencyObject obj, object value)
        {
            obj.SetValue(ColumnNameProperty, value);
        }

        public static readonly DependencyProperty ColumnNameProperty = DependencyProperty.RegisterAttached(
            "ColumnName",
            typeof(object),
            typeof(DataGridBehavior),
            new PropertyMetadata(null));

        #endregion

        #region ColumnFilterModel Property

        public static Criteria GetColumnFilterModel(DependencyObject obj)
        {
            return (Criteria)obj.GetValue(ColumnFilterModelProperty);
        }

        public static void SetColumnFilterModel(DependencyObject obj, Criteria value)
        {
            obj.SetValue(ColumnFilterModelProperty, value);
        }

        public static readonly DependencyProperty ColumnFilterModelProperty = DependencyProperty.RegisterAttached(
            "ColumnFilterModel",
            typeof(Criteria),
            typeof(DataGridBehavior),
            new PropertyMetadata(null));

        #endregion

        #region IsFiltersVisible Property

        public static bool GetIsFiltersVisible(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsFiltersVisibleProperty);
        }

        public static void SetIsFiltersVisible(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFiltersVisibleProperty, value);
        }

        public static readonly DependencyProperty IsFiltersVisibleProperty = DependencyProperty.RegisterAttached(
            "IsFiltersVisible",
            typeof(bool),
            typeof(DataGridBehavior),
            new PropertyMetadata(false));

        #endregion

        #region IsFiltersEnable Property

        public static object GetIsHeaderSettingsEnable(DependencyObject obj)
        {
            return (object)obj.GetValue(IsHeaderSettingsEnableProperty);
        }

        public static void SetIsHeaderSettingsEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHeaderSettingsEnableProperty, value);
        }

        public static readonly DependencyProperty IsHeaderSettingsEnableProperty = DependencyProperty.RegisterAttached(
            "IsHeaderSettingsEnable",
            typeof(bool),
            typeof(DataGridBehavior),
            new PropertyMetadata(null));

        #endregion

        #region Filters Property

        public static IList GetFilters(DependencyObject obj)
        {
            return (IList)obj.GetValue(FiltersProperty);
        }

        public static void SetFilters(DependencyObject obj, IList value)
        {
            obj.SetValue(FiltersProperty, value);
        }

        public static readonly DependencyProperty FiltersProperty = DependencyProperty.RegisterAttached(
            "Filters",
            typeof(IList),
            typeof(DataGridBehavior),
            new PropertyMetadata(OnFiltersChanged));

        private static void OnFiltersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (e.OldValue == null && e.NewValue != null)
                {
                    var filters = (IEnumerable<Criteria>)e.NewValue;
                    if (filters != null && filters.Any())
                    {
                        if (dataGrid.Columns.Any())
                        {
                            foreach (var dataGridColumn in dataGrid.Columns)
                            {
                                foreach (var filter in filters)
                                {
                                    if (GetColumnName(dataGridColumn)?.ToString() == filter.Name)
                                    {
                                        SetColumnFilterModel(dataGridColumn, filter);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            dataGrid.Columns.CollectionChanged += (sender, args) =>
                            {
                                foreach (var dataGridColumn in dataGrid.Columns)
                                {
                                    foreach (var filter in filters)
                                    {
                                        if (GetColumnName(dataGridColumn)?.ToString() == filter.Name)
                                        {
                                            SetColumnFilterModel(dataGridColumn, filter);
                                            break;
                                        }
                                    }
                                }
                            };
                        }
                    }
                }
            }
        }

        #endregion

        #region ColumnSettings Property

        public static IList GetColumnSettings(DependencyObject obj)
        {
            return (IList)obj.GetValue(ColumnSettingsProperty);
        }

        public static void SetColumnSettings(DependencyObject obj, IList value)
        {
            obj.SetValue(ColumnSettingsProperty, value);
        }

        public static readonly DependencyProperty ColumnSettingsProperty = DependencyProperty.RegisterAttached(
            "ColumnSettings",
            typeof(IList),
            typeof(DataGridBehavior),
            new PropertyMetadata(OnColumnSettingsChanged));

        private static void OnColumnSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (e.OldValue == null && e.NewValue != null)
                {
                    SetIsHeaderSettingsEnable(d, true);

                    var columnSettings = (IEnumerable<Criteria>)e.NewValue;
                    if (columnSettings != null && columnSettings.Any())
                    {
                        if (dataGrid.Columns.Any())
                        {
                            foreach (var columnSetting in columnSettings)
                            {
                                foreach (var column in dataGrid.Columns)
                                {
                                    DisplayColumn(column, columnSetting);
                                }

                                columnSetting.SetValueChangedHandler((s, e) =>
                                {
                                    var dataGridColumn = dataGrid.Columns.FirstOrDefault(p => GetColumnName(p)?.ToString() == columnSetting.Name);
                                    if (dataGridColumn != null)
                                    {
                                        DisplayColumn(dataGridColumn, columnSetting);
                                    }
                                });
                            }
                        }
                        else
                        {
                            dataGrid.Columns.CollectionChanged += (sender, args) =>
                            {
                                foreach (var columnSetting in columnSettings)
                                {
                                    foreach (var column in dataGrid.Columns)
                                    {
                                        DisplayColumn(column, columnSetting);
                                    }

                                    columnSetting.SetValueChangedHandler((s, e) =>
                                    {
                                        var dataGridColumn = dataGrid.Columns.FirstOrDefault(p => GetColumnName(p)?.ToString() == columnSetting.Name);
                                        if (dataGridColumn != null)
                                        {
                                            DisplayColumn(dataGridColumn, columnSetting);
                                        }
                                    });
                                }
                            };
                        }
                    }
                }
                else if (e.OldValue != null && e.NewValue == null)
                {
                    SetIsHeaderSettingsEnable(d, false);
                }
            }
        }

        private static void DisplayColumn(DataGridColumn dataGridColumn, Criteria criteria)
        {
            var ColumnName = GetColumnName(dataGridColumn)?.ToString();
            if (ColumnName != null && ColumnName == criteria.Name)
            {
                dataGridColumn.Visibility = (bool)criteria.Value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region HeaderSettings Property

        public static object GetHeaderSettings(DependencyObject obj)
        {
            return (object)obj.GetValue(HeaderSettingsProperty);
        }

        public static void SetHeaderSettings(DependencyObject obj, object value)
        {
            obj.SetValue(HeaderSettingsProperty, value);
        }

        public static readonly DependencyProperty HeaderSettingsProperty = DependencyProperty.RegisterAttached(
            "HeaderSettings",
            typeof(object),
            typeof(DataGridBehavior),
            new PropertyMetadata(null, OnHeaderSettingsChanged));

        private static void OnHeaderSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (e.OldValue == null && e.NewValue != null)
                {
                    SetIsHeaderSettingsEnable(d, true);
                }
                else if (e.OldValue != null && e.NewValue == null)
                {
                    SetIsHeaderSettingsEnable(d, false);
                }
            }
        }

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
    }
}
