using System;
using Stl.Fusion;

namespace Tauron;

public interface IDisposableState<TData> : IState<TData>, IDisposable { }