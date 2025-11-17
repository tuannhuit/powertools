using System;
using System.Windows;

namespace PowerTools.Core.SharedServices
{
    public static class ClipboardHelper
    {
        public static void CopyText(string str)
        {
            try
            {
                Clipboard.SetText(str);
            }
            catch (Exception e)
            {
                LoggingService.Instance.Info(e.Message);
            }
        }
    }
}
