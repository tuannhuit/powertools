using System;
using System.Threading.Tasks;
using Prism.Mvvm;

namespace PowerTools.Core.SharedServices
{
    public class TaskExecution : BindableBase
    {
        private static TaskExecution _instance;

        public static TaskExecution Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TaskExecution();

                return _instance;
            }
        }

        private TaskExecution()
        {

        }

        public void RunAsync(Action action)
        {
            try
            {
                Task.Run(action);
            }
            catch (Exception e)
            {
                LoggingService.Instance.Error("Occured error during running action async", e);
            }
        }
    }
}
