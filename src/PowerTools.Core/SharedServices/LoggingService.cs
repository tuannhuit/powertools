using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PowerTools.Core.SharedServices
{
    public class LoggingService : BindableBase
    {
        public static readonly int MAX_MESSAGES_COUNT = 2000;
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
                RaisePropertyChanged();
            }
        }

        private bool _doShowLog;
        public bool DoShowLog
        {
            get => _doShowLog;
            set
            {
                _doShowLog = value;
                if (DoShowLog)
                {
                    Status = string.Empty;
                }
                else
                {
                    Status = MessageList.Last();
                }
                RaisePropertyChanged();
            }
        }

        private bool _doTextWrapping;   
        public bool DoTextWrapping
        {
            get => _doTextWrapping;
            set
            {
                _doTextWrapping = value;
                RaisePropertyChanged();
            }
        }

        private bool _doShowTime;   
        public bool DoShowTime
        {
            get => _doShowTime;
            set
            {
                _doShowTime = value;
                RaisePropertyChanged();
            }
        }

        private string _status;
        public string Status
        {
            get => _status;
            private set
            {
                _status = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<string> _messageList;
        public ObservableCollection<string> MessageList
        {
            get => _messageList;
            private set
            {
                _messageList = value;
                RaisePropertyChanged();
            }
        }

        public Action<bool> DoShowLogCallback;

        private LoggingService()
        {
            _messageList = new ObservableCollection<string>();
            _message = "Logging started";
            _status = string.Empty;

            DoTextWrapping = false;
            DoShowTime = false;

            WriteLog("Ready");
        }

        public void Info(string message)
        {
            WriteLog(message);
        }

        public void Error(string message, Exception e)
        {
            if (e == null)
            {
                return;
            }

            var log = $"{message}{System.Environment.NewLine}Exception: {e.Message}\n{e.StackTrace}";
            WriteLog(log);
        }

        public void Clear()
        {
            _messageList.Clear();
            WriteLog("Ready");
        }

        public void ShowLogs(bool doShowLogs)
        {
            DoShowLogCallback?.Invoke(doShowLogs);
        }

        private void WriteLog(string message)
        {
            if (message == null)
            {
                return;
            }

            var dateTimeMessage = DoShowTime ? $"{DateTime.Now} " : string.Empty;
            var indexOfNewLine = message.IndexOf(Environment.NewLine, StringComparison.Ordinal);
            var newMessage = $"> {dateTimeMessage}" + (indexOfNewLine == -1 ? message : message.Substring(0, indexOfNewLine + 1));

            ApplicationService.Instance.InvokeUIAction(() =>
            {
                AddMessageIntoList($"> {dateTimeMessage}" + message);
                Message = string.Join(Environment.NewLine, MessageList);

                if (!DoShowLog)
                {
                    Status = newMessage;
                }
            });
        }

        private void AddMessageIntoList(string message)
        {
            _messageList.Add(message);

            if (_messageList.Count > MAX_MESSAGES_COUNT)
                _messageList.RemoveAt(0);

            RaisePropertyChanged("MessageList");
        }
    }
}
