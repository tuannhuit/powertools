using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerTools.Core.Models
{
    public class FilterCriteria : BindableBase
    {
        private Dictionary<string, Criteria> _criteriaDictionary;

        public Dictionary<string, Criteria> CriteriaDictionary
        {
            get => _criteriaDictionary;
            set
            {
                _criteriaDictionary = value;
                RaisePropertyChanged();
            }
        }

        public FilterCriteria(IEnumerable<Criteria> criteriaCollection)
        {
            if (criteriaCollection == null)
            {
                throw new ArgumentException("Criteria collection can not be null");
            }

            var criteriaEnumerable = criteriaCollection as Criteria[] ?? criteriaCollection.ToArray();
            var groups = criteriaEnumerable.GroupBy(p => p.Name).Where(p => p.Count() > 1);
            if (groups.Any())
            {
                throw new ArgumentException("Found duplicated criteria names in collection");
            }

            CriteriaDictionary = criteriaEnumerable.ToDictionary(p => p.Name, c => c);
        }
    }
}