using System.Collections.Immutable;

namespace SimpleProjectManager.Shared.Services;

public record SordOrders(ImmutableList<SordOrders> OrdersList);