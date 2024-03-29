﻿using Akka;
using Akka.Streams.Dsl;

namespace Tauron.Application.Akka.Redux;

public delegate Source<object, NotUsed> EffectFactory<TState>(IReduxStore<TState> store);