using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace PowerTools.Core.SharedServices
{
    public class LoggingService : BindableBase
    {
        private static LoggingService _instance;

        public static LoggingService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new LoggingService();

                return _instance;
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

        private LoggingService()
        {
            MessageList = new ObservableCollection<string>();

            WriteLog("Ready");
        }

        public void Info(string message)
        {
            WriteLog(message);
        }

        public void Clear()
        {
            WriteLog("Ready");
        }

        private void WriteLog(string message)
        {
            var d = Application.Current.Dispatcher;
            if (d.CheckAccess())
            {
                Message = message;
                AddMessageIntoList(message);
            }
            else
            {
                d.Invoke((Action)delegate
                {
                    Message = message;
                    AddMessageIntoList(message);
                });
            }
        }

        private void AddMessageIntoList(string message)
        {
            MessageList.Insert(0, message);

            if(MessageList.Count > 1000)
                MessageList.RemoveAt(999);
            
            RaisePropertyChanged("MessageList");
        }
    }
}
