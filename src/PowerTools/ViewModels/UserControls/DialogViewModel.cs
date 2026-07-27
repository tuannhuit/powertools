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
            get => _width <= 0 ? 500 : _width;
            set => SetProperty(ref _width, value);
        }

        private double _height;
        public double Height
        {
            get => _height <= 0 ? 400 : _height;
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
                var dialogParameters = parameters.GetValue<DialogInformation>("dialogInformation");

                var dialogActions = new List<CustomAction>();
                if (dialogParameters.Actions != null && dialogParameters.Actions.Any())
                {
                    foreach (var action in dialogParameters.Actions)
                    {
                        var invokeAndCloseAction = new CustomAction
                        {
                            Name = action.Name,
                            Icon = action.Icon,
                        };

                        invokeAndCloseAction.Command = new DelegateCommand(() =>
                        {
                            var actionParams = new ActionParams
                            {
                                // By default, we assume the action is not handled. The action itself can set this to true if it handles the action.
                                // This allows the action to control whether the dialog should close or not.
                                IsHandled = false
                            };
                            action.Action?.Invoke(actionParams);
                            
                            if (!actionParams.IsHandled && action.DoCloseWhenInvoked)
                            {
                                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
                            }
                        }).ObservesCanExecute(action.CanExecuteExpression);

                        dialogActions.Add(invokeAndCloseAction);
                    }
                }

                Title = dialogParameters.Title;
                Width = dialogParameters.Width;
                Height = dialogParameters.Height;
                Actions = new ObservableCollection<CustomAction>(dialogActions);
                ViewModel = dialogParameters.ViewModel;

                ViewNavigator.Instance.NavigateToDialogView(dialogParameters.ViewType, dialogParameters);
            }
        }
    }
}
