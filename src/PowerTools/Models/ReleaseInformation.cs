using Prism.Mvvm;

namespace PowerTools.Models
{
    public class ReleaseInformation: BindableBase
    {
        private string _changeLogs;
        private string _details;

        public string ChangeLogs
        {
            get => _changeLogs;
            set => SetProperty(ref _changeLogs, value);
        }

        public string Details
        {
            get => _details;
            set => SetProperty(ref _details, value);
        }
    }
}
