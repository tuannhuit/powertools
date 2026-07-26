using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;

namespace PowerTools.Core.Models
{
    public class DialogInformation
    {
        public string Title { get; set; }
        public Type ViewType { get; set; }
        public object ViewModel { get; set; }
        public Action<IDialogResult> Callback { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public IList<DialogAction> Actions { get; set; }
    }
}
