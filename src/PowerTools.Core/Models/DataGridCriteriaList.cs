using System;
using System.Collections.Generic;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;

namespace PowerTools.Core.Models
{
    public class DataGridCriteriaList : BindableBase
    {
        private ObservableCollection<Criteria> _filters;

        public ObservableCollection<Criteria> Filters
        {
            get => _filters;
            set
            {
                _filters = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Criteria> _columnSettings;

        public ObservableCollection<Criteria> ColumnSettings
        {
            get => _columnSettings;
            set
            {
                _columnSettings = value;
                RaisePropertyChanged();
            }
        }

        public DataGridCriteriaList(IEnumerable<Criteria> filterCriteria, IEnumerable<Criteria> columnSettings)
        {
            if (filterCriteria == null)
            {
                throw new ArgumentException("Filter list can not be null");
            }

            if (columnSettings == null)
            {
                throw new ArgumentException("Column Settings list can not be null");
            }

            var filterGroups = filterCriteria.GroupBy(p => p.Name).Where(p => p.Count() > 1);
            if (filterGroups.Any())
            {
                throw new ArgumentException("Found duplicated filter names in list");
            }

            var columnGroups = columnSettings.GroupBy(p => p.Name).Where(p => p.Count() > 1);
            if (columnGroups.Any())
            {
                throw new ArgumentException("Found duplicated column setting names in list");
            }

            Filters = new ObservableCollection<Criteria>(filterCriteria);
            ColumnSettings = new ObservableCollection<Criteria>(columnSettings);
        }
    }
}
