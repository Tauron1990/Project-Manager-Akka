using JetBrains.Annotations;

namespace SimpleProjectManager.Server.Data.DataConverters;

internal readonly record struct EntryKey([UsedImplicitly]Type From, [UsedImplicitly]Type To);