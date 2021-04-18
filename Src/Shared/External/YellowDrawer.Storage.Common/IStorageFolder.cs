using System;
using JetBrains.Annotations;

namespace YellowDrawer.Storage.Common
{
    [PublicAPI]
    public interface IStorageFolder
    {
        string GetPath();
        string GetName();
        long GetSize();
        DateTime GetLastUpdated();
        IStorageFolder GetParent();
        void Delete();
    }
}