using PowerTools.Core.Models;
using PowerTools.Helpers;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PowerTools.ViewModels.UserControls
{
    public class DialogViewModel : BindableBase, IDialogAware
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                RaisePropertyChanged(nameof(Title));
            }
        }

        private double _width;
        public double Width
        {
            get => _width <= 0 ? WindowSettings.MIN_WIDTH : _width;
            set => SetProperty(ref _width, value);
        }

        private double _height;
        public double Height
        {
            get => _height <= 0 ? WindowSettings.MIN_HEIGHT : _height;
            set => SetProperty(ref _height, value);
        }

        private object _viewModel;
        public object ViewModel
        {
            get => _viewModel;
            set => SetProperty(ref _viewModel, value);
        }

        private ObservableCollection<CustomAction> _actions;
        public ObservableCollection<CustomAction> Actions
        {
            get => _actions;
            set => SetProperty(ref _actions, value);
        }

        public DelegateCommand CloseCommand { get; private set; }

        public event Action<IDialogResult>? RequestClose;

        public DialogViewModel()
        {
            CloseCommand = new DelegateCommand(() => RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel)));
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("dialogInformation"))
            {
                var dialogInformation = parameters.GetValue<DialogInformation>("dialogInformation");

                var dialogActions = new List<CustomAction>();
                if (dialogInformation.Actions != null && dialogInformation.Actions.Any())
                {
                    foreach (var action in dialogInformation.Actions)
                    {
                        var delegateCommand = new DelegateCommand(() =>
                        {
                            var actionParams = new ActionParams
                            {
                                // By default, we assume the action is not handled. The action itself can set this to true if it handles the action.
                                // This allows the action to control whether the dialog should close or not.
                                IsHandled = false,
                                Data = dialogInformation.ViewModel
                            };
                            action.Action?.Invoke(actionParams);

                            if (!actionParams.IsHandled && action.DoCloseWhenInvoked)
                            {
                                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
                            }
                        });

                        if (action.CanExecuteExpression != null)
                        {
                            delegateCommand.ObservesCanExecute(action.CanExecuteExpression);
                        }

                        dialogActions.Add(new CustomAction
                        {
                            Name = action.Name,
                            Icon = action.Icon,
                            Command = delegateCommand
                        });
                    }
                }

                Title = dialogInformation.Title;
                Width = dialogInformation.Width;
                Height = dialogInformation.Height;
                Actions = new ObservableCollection<CustomAction>(dialogActions);
                ViewModel = dialogInformation.ViewModel;

                ViewNavigator.Instance.NavigateToDialogView(dialogInformation.ViewType, dialogInformation);
            }
        }
    }
}
