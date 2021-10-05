using System.IO;
using JetBrains.Annotations;

namespace Tauron.Application.VirtualFiles.Core
{
    [PublicAPI]
    public static class GenericPathHelper
    {
        public const char GenericSeperator = '/';

        public static string ChangeExtension(string path, string newExtension)
            => Path.ChangeExtension(path, newExtension);
            /*if (string.IsNullOrWhiteSpace(path)) return path;

            var point = -1;

            for (var i = path.Length - 1; i >= 0; i--)
            {
                if (path[i] == GenericSeperator) return path + newExtension;

                if (path[i] != '.') continue;

                point = i;

                break;
            }

            return point == -1 
                ? path 
                : $"{path[..point]}{newExtension}";*/

            public static string Combine(string first, string secund)
            {
                var result = Path.Combine(first, secund);

                return Path.DirectorySeparatorChar != Path.AltDirectorySeparatorChar 
                    ? result.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) 
                    : result;
            }

            public static string NormalizePath(string path)
                => path.Replace('\\', GenericSeperator);

            /*{
                if (string.IsNullOrEmpty(first))
                    return string.IsNullOrWhiteSpace(secund) ? string.Empty : secund;
    
                if (string.IsNullOrEmpty(secund))
                    return string.IsNullOrWhiteSpace(first) ? string.Empty : first;
                
                if (first.EndsWith(GenericSeperator))
                {
                    if (secund.StartsWith(GenericSeperator))
                        return first + secund[1..];
    
                    return first + secund;
                }
    
                if (secund.StartsWith(GenericSeperator))
                    return first + secund;
    
                return first + GenericSeperator + secund;
            }*/
    }
}