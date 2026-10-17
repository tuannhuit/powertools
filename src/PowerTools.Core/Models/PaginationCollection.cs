using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using PowerTools.Core.SharedServices;

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
            if (moreActions != null && moreActions.Any())
            {
                _additionalActions = moreActions;
            }
        }

        private List<T> _previousItems;
        private Func<IEnumerable<T>, IEnumerable<T>> _filterFunction;
        private Func<IEnumerable<T>, IEnumerable<T>> _sortFunction;

        /// <summary>
        /// Gets items source
        /// </summary>
        public List<T> ItemSource { get; private set; }

        private List<long> _filteredKeys;
        /// <summary>
        /// Gets filtered items from source
        /// </summary>
        public List<T> ItemSourceWithFilters { get; private set; }

        /// <summary>
        /// Gets item collection which is used for UI binding
        /// </summary>
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
            get => ItemSourceWithFilters.Count;
        }

        public int ItemStart
        {
            get => ItemSourceWithFilters.Count > 0 ? PageSize.GetValueOrDefault() * (Page - 1) + 1 : 0;
        }

        public int ItemEnd
        {
            get => ItemSourceWithFilters.Count > 0 ? ItemStart + Items.Count - 1 : 0;
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

        public bool IsFirstPage => ItemSourceWithFilters.Count > 0 ? _page == 1 : _page == 0;
        public bool IsLastPage => ItemSourceWithFilters.Count > 0 ? _page == _totalPage : _page == 0;

        public ICommand InvokeAction { get; private set; }

        public PaginationCollection()
            : this(new List<T>(), null, null)
        {

        }

        public PaginationCollection(IEnumerable<CustomAction> actions = null, ICommand refreshItemsCommand = null)
            : this(new List<T>(), actions, refreshItemsCommand)
        {

        }

        public PaginationCollection(IEnumerable<T> items, IEnumerable<CustomAction> actions = null, ICommand refreshItemsCommand = null)
        {
            if (items == null)
            {
                throw new Exception("The item source cannot be null");
            }

            Items = new ObservableCollection<T>();
            ItemSource = new List<T>(items);
            ItemSourceWithFilters = ItemSource.ToList();

            _previousItems = new List<T>();
            _page = 1;

            ApplyFilterToItemSource();
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
                },
                new PageAction
                {
                    Name = "RefreshItems",
                    Command = refreshItemsCommand
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
            if (foundAction == null)
            {
                LoggingService.Instance.Info($"Action '{actionName}' not found.");
                return;
            }

            if (foundAction.Command == null)
            {
                LoggingService.Instance.Info($"Action '{actionName}' does not have a command.");
                return;
            }

            foundAction.Command.Execute(null);
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

        public void SetFilter(Func<IEnumerable<T>, IEnumerable<T>> filterFunction)
        {
            _filterFunction = filterFunction;
        }

        public void SetSorter(Func<IEnumerable<T>, IEnumerable<T>> sortFunction)
        {
            _sortFunction = sortFunction;
        }

        /// <summary>
        /// Applies sort function to <see cref="ItemSourceWithFilters"/>
        /// </summary>
        /// <returns>Sorted results</returns>
        private List<T> ApplySort()
        {
            if (_sortFunction == null)
            {
                return ItemSourceWithFilters;
            }

            ItemSourceWithFilters = _sortFunction.Invoke(ItemSourceWithFilters).ToList();
            return ItemSourceWithFilters;
        }

        private List<T> ApplyFilterToItemSource()
        {
            if (_filterFunction == null)
            {
                ItemSourceWithFilters = ItemSource.ToList();
                return ItemSourceWithFilters;
            }

            ItemSourceWithFilters = _filterFunction.Invoke(ItemSource).ToList();
            return ItemSourceWithFilters;
        }

        /// <summary>
        /// Applies filters on input items and add filtered results to <see cref="ItemSourceWithFilters"/>
        /// </summary>
        /// <returns>Returns filtered results of input items</returns>
        private List<T> ApplyFilterAndAddRange(IEnumerable<T> items)
        {
            if (items == null)
            {
                return null;
            }

            List<T> filteredItems;

            if (_filterFunction == null)
            {
                filteredItems = items.ToList();
            }
            else
            {
                filteredItems = _filterFunction.Invoke(items).ToList();
            }

            ItemSourceWithFilters.AddRange(filteredItems);
            
            return filteredItems;
        }

        private int GetPageSize()
        {
            return _pageSize.GetValueOrDefault() > 0 ? _pageSize.GetValueOrDefault() : PageSizeDefault;
        }

        private void RecalculateItems()
        {
            _pageSize = GetPageSize();

            var itemCount = ItemSourceWithFilters.Count;
            _totalPage = itemCount / _pageSize.GetValueOrDefault();
            if (itemCount > _totalPage * _pageSize.GetValueOrDefault())
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

            var newItems = ItemSourceWithFilters.Skip(_pageSize.GetValueOrDefault() * (Page - 1)).Take(_pageSize.GetValueOrDefault()).ToList();

            if (newItems.Any())
            {
                var firstItem = newItems.First();
                if (firstItem is ITransformableObject)
                {
                    foreach (var item in newItems)
                    {
                        (item as ITransformableObject)?.Transform();
                    }
                }
            }

            var diffNewItems = newItems.Except(_previousItems);
            var diffPreviousItems = _previousItems.Except(newItems);

            if (!_previousItems.Any() && newItems.Any() || _previousItems.Any() && !newItems.Any() || diffNewItems.Any() || diffPreviousItems.Any())
            {
                Items = new ObservableCollection<T>(newItems);
                RaisePropertyChanged(nameof(Items));
            }

            RaisePropertyChanged(nameof(TotalPage));
            RaisePropertyChanged(nameof(TotalItems));
            RaisePropertyChanged(nameof(ItemStart));
            RaisePropertyChanged(nameof(ItemEnd));
            RaisePropertyChanged(nameof(PageInformation));
            RaisePropertyChanged(nameof(IsFirstPage));
            RaisePropertyChanged(nameof(IsLastPage));
        }

        public List<T> AddItem(T newItem)
        {
            return AddItemRange(new List<T>{ newItem });
        }

        public List<T> AddItemRange(IEnumerable<T> newItems)
        {
            if (newItems == null)
            {
                return null;
            }

            var addedItems = newItems as ICollection<T> ?? newItems.ToList();
            if (addedItems.Count == 0)
            {
                return new List<T>();
            }

            _previousItems = Items.ToList();
            ItemSource.AddRange(addedItems);

            var filteredItems = ApplyFilterAndAddRange(addedItems);

            ApplySort();
            RecalculateItems();

            return filteredItems;
        }

        public List<T> SetItems(IEnumerable<T> newItems)
        {
            if (newItems == null)
            {
                return null;
            }

            var itemSource = new List<T>(newItems);
            if (itemSource.Count == 0)
            {
                return new List<T>();
            }

            _previousItems = Items.ToList();
            ItemSource = itemSource;

            var filteredItems = ApplyFilterToItemSource();

            ApplySort();
            RecalculateItems();

            return filteredItems;
        }

        /// <summary>
        /// Applies filters and sort on <see cref="ItemSource"/>, then apply pagination.
        /// </summary>
        public void ApplyFilter()
        {
            _previousItems = Items.ToList();

            ApplyFilterToItemSource();
            ApplySort();
            RecalculateItems();
        }

        public void Clear()
        {
            _previousItems = new List<T>();
            ItemSource = new List<T>();
            ItemSourceWithFilters = new List<T>();

            if (Items != null)
            {
                Items.Clear();
            }

            RecalculateItems();
        }
    }
}
