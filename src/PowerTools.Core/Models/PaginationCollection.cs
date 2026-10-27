using PowerTools.Core.SharedServices;
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
                RaisePropertyChanged(nameof(DoSupportRefreshAction));
                RaisePropertyChanged(nameof(DoSupportExportAction));
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

        public bool DoSupportRefreshAction => Actions?.Any(p => p.Name == "RefreshItems" && p.Command != null) ?? false;
        public bool DoSupportExportAction => Actions?.Any(p => p.Name == "ExportItems" && p.Command != null) ?? false;

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

        private Func<IEnumerable<PaginationItem<T>>, IEnumerable<PaginationItem<T>>> _filterFunction;
        private Func<IEnumerable<PaginationItem<T>>, IEnumerable<PaginationItem<T>>> _sortFunction;

        #region ItemSource
        /// <summary>
        /// The source of items. Use a dictionary object to store data.
        /// It can be accessed by ids
        /// </summary>
        private Dictionary<long, PaginationItem<T>> _itemSource = new Dictionary<long, PaginationItem<T>>();

        /// <summary>
        /// Gets items source
        /// </summary>
        public IEnumerable<T> ItemSource => _itemSource.Values.Select(p => p.Value);
        #endregion

        #region FilteredItems
        /// <summary>
        /// The list of filtered item ids
        /// </summary>
        private List<long> _filteredIds = new List<long>();
        /// <summary>
        /// Gets filtered items from source
        /// </summary>
        public IEnumerable<T> FilteredItems => _filteredIds.Select(k => _itemSource[k].Value);

        /// <summary>
        /// Gets filtered items from source
        /// </summary>
        protected IEnumerable<PaginationItem<T>> FilteredPaginationItems => _filteredIds.Select(k => _itemSource[k]);

        #endregion

        private List<long> _itemIds = new List<long>();
        /// <summary>
        /// Gets item collection which is used for UI binding
        /// </summary>
        public ObservableCollection<T> Items => new ObservableCollection<T>(_itemIds.Select(id => _itemSource[id].Value));

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
            get => FilteredItems.Count();
        }

        public int ItemStart
        {
            get => FilteredItems.Any() && (Page - 1) >= 0 ? PageSize.GetValueOrDefault() * (Page - 1) + 1 : 0;
        }

        public int ItemEnd
        {
            get => FilteredItems.Any() && (Items.Count - 1) >= 0 ? ItemStart + Items.Count - 1 : 0;
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

        public bool IsFirstPage => FilteredItems.Any() ? _page == 1 : _page == 0;
        public bool IsLastPage => FilteredItems.Any() ? _page == _totalPage : _page == 0;

        public ICommand InvokeAction { get; private set; }

        public PaginationCollection()
            : this(new List<T>())
        {

        }

        public PaginationCollection(IEnumerable<CustomAction> actions = null, ICommand refreshItemsCommand = null, ICommand exportItemsCommand = null)
            : this(new List<T>(), actions, refreshItemsCommand, exportItemsCommand)
        {

        }

        public PaginationCollection(IEnumerable<T> items, IEnumerable<CustomAction> actions = null, ICommand refreshItemsCommand = null, ICommand exportItemsCommand = null)
        {
            if (items == null)
            {
                throw new Exception("The item source cannot be null");
            }

            _page = 1;

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
                },
                new PageAction
                {
                    Name = "ExportItems",
                    Command = exportItemsCommand
                }
            };

            if (actions != null && actions.Any())
            {
                Actions.AddRange(actions);
            }

            CacheActionProperties();
            InvokeAction = new DelegateCommand<string>(OnInvokeAction);

            SetItems(items);
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
            _page += 1;
            MoveToPage(Page);
        }

        public void MovePreviousPage()
        {
            _page -= 1;
            MoveToPage(Page);
        }

        public void SetFilter(Func<IEnumerable<PaginationItem<T>>, IEnumerable<PaginationItem<T>>> filterFunction)
        {
            _filterFunction = filterFunction;
        }

        public void SetSorter(Func<IEnumerable<PaginationItem<T>>, IEnumerable<PaginationItem<T>>> sortFunction)
        {
            _sortFunction = sortFunction;
        }

        /// <summary>
        /// Applies sort function to <see cref="FilteredItems"/>
        /// </summary>
        /// <returns>Sorted results</returns>
        private void ApplySort()
        {
            if (_sortFunction == null)
            {
                return;
            }

            _filteredIds = _sortFunction.Invoke(FilteredPaginationItems).Select(p => p.Id).ToList();
        }

        private void ApplyFilterToItemSource()
        {
            if (_filterFunction == null)
            {
                return;
            }

            _filteredIds = _filterFunction.Invoke(_itemSource.Values).Select(p => p.Id).ToList();
        }

        private int GetPageSize()
        {
            return _pageSize.GetValueOrDefault() > 0 ? _pageSize.GetValueOrDefault() : PageSizeDefault;
        }

        private void RecalculateItems()
        {
            _pageSize = GetPageSize();

            var itemCount = _filteredIds.Count;
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

            var newIds = _filteredIds.Skip(_pageSize.GetValueOrDefault() * (Page - 1)).Take(_pageSize.GetValueOrDefault());
            if (newIds.Any())
            {
                var firstItem = newIds.First();
                if (firstItem is ITransformableObject)
                {
                    foreach (var item in newIds)
                    {
                        (_itemSource[item] as ITransformableObject)?.Transform();
                    }
                }
            }

            if (_itemIds.Count != newIds.Count() || !_itemIds.Any() && !newIds.Any() || !_itemIds.SequenceEqual(newIds))
            {
                _itemIds = newIds.ToList();
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

        public bool ContainItemById(long itemKey)
        {
            return _itemSource.ContainsKey(itemKey);
        }
        public bool RemoveItemById(long itemKey)
        {
            if (ContainItemById(itemKey))
                return _itemSource.Remove(itemKey);

            return false;
        }

        public List<T> AddItem(T newItem, bool autoRefresh = true)
        {
            return AddItemRange(new List<T> { newItem }, autoRefresh);
        }

        public List<T> AddItemRange(IEnumerable<T> newItems, bool autoRefresh = true)
        {
            if (newItems == null || !newItems.Any())
            {
                return new List<T>();
            }

            var paginationItems = new List<PaginationItem<T>>();
            foreach (var newItem in newItems)
            {
                var newPaginationItem = new PaginationItem<T>(newItem);
                paginationItems.Add(newPaginationItem);
                _itemSource.Add(newPaginationItem.Id, newPaginationItem);
            }

            IEnumerable<PaginationItem<T>> filteredPaginationItems;
            if (_filterFunction == null)
            {
                filteredPaginationItems = paginationItems;
            }
            else
            {
                filteredPaginationItems = _filterFunction.Invoke(paginationItems);
            }

            _filteredIds.AddRange(filteredPaginationItems.Select(p => p.Id));

            if (autoRefresh)
            {
                ApplySort();
                RecalculateItems();
            }

            return filteredPaginationItems.Select(p => p.Value).ToList();
        }

        public List<T> SetItems(IEnumerable<T> newItems)
        {
            _filteredIds = new List<long>();
            _itemIds = new List<long>();
            _itemSource = new Dictionary<long, PaginationItem<T>>();

            return AddItemRange(newItems);
        }

        /// <summary>
        /// Applies filters and sort on <see cref="ItemSource"/>, then apply pagination.
        /// </summary>
        public void Invalidate()
        {
            ApplyFilterToItemSource();
            ApplySort();
            RecalculateItems();
        }

        public void Clear()
        {
            _itemIds = new List<long>();
            _filteredIds = new List<long>();
            _itemSource = new Dictionary<long, PaginationItem<T>>();

            RecalculateItems();
        }
    }
}
