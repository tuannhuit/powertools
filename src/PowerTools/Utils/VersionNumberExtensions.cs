using System;

namespace PowerTools.Utils
{
    public static class VersionNumberExtensions
    {
        public static int GetVersionValue(this string versionStr)
        {
            if (string.IsNullOrEmpty(versionStr))
                return 0;

            var parts = versionStr.Split('.');

            if (parts.Length != 4)
                throw new Exception("Not correct format of version");

            return int.Parse(parts[0]) * 1000
                   + int.Parse(parts[1]) * 100
                   + int.Parse(parts[2]) * 10
                   + int.Parse(parts[3]);
        }
    }
}
