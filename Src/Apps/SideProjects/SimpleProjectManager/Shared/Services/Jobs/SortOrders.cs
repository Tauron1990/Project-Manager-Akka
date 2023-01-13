using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services;

public record SortOrders(ImmutableList<SortOrder> OrdersList);