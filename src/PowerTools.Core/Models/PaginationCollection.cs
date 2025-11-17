using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Prism.Commands;

namespace PowerTools.Core.Models
{
    public class PaginationCollection<T> : BindableBase
    {
        public static readonly int PAGE_SIZE = 1000;

        private ObservableCollection<CustomAction> _actions;

        public ObservableCollection<CustomAction> Actions
        {
            get => _actions;
            set
            {
                _actions = value;
                RaisePropertyChanged();
            }
        }

        public CustomAction Action1
        {
            get
            {
                if (Actions == null) return null;

                var actions = Actions.Where(p => p is not PageAction);
                if (!actions.Any()) return null;
                return actions.ElementAt(0);
            }
        }

        public CustomAction Action2
        {
            get
            {
                if (Actions == null) return null;

                var actions = Actions.Where(p => p is not PageAction);
                if (actions.Count() < 2) return null;
                return actions.ElementAt(1);
            }
        }

        public CustomAction Action3
        {
            get
            {
                if (Actions == null) return null;

                var actions = Actions.Where(p => p is not PageAction);
                if (actions.Count() < 3) return null;
                return actions.ElementAt(2);
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

        public bool IsFirstPage => _itemSource.Any() ? _page == 1 : _page == 0;
        public bool IsLastPage => _itemSource.Any() ? _page == _totalPage : _page == 0;

        public ICommand InvokeAction { get; set; }

        public PaginationCollection(IEnumerable<T> items, IEnumerable<CustomAction> actions = null)
        {
            if (items == null)
            {
                throw new Exception("The item source cannot be null");
            }
            _itemSource = items;
            _page = 1;

            RecalculateItems();

            Actions = new ObservableCollection<CustomAction>
            {
                new PageAction
                {
                    Name = "MoveNextPage",
                    Command = new DelegateCommand(OnMoveNext)
                },
                new PageAction
                {
                    Name = "MovePreviousPage",
                    Command = new DelegateCommand(OnMovePrevious)
                }
            };
            Actions.AddRange(actions);
            InvokeAction = new DelegateCommand<string>(OnInvokeAction);
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
                _page = 1;
            }

            if (movePage > TotalPage)
            {
                _page = TotalPage;
            }

            RecalculateItems();
        }

        public void MoveNextPage()
        {
            _page += 1;
            MoveToPage(Page);
        }

        public void MovePreviousPage()
        {
            _page -= 1;
            MoveToPage(Page);
        }

        private void RecalculateItems()
        {
            _totalPage = _itemSource.Count() / PAGE_SIZE;
            if (_itemSource.Count() > _totalPage * PAGE_SIZE)
            {
                // Calculate total pages and Raise UI event
                TotalPage += 1;
            }

            if (_page < 1)
            {
                _page = 1;
            }

            if (_page > _totalPage)
            {
                _page = _totalPage;
            }

            RaisePropertyChanged("Items");
            RaisePropertyChanged("TotalItems");
            RaisePropertyChanged("ItemStart");
            RaisePropertyChanged("ItemEnd");
            RaisePropertyChanged("PageInformation");
            RaisePropertyChanged("IsFirstPage");
            RaisePropertyChanged("IsLastPage");
        }

        public void AddItem(T newItem)
        {
            if (_itemSource == null)
            {
                _itemSource = new List<T>();
            }
            _itemSource.Append(newItem);

            RecalculateItems();
        }

        public void AddItemRange(IEnumerable<T> newItems)
        {
            if (_itemSource == null)
            {
                _itemSource = new List<T>();
            }

            foreach (var newItem in newItems)
            {
                _itemSource.Append(newItem);
            }

            RecalculateItems();
        }

        public void SetItems(IEnumerable<T> newItems)
        {
            _itemSource = new List<T>();

            foreach (var newItem in newItems)
            {
                _itemSource.Append(newItem);
            }

            RecalculateItems();
        }
    }
}
