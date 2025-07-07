using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace PowerTools.Core.SharedServices
{
    public class LoggingService : BindableBase
    {
        private static readonly object _lock = new object();

        private static LoggingService _instance;
        public static LoggingService Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                        _instance = new LoggingService();

                    return _instance;
                }
            }
        }

        private string _message;
        public string Message
        {
            get => _message;
            private set
            {
                _message = value;
                RaisePropertyChanged("Message");
            }
        }

        private string _status;
        public string Status
        {
            get => _status;
            private set
            {
                _status = value;
                RaisePropertyChanged("Status");
            }
        }

        private ObservableCollection<string> _messageList;
        public ObservableCollection<string> MessageList
        {
            get => _messageList;
            private set
            {
                _messageList = value;
                RaisePropertyChanged("MessageList");
            }
        }

        public Action<bool> DoShowLogCallback;

        private LoggingService()
        {
            _messageList = new ObservableCollection<string>();
            _message = "Logging started";
            _status = string.Empty;

            WriteLog("Ready");
            SetStatus("Ready");
        }

        public void Info(string message)
        {
            WriteLog(message);
        }

        public void Error(string message, Exception e)
        {
            var log = $"{message}{System.Environment.NewLine}Exception: {e.StackTrace}";
            WriteLog(log);
        }

        public void Clear()
        {
            WriteLog("Ready");
            SetStatus("Ready");
        }

        public void ShowLogs(bool doShowLogs)
        {
            DoShowLogCallback?.Invoke(doShowLogs);
        }

        private void SetStatus(string status)
        {
            ApplicationService.Instance.InvokeUIAction(() =>
            {
                Status = status;
            });
        }

        private void WriteLog(string message)
        {
            ApplicationService.Instance.InvokeUIAction(() =>
            {
                _message += System.Environment.NewLine + $"> {DateTime.Now} " + message;
                RaisePropertyChanged("Message");
            });
        }

        private void AddMessageIntoList(string message)
        {
            MessageList.Add(message);

            if (MessageList.Count > 2000)
                MessageList.RemoveAt(0);

            RaisePropertyChanged("MessageList");
        }
    }
}
