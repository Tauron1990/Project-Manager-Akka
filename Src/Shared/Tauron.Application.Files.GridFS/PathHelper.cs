namespace Tauron.Application.Files.GridFS
{
    internal static class PathHelper
    {
        internal const char Seperator = '/';

        internal static string ChangeExtension(string path, string newExtension)
        {
            if (string.IsNullOrWhiteSpace(path)) return path;

            var point = -1;

            for (var i = path.Length - 1; i >= 0; i--)
            {
                if(path[i] == Seperator) return path + newExtension;

                if(path[i] != '.') continue;

                point = i;
                break;
            }

            if (point == -1) return path;

            return path.Remove(point) + newExtension;
        }

        //public static bool HasExtension(string path)
        //{
        //    if (string.IsNullOrWhiteSpace(path)) return false;

        //    var point = -1;

        //    for (var i = path.Length - 1; i >= 0; i--)
        //    {
        //        if (path[i] == Seperator) return false;

        //        if (path[i] != '.') continue;

        //        point = i;
        //        break;
        //    }

        //    if (point == -1) return false;

        //    return true;
        //}

        internal static string Combine(string first, string secund)
        {
            if (first.EndsWith(Seperator))
            {
                if (secund.StartsWith(Seperator))
                    return first + secund[1..];
                return first + secund;
            }

            if (secund.StartsWith(Seperator))
                return first + secund;

            return first + Seperator + secund;
        }
    }
}