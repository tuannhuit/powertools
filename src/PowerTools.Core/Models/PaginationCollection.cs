using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PowerTools.Core.Models
{
    public class PaginationCollection<T> : BindableBase
    {
        public static readonly int PAGE_SIZE = 500;
        public static readonly int ACTIVE_ACTION_SIZE = 3;

        private ObservableCollection<CustomAction> _actions;
        private List<CustomAction> _activeActions;
        private List<CustomAction> _additionalActions;
        private List<CustomAction> _nonPageActions;

        public ObservableCollection<CustomAction> Actions
        {
            get => _actions;
            set
            {
                _actions = value;
                CacheActionProperties();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(ActiveActions));
                RaisePropertyChanged(nameof(AdditionalActions));
            }
        }

        public List<CustomAction> ActiveActions
        {
            get => _activeActions;
            private set => SetProperty(ref _activeActions, value);
        }

        public List<CustomAction> AdditionalActions
        {
            get => _additionalActions;
            private set => SetProperty(ref _additionalActions, value);
        }

        private void CacheActionProperties()
        {
            if (Actions == null)
            {
                _nonPageActions = new List<CustomAction>();
                _activeActions = null;
                _additionalActions = null;
                return;
            }

            _nonPageActions = Actions.Where(p => p is not PageAction).ToList().OrderBy(p => p.Name).ToList();

            var activeActionCount = Math.Min(ACTIVE_ACTION_SIZE, _nonPageActions.Count);
            _activeActions = _nonPageActions.Take(activeActionCount).ToList();

            var moreActions = _nonPageActions.Skip(activeActionCount).ToList();
            if(moreActions!=null && moreActions.Any())
            {
                _additionalActions = moreActions;
            }
        }

        private List<T> _previousItems;

        public List<T> ItemSource { get; private set; }

        public ObservableCollection<T> Items { get; private set; }

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
            get => ItemSource.Count();
        }

        public int ItemStart
        {
            get => ItemSource.Any() ? PAGE_SIZE * (Page - 1) + 1 : 0;
        }

        public int ItemEnd
        {
            get => ItemSource.Any() ? ItemStart + Items.Count - 1 : 0;
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
                _previousItems = Items.ToList();
                _page = value;

                RaisePropertyChanged();
                RecalculateItems();
            }
        }

        public int PageSizeDefault => PAGE_SIZE;

        /// <summary>
        /// The index of current page
        /// </summary>
        private int? _pageSize;
        public int? PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                RaisePropertyChanged();
                RecalculateItems();
            }
        }

        public bool IsFirstPage => ItemSource.Any() ? _page == 1 : _page == 0;
        public bool IsLastPage => ItemSource.Any() ? _page == _totalPage : _page == 0;

        public ICommand InvokeAction { get; set; }

        public PaginationCollection(IEnumerable<CustomAction> actions = null)
            : this(new List<T>(), actions)
        {

        }

        public PaginationCollection(IEnumerable<T> items, IEnumerable<CustomAction> actions = null)
        {
            if (items == null)
            {
                throw new Exception("The item source cannot be null");
            }

            Items = new ObservableCollection<T>();
            ItemSource = new List<T>(items);

            _previousItems = new List<T>();
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

            if (actions != null && actions.Any())
            {
                Actions.AddRange(actions);
            }
            CacheActionProperties();
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
            _previousItems = Items.ToList();
            _page += 1;
            MoveToPage(Page);
        }

        public void MovePreviousPage()
        {
            _previousItems = Items.ToList();
            _page -= 1;
            MoveToPage(Page);
        }

        private int GetPageSize()
        {
            return _pageSize.GetValueOrDefault() > 0 ? _pageSize.GetValueOrDefault() : PageSizeDefault;
        }

        private void RecalculateItems()
        {
            var pageSize = GetPageSize();
            var itemCount = ItemSource.Count;
            _totalPage = itemCount / pageSize;
            if (itemCount > _totalPage * pageSize)
            {
                _totalPage += 1;
            }

            if (_page < 1)
            {
                _page = 1;
            }

            if (_page > _totalPage)
            {
                _page = _totalPage;
            }

            var newItems = ItemSource.Skip(pageSize * (Page - 1)).Take(pageSize).ToList();

            if (newItems.Any())
            {
                var firstItem = newItems.First();
                if (firstItem is ITransformableObject)
                {
                    foreach (var item in newItems)
                    {
                        ((ITransformableObject)item).Transform();
                    }
                }
            }

            var diffNewItems = newItems.Except(_previousItems);
            var diffPreviousItems = _previousItems.Except(newItems);

            if (!_previousItems.Any() && newItems.Any() || _previousItems.Any() && !newItems.Any() || diffNewItems.Any() || diffPreviousItems.Any())
            {
                Items.Clear();
                Items.AddRange(newItems);
                //Items = new ObservableCollection<T>(newItems);
                //RaisePropertyChanged(nameof(Items));
            }

            RaisePropertyChanged(nameof(TotalPage));
            RaisePropertyChanged(nameof(TotalItems));
            RaisePropertyChanged(nameof(ItemStart));
            RaisePropertyChanged(nameof(ItemEnd));
            RaisePropertyChanged(nameof(PageInformation));
            RaisePropertyChanged(nameof(IsFirstPage));
            RaisePropertyChanged(nameof(IsLastPage));
        }

        public void AddItem(T newItem)
        {
            if (ItemSource == null)
            {
                ItemSource = new List<T>();
            }

            _previousItems = Items.ToList();
            ItemSource.Add(newItem);

            RecalculateItems();
        }

        public void AddItemRange(IEnumerable<T> newItems)
        {
            if (newItems == null)
            {
                return;
            }

            if (ItemSource == null)
            {
                ItemSource = new List<T>();
            }

            _previousItems = Items.ToList();
            ItemSource.AddRange(newItems);

            RecalculateItems();
        }

        public void SetItems(IEnumerable<T> newItems)
        {
            if (newItems == null)
            {
                return;
            }

            _previousItems = Items.ToList();

            ItemSource = new List<T>(newItems);

            RecalculateItems();
        }

        public void Clear()
        {
            if (_previousItems != null)
            {
                _previousItems.Clear();
            }

            if (ItemSource != null)
            {
                ItemSource.Clear();
            }

            if (Items != null)
            {
                Items.Clear();
            }

            RecalculateItems();
        }
    }
}
