using System;

namespace Tauron.Application;

public sealed record TransactionResult(TrasnactionState State, Exception? Exception);