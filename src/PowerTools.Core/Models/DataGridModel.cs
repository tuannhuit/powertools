using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PowerTools.Core.Models
{
    public class DataGridModel<T> : BindableBase
    {
        public static readonly int PAGE_SIZE = 1000;

        private ObservableCollection<DataGridAction> _actions;

        public ObservableCollection<DataGridAction> Actions
        {
            get => _actions;
            set
            {
                _actions = value;
                RaisePropertyChanged();
            }
        }

        public DataGridAction Action1
        {
            get
            {
                if (Actions == null || !Actions.Any()) return null;
                return Actions[0];
            }
        }

        public DataGridAction Action2
        {
            get
            {
                if (Actions == null || Actions.Count < 1) return null;
                return Actions[1];
            }
        }

        public DataGridAction Action3
        {
            get
            {
                if (Actions == null || Actions.Count < 2) return null;
                return Actions[2];
            }
        }

        private IEnumerable<T> _itemSource;
        public ObservableCollection<T> Items => new ObservableCollection<T>(_itemSource.Skip(PAGE_SIZE * (Page - 1)).Take(PAGE_SIZE));

        /// <summary>
        /// Total number of pages
        /// </summary>
        private int _totalPage;
        public int TotalPage
        {
            get => _totalPage;
            private set
            {
                _totalPage = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// The index of current page
        /// </summary>
        private int _page;
        public int Page
        {
            get => _page;
            set
            {
                _page = value;
                RaisePropertyChanged();
                RecalculateItems();
            }
        }

        public DataGridModel(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new Exception("The item source cannot be null");
            }
            _itemSource = items;
            _page = 1;

            RecalculateItems();

            Actions = new ObservableCollection<DataGridAction>();
        }

        public DataGridModel(ObservableCollection<T> items)
        {
            if (items == null)
            {
                throw new Exception("The item source cannot be null");
            }
            _itemSource = items;
            _page = 1;

            RecalculateItems();

            Actions = new ObservableCollection<DataGridAction>();
        }

        public void MoveToPage(int movePage)
        {
            if (movePage < 1)
            {
                Page = 1;
            }

            if (movePage > TotalPage)
            {
                Page = TotalPage;
            }

            RecalculateItems();
        }

        public void MoveNextPage()
        {
            Page += 1;
            MoveToPage(Page);
        }

        public void MovePreviousPage()
        {
            Page -= 1;
            MoveToPage(Page);
        }

        private void RecalculateItems()
        {
            _totalPage = _itemSource.Count() % PAGE_SIZE;
            if (_itemSource.Count() > _totalPage % PAGE_SIZE)
            {
                // Calculate total pages and Raise UI event
                TotalPage += 1;
            }

            if (Page < 1)
            {
                Page = 1;
            }

            if (Page > TotalPage)
            {
                Page = TotalPage;
            }

            RaisePropertyChanged("Items");
        }
    }
}
