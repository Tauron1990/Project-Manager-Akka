using System;
using System.Collections.Generic;
using System.Windows.Markup;
using JetBrains.Annotations;
using Tauron.AkkaHost;
using Tauron.Localization;

namespace Tauron.Application.Wpf.UI
{
    [MarkupExtensionReturnType(typeof(object))]
    [PublicAPI]
    public sealed class Loc : UpdatableMarkupExtension
    {
        private static readonly Dictionary<string, object?> Cache = new();

        public Loc(string entryName) => EntryName = entryName;

        public string? EntryName { get; set; }

        protected override object DesignTime()
        {
            if (EntryName?.Length > 25)
                return EntryName.Substring(EntryName.Length - 25, 10);

            return EntryName ?? nameof(DesignTime);
        }

        protected override object ProvideValueInternal(IServiceProvider serviceProvider)
        {
            try
            {
                lock (Cache)
                {
                    if (!string.IsNullOrWhiteSpace(EntryName) && Cache.TryGetValue(EntryName, out var result))
                        return result!;
                }

                if (string.IsNullOrWhiteSpace(EntryName)) return "Kein antrag angegeben";

                ActorApplication.ActorSystem.Loc().Request(
                    EntryName,
                    o =>
                    {
                        var res = o.GetOrElse(EntryName);
                        lock (Cache)
                        {
                            Cache[EntryName] = res;
                        }

                        UpdateValue(res);
                    });

                return "Loading";
            }
            catch (Exception e)
            {
                return $"Error... {e}";
            }
        }
    }
}