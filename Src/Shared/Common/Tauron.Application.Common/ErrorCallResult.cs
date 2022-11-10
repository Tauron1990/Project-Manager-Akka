using System;

namespace Tauron;

public sealed record ErrorCallResult(Exception Error) : CallResult(IsOk: false);