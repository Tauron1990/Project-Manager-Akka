using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Tauron.Host;
using Tauron.Localization;

namespace Tauron.Application.Avalonia.UI
{
    [PublicAPI]
    public sealed class Loc : UpdatableMarkupExtension
    {
        private static Dictionary<string, object?> _cache = new();

        public Loc(string entryName) => EntryName = entryName;

        public string EntryName { get; set; }

        protected override object ProvideValueInternal(IServiceProvider serviceProvider)
        {
            try
            {
                lock(_cache)
                    if (_cache.TryGetValue(EntryName, out var result)) return result!;
                
                ActorApplication.Application.ActorSystem.Loc().Request(EntryName, o =>
                {
                    var res = o ?? EntryName;
                    lock(_cache)
                        _cache[EntryName] = res;
                    UpdateValue(res);
                });

                return "Loading";
            }
            catch (Exception)
            {
                return "Error...";
            }
        }
    }
}