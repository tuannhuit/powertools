using PowerTools.Core.SharedServices;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PowerTools.Core.Models
{
    public class PaginationCollection<T> : BindableBase
    {
        public static readonly int PAGE_SIZE = 100;

        private ObservableCollection<CustomAction> _actions;
        private CustomAction _action1;
        private CustomAction _action2;
        private CustomAction _action3;
        private List<CustomAction> _nonPageActions;

        public ObservableCollection<CustomAction> Actions
        {
            get => _actions;
            set
            {
                _actions = value;
                CacheActionProperties();
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Action1));
                RaisePropertyChanged(nameof(Action2));
                RaisePropertyChanged(nameof(Action3));
            }
        }

        public CustomAction Action1
        {
            get => _action1;
            private set => SetProperty(ref _action1, value);
        }

        public CustomAction Action2
        {
            get => _action2;
            private set => SetProperty(ref _action2, value);
        }

        public CustomAction Action3
        {
            get => _action3;
            private set => SetProperty(ref _action3, value);
        }

        private void CacheActionProperties()
        {
            if (Actions == null)
            {
                _nonPageActions = new List<CustomAction>();
                _action1 = null;
                _action2 = null;
                _action3 = null;
                return;
            }

            _nonPageActions = Actions.Where(p => p is not PageAction).ToList();
            _action1 = _nonPageActions.Count > 0 ? _nonPageActions[0] : null;
            _action2 = _nonPageActions.Count > 1 ? _nonPageActions[1] : null;
            _action3 = _nonPageActions.Count > 2 ? _nonPageActions[2] : null;
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

        private void RecalculateItems()
        {
            var itemCount = ItemSource.Count;
            _totalPage = itemCount / PAGE_SIZE;
            if (itemCount > _totalPage * PAGE_SIZE)
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

            var newItems = ItemSource.Skip(PAGE_SIZE * (Page - 1)).Take(PAGE_SIZE).ToList();

            if (ShouldUpdateItems(newItems))
            {
                ApplicationService.Instance.InvokeUIAction(() =>
                {
                    UpdateItemsCollection(newItems);
                    RaiseMultiplePropertyChanged();
                });
            }
            else
            {
                RaiseMultiplePropertyChanged();
            }
        }

        private bool ShouldUpdateItems(List<T> newItems)
        {
            if (_previousItems.Count != newItems.Count) return true;

            for (int i = 0; i < newItems.Count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(_previousItems[i], newItems[i]))
                    return true;
            }
            return false;
        }

        private void UpdateItemsCollection(List<T> newItems)
        {
            // If more than 50% of items are different, it's faster to clear and reload
            int differentCount = 0;
            int maxCheck = Math.Min(Items.Count, newItems.Count);

            for (int i = 0; i < maxCheck; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(Items[i], newItems[i]))
                    differentCount++;
            }

            bool useClearAndReload = (differentCount > maxCheck / 2) || 
                                     (Math.Abs(Items.Count - newItems.Count) > newItems.Count / 2);

            if (useClearAndReload)
            {
                Items.Clear();
                foreach (var item in newItems)
                {
                    Items.Add(item);
                }
            }
            else
            {
                // Update in-place for small changes
                if (Items.Count > newItems.Count)
                {
                    for (int i = Items.Count - 1; i >= newItems.Count; i--)
                    {
                        Items.RemoveAt(i);
                    }
                }

                for (int i = 0; i < newItems.Count; i++)
                {
                    if (i < Items.Count)
                    {
                        if (!EqualityComparer<T>.Default.Equals(Items[i], newItems[i]))
                        {
                            Items[i] = newItems[i];
                        }
                    }
                    else
                    {
                        Items.Add(newItems[i]);
                    }
                }
            }
        }

        private void RaiseMultiplePropertyChanged()
        {
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
            ItemSource.Append(newItem);

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
            SetItems(new List<T>());
        }
    }
}
