using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Prism.Commands;

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

        public int TotalItems
        {
            get => _itemSource.Count();
        }

        public int ItemStart
        {
            get => _itemSource.Any() ? PAGE_SIZE * (Page - 1) + 1 : 0;
        }

        public int ItemEnd
        {
            get => _itemSource.Any() ? ItemStart + Items.Count - 1 : 0;
        }

        public string PageInformation => $"{ItemStart}-{ItemEnd} of {TotalItems}";

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

        public ICommand InvokeAction { get; set; }

        public DataGridModel(IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new Exception("The item source cannot be null");
            }
            _itemSource = items;
            _page = 1;

            RecalculateItems();

            Actions = new ObservableCollection<DataGridAction>
            {
                new DataGridAction
                {
                    Name = "MoveNextPage",
                    Command = new DelegateCommand(OnMoveNext)
                },
                new DataGridAction
                {
                    Name = "MovePreviousPage",
                    Command = new DelegateCommand(OnMoveNext)
                }
            };
            InvokeAction = new DelegateCommand<string>(OnInvokeAction);
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

        private void OnMoveNext()
        {
            MoveNextPage();
        }

        private void OnMovePrevious()
        {
            MovePreviousPage();
        }

        private void OnInvokeAction(string actionName)
        {
            var foundAction = Actions.FirstOrDefault(p => p.Name == actionName);
            foundAction?.Command.Execute(null);
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
            RaisePropertyChanged("TotalItems");
            RaisePropertyChanged("ItemStart");
            RaisePropertyChanged("ItemEnd");
            RaisePropertyChanged("PageInformation");
        }
    }
}
