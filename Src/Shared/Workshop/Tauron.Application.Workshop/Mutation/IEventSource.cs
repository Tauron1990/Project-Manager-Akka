using System;

namespace Tauron.Application.Workshop.Mutation;

public interface IEventSource<out TRespond> : IObservable<TRespond> { }