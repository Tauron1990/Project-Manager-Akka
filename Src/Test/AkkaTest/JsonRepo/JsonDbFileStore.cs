using System;

namespace AkkaTest.JsonRepo
{
    public static class JsonDbFileStore
    {
        private sealed record FileKey(string FileName, Type)
    }
}