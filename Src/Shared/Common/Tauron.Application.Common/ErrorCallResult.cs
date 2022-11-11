using System;

namespace Tauron;

public sealed record ErrorCallResult<TResult>(Exception Error) : CallResult<TResult>(IsOk: false);