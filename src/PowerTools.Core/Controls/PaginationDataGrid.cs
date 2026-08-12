using PowerTools.Core.Models;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PowerTools.Core.Controls
{
    public class PaginationDataGrid : DataGrid
    {
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public IList Filters
        {
            get { return (IList)GetValue(FiltersProperty); }
            set { SetValue(FiltersProperty, value); }
        }

        public IList ColumnSettings
        {
            get { return (IList)GetValue(ColumnSettingsProperty); }
            set { SetValue(ColumnSettingsProperty, value); }
        }

        public object Metadata
        {
            get { return (object)GetValue(MetadataProperty); }
            set { SetValue(MetadataProperty, value); }
        }

        public object HeaderSettings
        {
            get { return (object)GetValue(HeaderSettingsProperty); }
            set { SetValue(HeaderSettingsProperty, value); }
        }

        public object HeaderExtendContent
        {
            get { return (object)GetValue(HeaderExtendContentProperty); }
            set { SetValue(HeaderExtendContentProperty, value); }
        }

        public bool IsFiltersVisible
        {
            get { return (bool)GetValue(IsFiltersVisibleProperty); }
            set { SetValue(IsFiltersVisibleProperty, value); }
        }

        public bool IsHeaderSettingsEnable
        {
            get { return (bool)GetValue(IsHeaderSettingsEnableProperty); }
            set { SetValue(IsHeaderSettingsEnableProperty, value); }
        }

        public bool IsFiltersEnable
        {
            get { return (bool)GetValue(IsFiltersEnableProperty); }
            set { SetValue(IsFiltersEnableProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(PaginationDataGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty FiltersProperty = DependencyProperty.Register("Filters", typeof(IList), typeof(PaginationDataGrid), new FrameworkPropertyMetadata(null, OnFiltersChanged));
        public static readonly DependencyProperty ColumnSettingsProperty = DependencyProperty.Register("ColumnSettings", typeof(IList), typeof(PaginationDataGrid), new FrameworkPropertyMetadata(null, OnColumnSettingsChanged));
        public static readonly DependencyProperty ButtonActionsProperty = DependencyProperty.Register("ButtonActions", typeof(IList), typeof(PaginationDataGrid), new FrameworkPropertyMetadata(null, null));
        public static readonly DependencyProperty HeaderSettingsProperty = DependencyProperty.Register("HeaderSettings", typeof(object), typeof(PaginationDataGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty HeaderExtendContentProperty = DependencyProperty.Register("HeaderExtendContent", typeof(object), typeof(PaginationDataGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty IsFiltersVisibleProperty = DependencyProperty.Register("IsFiltersVisible", typeof(bool), typeof(PaginationDataGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty IsHeaderSettingsEnableProperty = DependencyProperty.Register("IsHeaderSettingsEnable", typeof(bool), typeof(PaginationDataGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty IsFiltersEnableProperty = DependencyProperty.Register("IsFiltersEnable", typeof(bool), typeof(PaginationDataGrid), new PropertyMetadata(null));
        public static readonly DependencyProperty MetadataProperty = DependencyProperty.Register("Metadata", typeof(object), typeof(PaginationDataGrid), new PropertyMetadata(null));

        private static void OnFiltersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (e.NewValue != null)
                {
                    SetIsFiltersEnable(d, true);

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
                                        SetFilterModel(dataGridColumn, filter);
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
                                            SetFilterModel(dataGridColumn, filter);
                                            break;
                                        }
                                    }
                                }
                            };
                        }
                    }
                }
                else if (e.OldValue != null && e.NewValue == null)
                {
                    SetIsFiltersEnable(d, false);
                }
            }
        }

        public static object GetColumnName(DependencyObject obj)
        {
            if(obj is PaginationDataGridTextColumn)
                return (object)obj.GetValue(PaginationDataGridTextColumn.NameProperty);

            if(obj is PaginationDataGridTemplateColumn)
                return (object)obj.GetValue(PaginationDataGridTemplateColumn.NameProperty);

            return null;
        }
        public static void SetFilterModel(DependencyObject obj, Criteria value)
        {
            if(obj is PaginationDataGridTextColumn)
                obj.SetValue(PaginationDataGridTextColumn.FilterModelProperty, value);

            if(obj is PaginationDataGridTemplateColumn)
                obj.SetValue(PaginationDataGridTemplateColumn.FilterModelProperty, value);
        }

        private static void OnColumnSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if (e.NewValue != null)
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

                                columnSetting.ValueCallbackHandler = (sender) =>
                                {
                                    var dataGridColumn = dataGrid.Columns.FirstOrDefault(p =>
                                        GetColumnName(p)?.ToString() == columnSetting.Name);
                                    if (dataGridColumn != null)
                                    {
                                        DisplayColumn(dataGridColumn, columnSetting);
                                    }
                                };
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

                                    columnSetting.ValueCallbackHandler = (sender) =>
                                    {
                                        var dataGridColumn = dataGrid.Columns.FirstOrDefault(p =>
                                            GetColumnName(p)?.ToString() == columnSetting.Name);
                                        if (dataGridColumn != null)
                                        {
                                            DisplayColumn(dataGridColumn, columnSetting);
                                        }
                                    };
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

        public static void SetIsHeaderSettingsEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsHeaderSettingsEnableProperty, value);
        }

        public static void SetIsFiltersEnable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsFiltersEnableProperty, value);
        }
    }
}
