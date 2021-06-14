using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tauron.Application.Dataflow
{
    class Class1
    {
        
        //		public static IObservable<Unit> Start(Action action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.Start(action);
        //		}

        //		public static IObservable<Unit> Start(Action action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Start(action, scheduler);
        //		}

        //		public static IObservable<Unit> StartAsync(Func<Task> actionAsync)
        //		{
        //			if (actionAsync == null)
        //			{
        //				throw new ArgumentNullException("actionAsync");
        //			}
        //			return s_impl.StartAsync(actionAsync);
        //		}

        //		public static IObservable<Unit> StartAsync(Func<Task> actionAsync, IScheduler scheduler)
        //		{
        //			if (actionAsync == null)
        //			{
        //				throw new ArgumentNullException("actionAsync");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.StartAsync(actionAsync, scheduler);
        //		}

        //		public static IObservable<Unit> StartAsync(Func<CancellationToken, Task> actionAsync)
        //		{
        //			if (actionAsync == null)
        //			{
        //				throw new ArgumentNullException("actionAsync");
        //			}
        //			return s_impl.StartAsync(actionAsync);
        //		}

        //		public static IObservable<Unit> StartAsync(Func<CancellationToken, Task> actionAsync, IScheduler scheduler)
        //		{
        //			if (actionAsync == null)
        //			{
        //				throw new ArgumentNullException("actionAsync");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.StartAsync(actionAsync, scheduler);
        //		}

        //		public static IObservable<TResult> FromAsync<TResult>(Func<Task<TResult>> functionAsync)
        //		{
        //			if (functionAsync == null)
        //			{
        //				throw new ArgumentNullException("functionAsync");
        //			}
        //			return s_impl.FromAsync(functionAsync);
        //		}

        //		public static IObservable<TResult> FromAsync<TResult>(Func<Task<TResult>> functionAsync, IScheduler scheduler)
        //		{
        //			if (functionAsync == null)
        //			{
        //				throw new ArgumentNullException("functionAsync");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromAsync(functionAsync, scheduler);
        //		}

        //		public static IObservable<TResult> FromAsync<TResult>(Func<CancellationToken, Task<TResult>> functionAsync)
        //		{
        //			if (functionAsync == null)
        //			{
        //				throw new ArgumentNullException("functionAsync");
        //			}
        //			return s_impl.FromAsync(functionAsync);
        //		}

        //		public static IObservable<TResult> FromAsync<TResult>(Func<CancellationToken, Task<TResult>> functionAsync, IScheduler scheduler)
        //		{
        //			if (functionAsync == null)
        //			{
        //				throw new ArgumentNullException("functionAsync");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromAsync(functionAsync, scheduler);
        //		}

        //		public static IObservable<Unit> FromAsync(Func<Task> actionAsync)
        //		{
        //			if (actionAsync == null)
        //			{
        //				throw new ArgumentNullException("actionAsync");
        //			}
        //			return s_impl.FromAsync(actionAsync);
        //		}

        //		public static IObservable<Unit> FromAsync(Func<Task> actionAsync, IScheduler scheduler)
        //		{
        //			if (actionAsync == null)
        //			{
        //				throw new ArgumentNullException("actionAsync");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromAsync(actionAsync, scheduler);
        //		}

        //		public static IObservable<Unit> FromAsync(Func<CancellationToken, Task> actionAsync)
        //		{
        //			if (actionAsync == null)
        //			{
        //				throw new ArgumentNullException("actionAsync");
        //			}
        //			return s_impl.FromAsync(actionAsync);
        //		}

        //		public static IObservable<Unit> FromAsync(Func<CancellationToken, Task> actionAsync, IScheduler scheduler)
        //		{
        //			if (actionAsync == null)
        //			{
        //				throw new ArgumentNullException("actionAsync");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromAsync(actionAsync, scheduler);
        //		}

        //		public static Func<IObservable<TResult>> ToAsync<TResult>(this Func<TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<IObservable<TResult>> ToAsync<TResult>(this Func<TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, IObservable<TResult>> ToAsync<TArg1, TResult>(this Func<TArg1, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, IObservable<TResult>> ToAsync<TArg1, TResult>(this Func<TArg1, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, IObservable<TResult>> ToAsync<TArg1, TArg2, TResult>(this Func<TArg1, TArg2, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, IObservable<TResult>> ToAsync<TArg1, TArg2, TResult>(this Func<TArg1, TArg2, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TResult>(this Func<TArg1, TArg2, TArg3, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TResult>(this Func<TArg1, TArg2, TArg3, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult> function)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			return s_impl.ToAsync(function);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, IObservable<TResult>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult>(this Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, TResult> function, IScheduler scheduler)
        //		{
        //			if (function == null)
        //			{
        //				throw new ArgumentNullException("function");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(function, scheduler);
        //		}

        //		public static Func<IObservable<Unit>> ToAsync(this Action action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<IObservable<Unit>> ToAsync(this Action action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, IObservable<Unit>> ToAsync<TArg1>(this Action<TArg1> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, IObservable<Unit>> ToAsync<TArg1>(this Action<TArg1> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, IObservable<Unit>> ToAsync<TArg1, TArg2>(this Action<TArg1, TArg2> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, IObservable<Unit>> ToAsync<TArg1, TArg2>(this Action<TArg1, TArg2> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3>(this Action<TArg1, TArg2, TArg3> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3>(this Action<TArg1, TArg2, TArg3> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4>(this Action<TArg1, TArg2, TArg3, TArg4> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4>(this Action<TArg1, TArg2, TArg3, TArg4> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16> action)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			return s_impl.ToAsync(action);
        //		}

        //		public static Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16, IObservable<Unit>> ToAsync<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16>(this Action<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TArg9, TArg10, TArg11, TArg12, TArg13, TArg14, TArg15, TArg16> action, IScheduler scheduler)
        //		{
        //			if (action == null)
        //			{
        //				throw new ArgumentNullException("action");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToAsync(action, scheduler);
        //		}

        //		public static AsyncSubject<TSource> GetAwaiter<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.GetAwaiter(source);
        //		}

        //		public static AsyncSubject<TSource> GetAwaiter<TSource>(this IConnectableObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.GetAwaiter(source);
        //		}

        //		public static AsyncSubject<TSource> RunAsync<TSource>(this IObservable<TSource> source, CancellationToken cancellationToken)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.RunAsync(source, cancellationToken);
        //		}

        //		public static AsyncSubject<TSource> RunAsync<TSource>(this IConnectableObservable<TSource> source, CancellationToken cancellationToken)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.RunAsync(source, cancellationToken);
        //		}

        //		public static IConnectableObservable<TResult> Multicast<TSource, TResult>(this IObservable<TSource> source, ISubject<TSource, TResult> subject)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (subject == null)
        //			{
        //				throw new ArgumentNullException("subject");
        //			}
        //			return s_impl.Multicast(source, subject);
        //		}

        //		public static IObservable<TResult> Multicast<TSource, TIntermediate, TResult>(this IObservable<TSource> source, Func<ISubject<TSource, TIntermediate>> subjectSelector, Func<IObservable<TIntermediate>, IObservable<TResult>> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (subjectSelector == null)
        //			{
        //				throw new ArgumentNullException("subjectSelector");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.Multicast(source, subjectSelector, selector);
        //		}

        //		public static IConnectableObservable<TSource> Publish<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Publish(source);
        //		}

        //		public static IObservable<TResult> Publish<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.Publish(source, selector);
        //		}

        //		public static IConnectableObservable<TSource> Publish<TSource>(this IObservable<TSource> source, TSource initialValue)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Publish(source, initialValue);
        //		}

        //		public static IObservable<TResult> Publish<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, TSource initialValue)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.Publish(source, selector, initialValue);
        //		}

        //		public static IConnectableObservable<TSource> PublishLast<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.PublishLast(source);
        //		}

        //		public static IObservable<TResult> PublishLast<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.PublishLast(source, selector);
        //		}

        //		public static IObservable<TSource> RefCount<TSource>(this IConnectableObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.RefCount(source);
        //		}

        //		public static IObservable<TSource> RefCount<TSource>(this IConnectableObservable<TSource> source, TimeSpan disconnectDelay)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (disconnectDelay < TimeSpan.Zero)
        //			{
        //				throw new ArgumentException("Delay cannot be less than zero", "disconnectDelay");
        //			}
        //			return s_impl.RefCount(source, disconnectDelay);
        //		}

        //		public static IObservable<TSource> RefCount<TSource>(this IConnectableObservable<TSource> source, TimeSpan disconnectDelay, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			if (disconnectDelay < TimeSpan.Zero)
        //			{
        //				throw new ArgumentException("Delay cannot be less than zero", "disconnectDelay");
        //			}
        //			return s_impl.RefCount(source, disconnectDelay, scheduler);
        //		}

        //		public static IObservable<TSource> RefCount<TSource>(this IConnectableObservable<TSource> source, int minObservers)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (minObservers <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("minObservers");
        //			}
        //			return s_impl.RefCount(source, minObservers);
        //		}

        //		public static IObservable<TSource> RefCount<TSource>(this IConnectableObservable<TSource> source, int minObservers, TimeSpan disconnectDelay)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (disconnectDelay < TimeSpan.Zero)
        //			{
        //				throw new ArgumentException("Delay cannot be less than zero", "disconnectDelay");
        //			}
        //			if (minObservers <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("minObservers");
        //			}
        //			return s_impl.RefCount(source, minObservers, disconnectDelay);
        //		}

        //		public static IObservable<TSource> RefCount<TSource>(this IConnectableObservable<TSource> source, int minObservers, TimeSpan disconnectDelay, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			if (disconnectDelay < TimeSpan.Zero)
        //			{
        //				throw new ArgumentException("Delay cannot be less than zero", "disconnectDelay");
        //			}
        //			if (minObservers <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("minObservers");
        //			}
        //			return s_impl.RefCount(source, minObservers, disconnectDelay, scheduler);
        //		}

        //		public static IObservable<TSource> AutoConnect<TSource>(this IConnectableObservable<TSource> source, int minObservers = 1, Action<IDisposable>? onConnect = null)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.AutoConnect(source, minObservers, onConnect);
        //		}

        //		public static IConnectableObservable<TSource> Replay<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Replay(source);
        //		}

        //		public static IConnectableObservable<TSource> Replay<TSource>(this IObservable<TSource> source, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Replay(source, scheduler);
        //		}

        //		public static IObservable<TResult> Replay<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.Replay(source, selector);
        //		}

        //		public static IObservable<TResult> Replay<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Replay(source, selector, scheduler);
        //		}

        //		public static IConnectableObservable<TSource> Replay<TSource>(this IObservable<TSource> source, TimeSpan window)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (window < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("window");
        //			}
        //			return s_impl.Replay(source, window);
        //		}

        //		public static IObservable<TResult> Replay<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, TimeSpan window)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			if (window < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("window");
        //			}
        //			return s_impl.Replay(source, selector, window);
        //		}

        //		public static IConnectableObservable<TSource> Replay<TSource>(this IObservable<TSource> source, TimeSpan window, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (window < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("window");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Replay(source, window, scheduler);
        //		}

        //		public static IObservable<TResult> Replay<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, TimeSpan window, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			if (window < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("window");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Replay(source, selector, window, scheduler);
        //		}

        //		public static IConnectableObservable<TSource> Replay<TSource>(this IObservable<TSource> source, int bufferSize, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (bufferSize < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("bufferSize");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Replay(source, bufferSize, scheduler);
        //		}

        //		public static IObservable<TResult> Replay<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, int bufferSize, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			if (bufferSize < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("bufferSize");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Replay(source, selector, bufferSize, scheduler);
        //		}

        //		public static IConnectableObservable<TSource> Replay<TSource>(this IObservable<TSource> source, int bufferSize)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (bufferSize < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("bufferSize");
        //			}
        //			return s_impl.Replay(source, bufferSize);
        //		}

        //		public static IObservable<TResult> Replay<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, int bufferSize)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			if (bufferSize < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("bufferSize");
        //			}
        //			return s_impl.Replay(source, selector, bufferSize);
        //		}

        //		public static IConnectableObservable<TSource> Replay<TSource>(this IObservable<TSource> source, int bufferSize, TimeSpan window)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (bufferSize < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("bufferSize");
        //			}
        //			if (window < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("window");
        //			}
        //			return s_impl.Replay(source, bufferSize, window);
        //		}

        //		public static IObservable<TResult> Replay<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, int bufferSize, TimeSpan window)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			if (bufferSize < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("bufferSize");
        //			}
        //			if (window < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("window");
        //			}
        //			return s_impl.Replay(source, selector, bufferSize, window);
        //		}

        //		public static IConnectableObservable<TSource> Replay<TSource>(this IObservable<TSource> source, int bufferSize, TimeSpan window, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (bufferSize < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("bufferSize");
        //			}
        //			if (window < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("window");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Replay(source, bufferSize, window, scheduler);
        //		}

        //		public static IObservable<TResult> Replay<TSource, TResult>(this IObservable<TSource> source, Func<IObservable<TSource>, IObservable<TResult>> selector, int bufferSize, TimeSpan window, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			if (bufferSize < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("bufferSize");
        //			}
        //			if (window < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("window");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Replay(source, selector, bufferSize, window, scheduler);
        //		}

        //		public static IEnumerable<IList<TSource>> Chunkify<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Chunkify(source);
        //		}

        //		public static IEnumerable<TResult> Collect<TSource, TResult>(this IObservable<TSource> source, Func<TResult> newCollector, Func<TResult, TSource, TResult> merge)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (newCollector == null)
        //			{
        //				throw new ArgumentNullException("newCollector");
        //			}
        //			if (merge == null)
        //			{
        //				throw new ArgumentNullException("merge");
        //			}
        //			return s_impl.Collect(source, newCollector, merge);
        //		}

        //		public static IEnumerable<TResult> Collect<TSource, TResult>(this IObservable<TSource> source, Func<TResult> getInitialCollector, Func<TResult, TSource, TResult> merge, Func<TResult, TResult> getNewCollector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (getInitialCollector == null)
        //			{
        //				throw new ArgumentNullException("getInitialCollector");
        //			}
        //			if (merge == null)
        //			{
        //				throw new ArgumentNullException("merge");
        //			}
        //			if (getNewCollector == null)
        //			{
        //				throw new ArgumentNullException("getNewCollector");
        //			}
        //			return s_impl.Collect(source, getInitialCollector, merge, getNewCollector);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		public static TSource First<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.First(source);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		public static TSource First<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (predicate == null)
        //			{
        //				throw new ArgumentNullException("predicate");
        //			}
        //			return s_impl.First(source, predicate);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		[return: MaybeNull]
        //		public static TSource FirstOrDefault<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.FirstOrDefault(source);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		[return: MaybeNull]
        //		public static TSource FirstOrDefault<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (predicate == null)
        //			{
        //				throw new ArgumentNullException("predicate");
        //			}
        //			return s_impl.FirstOrDefault(source, predicate);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		public static void ForEach<TSource>(this IObservable<TSource> source, Action<TSource> onNext)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (onNext == null)
        //			{
        //				throw new ArgumentNullException("onNext");
        //			}
        //			s_impl.ForEach(source, onNext);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		public static void ForEach<TSource>(this IObservable<TSource> source, Action<TSource, int> onNext)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (onNext == null)
        //			{
        //				throw new ArgumentNullException("onNext");
        //			}
        //			s_impl.ForEach(source, onNext);
        //		}

        //		public static IEnumerator<TSource> GetEnumerator<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.GetEnumerator(source);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		public static TSource Last<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Last(source);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		public static TSource Last<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (predicate == null)
        //			{
        //				throw new ArgumentNullException("predicate");
        //			}
        //			return s_impl.Last(source, predicate);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		[return: MaybeNull]
        //		public static TSource LastOrDefault<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.LastOrDefault(source);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		[return: MaybeNull]
        //		public static TSource LastOrDefault<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (predicate == null)
        //			{
        //				throw new ArgumentNullException("predicate");
        //			}
        //			return s_impl.LastOrDefault(source, predicate);
        //		}

        //		public static IEnumerable<TSource> Latest<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Latest(source);
        //		}

        //		public static IEnumerable<TSource> MostRecent<TSource>(this IObservable<TSource> source, TSource initialValue)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.MostRecent(source, initialValue);
        //		}

        //		public static IEnumerable<TSource> Next<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Next(source);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		public static TSource Single<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Single(source);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		public static TSource Single<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (predicate == null)
        //			{
        //				throw new ArgumentNullException("predicate");
        //			}
        //			return s_impl.Single(source, predicate);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		[return: MaybeNull]
        //		public static TSource SingleOrDefault<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.SingleOrDefault(source);
        //		}

        //		[Obsolete("This blocking operation is no longer supported. Instead, use the async version in combination with C# and Visual Basic async/await support. In case you need a blocking operation, use Wait or convert the resulting observable sequence to a Task object and block.")]
        //		[return: MaybeNull]
        //		public static TSource SingleOrDefault<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (predicate == null)
        //			{
        //				throw new ArgumentNullException("predicate");
        //			}
        //			return s_impl.SingleOrDefault(source, predicate);
        //		}

        //		public static TSource Wait<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Wait(source);
        //		}

        //		public static IObservable<TSource> ObserveOn<TSource>(this IObservable<TSource> source, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ObserveOn(source, scheduler);
        //		}

        //		public static IObservable<TSource> ObserveOn<TSource>(this IObservable<TSource> source, SynchronizationContext context)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (context == null)
        //			{
        //				throw new ArgumentNullException("context");
        //			}
        //			return s_impl.ObserveOn(source, context);
        //		}

        //		public static IObservable<TSource> SubscribeOn<TSource>(this IObservable<TSource> source, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.SubscribeOn(source, scheduler);
        //		}

        //		public static IObservable<TSource> SubscribeOn<TSource>(this IObservable<TSource> source, SynchronizationContext context)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (context == null)
        //			{
        //				throw new ArgumentNullException("context");
        //			}
        //			return s_impl.SubscribeOn(source, context);
        //		}

        //		public static IObservable<TSource> Synchronize<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Synchronize(source);
        //		}

        //		public static IObservable<TSource> Synchronize<TSource>(this IObservable<TSource> source, object gate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (gate == null)
        //			{
        //				throw new ArgumentNullException("gate");
        //			}
        //			return s_impl.Synchronize(source, gate);
        //		}

        //		public static IDisposable Subscribe<TSource>(this IEnumerable<TSource> source, IObserver<TSource> observer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (observer == null)
        //			{
        //				throw new ArgumentNullException("observer");
        //			}
        //			return s_impl.Subscribe(source, observer);
        //		}

        //		public static IDisposable Subscribe<TSource>(this IEnumerable<TSource> source, IObserver<TSource> observer, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (observer == null)
        //			{
        //				throw new ArgumentNullException("observer");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Subscribe(source, observer, scheduler);
        //		}

        //		public static IEnumerable<TSource> ToEnumerable<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.ToEnumerable(source);
        //		}

        //		public static IEventSource<Unit> ToEvent(this IObservable<Unit> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.ToEvent(source);
        //		}

        //		public static IEventSource<TSource> ToEvent<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.ToEvent(source);
        //		}

        //		public static IEventPatternSource<TEventArgs> ToEventPattern<TEventArgs>(this IObservable<EventPattern<TEventArgs>> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.ToEventPattern(source);
        //		}

        //		public static IObservable<TSource> ToObservable<TSource>(this IEnumerable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.ToObservable(source);
        //		}

        //		public static IObservable<TSource> ToObservable<TSource>(this IEnumerable<TSource> source, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.ToObservable(source, scheduler);
        //		}

        //		public static IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, IDisposable> subscribe)
        //		{
        //			if (subscribe == null)
        //			{
        //				throw new ArgumentNullException("subscribe");
        //			}
        //			return s_impl.Create(subscribe);
        //		}

        //		public static IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, Action> subscribe)
        //		{
        //			if (subscribe == null)
        //			{
        //				throw new ArgumentNullException("subscribe");
        //			}
        //			return s_impl.Create(subscribe);
        //		}

        //		public static IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, CancellationToken, Task> subscribeAsync)
        //		{
        //			if (subscribeAsync == null)
        //			{
        //				throw new ArgumentNullException("subscribeAsync");
        //			}
        //			return s_impl.Create(subscribeAsync);
        //		}

        //		public static IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, Task> subscribeAsync)
        //		{
        //			if (subscribeAsync == null)
        //			{
        //				throw new ArgumentNullException("subscribeAsync");
        //			}
        //			return s_impl.Create(subscribeAsync);
        //		}

        //		public static IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, CancellationToken, Task<IDisposable>> subscribeAsync)
        //		{
        //			if (subscribeAsync == null)
        //			{
        //				throw new ArgumentNullException("subscribeAsync");
        //			}
        //			return s_impl.Create(subscribeAsync);
        //		}

        //		public static IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, Task<IDisposable>> subscribeAsync)
        //		{
        //			if (subscribeAsync == null)
        //			{
        //				throw new ArgumentNullException("subscribeAsync");
        //			}
        //			return s_impl.Create(subscribeAsync);
        //		}

        //		public static IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, CancellationToken, Task<Action>> subscribeAsync)
        //		{
        //			if (subscribeAsync == null)
        //			{
        //				throw new ArgumentNullException("subscribeAsync");
        //			}
        //			return s_impl.Create(subscribeAsync);
        //		}

        //		public static IObservable<TResult> Create<TResult>(Func<IObserver<TResult>, Task<Action>> subscribeAsync)
        //		{
        //			if (subscribeAsync == null)
        //			{
        //				throw new ArgumentNullException("subscribeAsync");
        //			}
        //			return s_impl.Create(subscribeAsync);
        //		}

        //		public static IObservable<TResult> Defer<TResult>(Func<IObservable<TResult>> observableFactory)
        //		{
        //			if (observableFactory == null)
        //			{
        //				throw new ArgumentNullException("observableFactory");
        //			}
        //			return s_impl.Defer(observableFactory);
        //		}

        //		public static IObservable<TResult> Defer<TResult>(Func<Task<IObservable<TResult>>> observableFactoryAsync)
        //		{
        //			if (observableFactoryAsync == null)
        //			{
        //				throw new ArgumentNullException("observableFactoryAsync");
        //			}
        //			return s_impl.Defer(observableFactoryAsync);
        //		}

        //		public static IObservable<TResult> DeferAsync<TResult>(Func<CancellationToken, Task<IObservable<TResult>>> observableFactoryAsync)
        //		{
        //			if (observableFactoryAsync == null)
        //			{
        //				throw new ArgumentNullException("observableFactoryAsync");
        //			}
        //			return s_impl.Defer(observableFactoryAsync);
        //		}

        //		public static IObservable<TResult> Empty<TResult>()
        //		{
        //			return s_impl.Empty<TResult>();
        //		}

        //		public static IObservable<TResult> Empty<TResult>(TResult witness)
        //		{
        //			return s_impl.Empty<TResult>();
        //		}

        //		public static IObservable<TResult> Empty<TResult>(IScheduler scheduler)
        //		{
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Empty<TResult>(scheduler);
        //		}

        //		public static IObservable<TResult> Empty<TResult>(IScheduler scheduler, TResult witness)
        //		{
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Empty<TResult>(scheduler);
        //		}

        //		public static IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector)
        //		{
        //			if (condition == null)
        //			{
        //				throw new ArgumentNullException("condition");
        //			}
        //			if (iterate == null)
        //			{
        //				throw new ArgumentNullException("iterate");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Generate(initialState, condition, iterate, resultSelector);
        //		}

        //		public static IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, IScheduler scheduler)
        //		{
        //			if (condition == null)
        //			{
        //				throw new ArgumentNullException("condition");
        //			}
        //			if (iterate == null)
        //			{
        //				throw new ArgumentNullException("iterate");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Generate(initialState, condition, iterate, resultSelector, scheduler);
        //		}

        //		public static IObservable<TResult> Never<TResult>()
        //		{
        //			return s_impl.Never<TResult>();
        //		}

        //		public static IObservable<TResult> Never<TResult>(TResult witness)
        //		{
        //			return s_impl.Never<TResult>();
        //		}

        //		public static IObservable<int> Range(int start, int count)
        //		{
        //			long max = (long)start + (long)count - 1;
        //			if (count < 0 || max > int.MaxValue)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			return s_impl.Range(start, count);
        //		}

        //		public static IObservable<int> Range(int start, int count, IScheduler scheduler)
        //		{
        //			long max = (long)start + (long)count - 1;
        //			if (count < 0 || max > int.MaxValue)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Range(start, count, scheduler);
        //		}

        //		public static IObservable<TResult> Repeat<TResult>(TResult value)
        //		{
        //			return s_impl.Repeat(value);
        //		}

        //		public static IObservable<TResult> Repeat<TResult>(TResult value, IScheduler scheduler)
        //		{
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Repeat(value, scheduler);
        //		}

        //		public static IObservable<TResult> Repeat<TResult>(TResult value, int repeatCount)
        //		{
        //			if (repeatCount < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("repeatCount");
        //			}
        //			return s_impl.Repeat(value, repeatCount);
        //		}

        //		public static IObservable<TResult> Repeat<TResult>(TResult value, int repeatCount, IScheduler scheduler)
        //		{
        //			if (repeatCount < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("repeatCount");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Repeat(value, repeatCount, scheduler);
        //		}

        //		public static IObservable<TResult> Return<TResult>(TResult value)
        //		{
        //			return s_impl.Return(value);
        //		}

        //		public static IObservable<TResult> Return<TResult>(TResult value, IScheduler scheduler)
        //		{
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Return(value, scheduler);
        //		}

        //		public static IObservable<TResult> Throw<TResult>(Exception exception)
        //		{
        //			if (exception == null)
        //			{
        //				throw new ArgumentNullException("exception");
        //			}
        //			return s_impl.Throw<TResult>(exception);
        //		}

        //		public static IObservable<TResult> Throw<TResult>(Exception exception, TResult witness)
        //		{
        //			if (exception == null)
        //			{
        //				throw new ArgumentNullException("exception");
        //			}
        //			return s_impl.Throw<TResult>(exception);
        //		}

        //		public static IObservable<TResult> Throw<TResult>(Exception exception, IScheduler scheduler)
        //		{
        //			if (exception == null)
        //			{
        //				throw new ArgumentNullException("exception");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Throw<TResult>(exception, scheduler);
        //		}

        //		public static IObservable<TResult> Throw<TResult>(Exception exception, IScheduler scheduler, TResult witness)
        //		{
        //			if (exception == null)
        //			{
        //				throw new ArgumentNullException("exception");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Throw<TResult>(exception, scheduler);
        //		}

        //		public static IObservable<TResult> Using<TResult, TResource>(Func<TResource> resourceFactory, Func<TResource, IObservable<TResult>> observableFactory) where TResource : IDisposable
        //		{
        //			if (resourceFactory == null)
        //			{
        //				throw new ArgumentNullException("resourceFactory");
        //			}
        //			if (observableFactory == null)
        //			{
        //				throw new ArgumentNullException("observableFactory");
        //			}
        //			return s_impl.Using(resourceFactory, observableFactory);
        //		}

        //		public static IObservable<TResult> Using<TResult, TResource>(Func<CancellationToken, Task<TResource>> resourceFactoryAsync, Func<TResource, CancellationToken, Task<IObservable<TResult>>> observableFactoryAsync) where TResource : IDisposable
        //		{
        //			if (resourceFactoryAsync == null)
        //			{
        //				throw new ArgumentNullException("resourceFactoryAsync");
        //			}
        //			if (observableFactoryAsync == null)
        //			{
        //				throw new ArgumentNullException("observableFactoryAsync");
        //			}
        //			return s_impl.Using(resourceFactoryAsync, observableFactoryAsync);
        //		}

        //		public static IObservable<EventPattern<object>> FromEventPattern(Action<EventHandler> addHandler, Action<EventHandler> removeHandler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			return s_impl.FromEventPattern(addHandler, removeHandler);
        //		}

        //		public static IObservable<EventPattern<object>> FromEventPattern(Action<EventHandler> addHandler, Action<EventHandler> removeHandler, IScheduler scheduler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEventPattern(addHandler, removeHandler, scheduler);
        //		}

        //		public static IObservable<EventPattern<TEventArgs>> FromEventPattern<TDelegate, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			return s_impl.FromEventPattern<TDelegate, TEventArgs>(addHandler, removeHandler);
        //		}

        //		public static IObservable<EventPattern<TEventArgs>> FromEventPattern<TDelegate, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEventPattern<TDelegate, TEventArgs>(addHandler, removeHandler, scheduler);
        //		}

        //		public static IObservable<EventPattern<TEventArgs>> FromEventPattern<TDelegate, TEventArgs>(Func<EventHandler<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
        //		{
        //			if (conversion == null)
        //			{
        //				throw new ArgumentNullException("conversion");
        //			}
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			return s_impl.FromEventPattern(conversion, addHandler, removeHandler);
        //		}

        //		public static IObservable<EventPattern<TEventArgs>> FromEventPattern<TDelegate, TEventArgs>(Func<EventHandler<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
        //		{
        //			if (conversion == null)
        //			{
        //				throw new ArgumentNullException("conversion");
        //			}
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEventPattern(conversion, addHandler, removeHandler, scheduler);
        //		}

        //		public static IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TDelegate, TSender, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			return s_impl.FromEventPattern<TDelegate, TSender, TEventArgs>(addHandler, removeHandler);
        //		}

        //		public static IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TDelegate, TSender, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEventPattern<TDelegate, TSender, TEventArgs>(addHandler, removeHandler, scheduler);
        //		}

        //		public static IObservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(Action<EventHandler<TEventArgs>> addHandler, Action<EventHandler<TEventArgs>> removeHandler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			return s_impl.FromEventPattern(addHandler, removeHandler);
        //		}

        //		public static IObservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(Action<EventHandler<TEventArgs>> addHandler, Action<EventHandler<TEventArgs>> removeHandler, IScheduler scheduler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEventPattern(addHandler, removeHandler, scheduler);
        //		}

        //		public static IObservable<EventPattern<object>> FromEventPattern(object target, string eventName)
        //		{
        //			if (target == null)
        //			{
        //				throw new ArgumentNullException("target");
        //			}
        //			if (eventName == null)
        //			{
        //				throw new ArgumentNullException("eventName");
        //			}
        //			return s_impl.FromEventPattern(target, eventName);
        //		}

        //		public static IObservable<EventPattern<object>> FromEventPattern(object target, string eventName, IScheduler scheduler)
        //		{
        //			if (target == null)
        //			{
        //				throw new ArgumentNullException("target");
        //			}
        //			if (eventName == null)
        //			{
        //				throw new ArgumentNullException("eventName");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEventPattern(target, eventName, scheduler);
        //		}

        //		public static IObservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(object target, string eventName)
        //		{
        //			if (target == null)
        //			{
        //				throw new ArgumentNullException("target");
        //			}
        //			if (eventName == null)
        //			{
        //				throw new ArgumentNullException("eventName");
        //			}
        //			return s_impl.FromEventPattern<TEventArgs>(target, eventName);
        //		}

        //		public static IObservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(object target, string eventName, IScheduler scheduler)
        //		{
        //			if (target == null)
        //			{
        //				throw new ArgumentNullException("target");
        //			}
        //			if (eventName == null)
        //			{
        //				throw new ArgumentNullException("eventName");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEventPattern<TEventArgs>(target, eventName, scheduler);
        //		}

        //		public static IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TSender, TEventArgs>(object target, string eventName)
        //		{
        //			if (target == null)
        //			{
        //				throw new ArgumentNullException("target");
        //			}
        //			if (eventName == null)
        //			{
        //				throw new ArgumentNullException("eventName");
        //			}
        //			return s_impl.FromEventPattern<TSender, TEventArgs>(target, eventName);
        //		}

        //		public static IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TSender, TEventArgs>(object target, string eventName, IScheduler scheduler)
        //		{
        //			if (target == null)
        //			{
        //				throw new ArgumentNullException("target");
        //			}
        //			if (eventName == null)
        //			{
        //				throw new ArgumentNullException("eventName");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEventPattern<TSender, TEventArgs>(target, eventName, scheduler);
        //		}

        //		public static IObservable<EventPattern<object>> FromEventPattern(Type type, string eventName)
        //		{
        //			if (type == null)
        //			{
        //				throw new ArgumentNullException("type");
        //			}
        //			if (eventName == null)
        //			{
        //				throw new ArgumentNullException("eventName");
        //			}
        //			return s_impl.FromEventPattern(type, eventName);
        //		}

        //		public static IObservable<EventPattern<object>> FromEventPattern(Type type, string eventName, IScheduler scheduler)
        //		{
        //			if (type == null)
        //			{
        //				throw new ArgumentNullException("type");
        //			}
        //			if (eventName == null)
        //			{
        //				throw new ArgumentNullException("eventName");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEventPattern(type, eventName, scheduler);
        //		}

        //		public static IObservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(Type type, string eventName)
        //		{
        //			if (type == null)
        //			{
        //				throw new ArgumentNullException("type");
        //			}
        //			if (eventName == null)
        //			{
        //				throw new ArgumentNullException("eventName");
        //			}
        //			return s_impl.FromEventPattern<TEventArgs>(type, eventName);
        //		}

        //		public static IObservable<EventPattern<TEventArgs>> FromEventPattern<TEventArgs>(Type type, string eventName, IScheduler scheduler)
        //		{
        //			if (type == null)
        //			{
        //				throw new ArgumentNullException("type");
        //			}
        //			if (eventName == null)
        //			{
        //				throw new ArgumentNullException("eventName");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEventPattern<TEventArgs>(type, eventName, scheduler);
        //		}

        //		public static IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TSender, TEventArgs>(Type type, string eventName)
        //		{
        //			if (type == null)
        //			{
        //				throw new ArgumentNullException("type");
        //			}
        //			if (eventName == null)
        //			{
        //				throw new ArgumentNullException("eventName");
        //			}
        //			return s_impl.FromEventPattern<TSender, TEventArgs>(type, eventName);
        //		}

        //		public static IObservable<EventPattern<TSender, TEventArgs>> FromEventPattern<TSender, TEventArgs>(Type type, string eventName, IScheduler scheduler)
        //		{
        //			if (type == null)
        //			{
        //				throw new ArgumentNullException("type");
        //			}
        //			if (eventName == null)
        //			{
        //				throw new ArgumentNullException("eventName");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEventPattern<TSender, TEventArgs>(type, eventName, scheduler);
        //		}

        //		public static IObservable<TEventArgs> FromEvent<TDelegate, TEventArgs>(Func<Action<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
        //		{
        //			if (conversion == null)
        //			{
        //				throw new ArgumentNullException("conversion");
        //			}
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			return s_impl.FromEvent(conversion, addHandler, removeHandler);
        //		}

        //		public static IObservable<TEventArgs> FromEvent<TDelegate, TEventArgs>(Func<Action<TEventArgs>, TDelegate> conversion, Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
        //		{
        //			if (conversion == null)
        //			{
        //				throw new ArgumentNullException("conversion");
        //			}
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEvent(conversion, addHandler, removeHandler, scheduler);
        //		}

        //		public static IObservable<TEventArgs> FromEvent<TDelegate, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			return s_impl.FromEvent<TDelegate, TEventArgs>(addHandler, removeHandler);
        //		}

        //		public static IObservable<TEventArgs> FromEvent<TDelegate, TEventArgs>(Action<TDelegate> addHandler, Action<TDelegate> removeHandler, IScheduler scheduler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEvent<TDelegate, TEventArgs>(addHandler, removeHandler, scheduler);
        //		}

        //		public static IObservable<TEventArgs> FromEvent<TEventArgs>(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			return s_impl.FromEvent(addHandler, removeHandler);
        //		}

        //		public static IObservable<TEventArgs> FromEvent<TEventArgs>(Action<Action<TEventArgs>> addHandler, Action<Action<TEventArgs>> removeHandler, IScheduler scheduler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEvent(addHandler, removeHandler, scheduler);
        //		}

        //		public static IObservable<Unit> FromEvent(Action<Action> addHandler, Action<Action> removeHandler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			return s_impl.FromEvent(addHandler, removeHandler);
        //		}

        //		public static IObservable<Unit> FromEvent(Action<Action> addHandler, Action<Action> removeHandler, IScheduler scheduler)
        //		{
        //			if (addHandler == null)
        //			{
        //				throw new ArgumentNullException("addHandler");
        //			}
        //			if (removeHandler == null)
        //			{
        //				throw new ArgumentNullException("removeHandler");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.FromEvent(addHandler, removeHandler, scheduler);
        //		}

        //		public static Task ForEachAsync<TSource>(this IObservable<TSource> source, Action<TSource> onNext)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (onNext == null)
        //			{
        //				throw new ArgumentNullException("onNext");
        //			}
        //			return s_impl.ForEachAsync(source, onNext);
        //		}

        //		public static Task ForEachAsync<TSource>(this IObservable<TSource> source, Action<TSource> onNext, CancellationToken cancellationToken)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (onNext == null)
        //			{
        //				throw new ArgumentNullException("onNext");
        //			}
        //			return s_impl.ForEachAsync(source, onNext, cancellationToken);
        //		}

        //		public static Task ForEachAsync<TSource>(this IObservable<TSource> source, Action<TSource, int> onNext)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (onNext == null)
        //			{
        //				throw new ArgumentNullException("onNext");
        //			}
        //			return s_impl.ForEachAsync(source, onNext);
        //		}

        //		public static Task ForEachAsync<TSource>(this IObservable<TSource> source, Action<TSource, int> onNext, CancellationToken cancellationToken)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (onNext == null)
        //			{
        //				throw new ArgumentNullException("onNext");
        //			}
        //			return s_impl.ForEachAsync(source, onNext, cancellationToken);
        //		}

        //		public static IObservable<TResult> Case<TValue, TResult>(Func<TValue> selector, IDictionary<TValue, IObservable<TResult>> sources, IObservable<TResult> defaultSource) where TValue : notnull
        //		{
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			if (defaultSource == null)
        //			{
        //				throw new ArgumentNullException("defaultSource");
        //			}
        //			return s_impl.Case(selector, sources, defaultSource);
        //		}

        //		public static IObservable<TResult> Case<TValue, TResult>(Func<TValue> selector, IDictionary<TValue, IObservable<TResult>> sources, IScheduler scheduler) where TValue : notnull
        //		{
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Case(selector, sources, scheduler);
        //		}

        //		public static IObservable<TResult> Case<TValue, TResult>(Func<TValue> selector, IDictionary<TValue, IObservable<TResult>> sources) where TValue : notnull
        //		{
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Case(selector, sources);
        //		}

        //		public static IObservable<TSource> DoWhile<TSource>(this IObservable<TSource> source, Func<bool> condition)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (condition == null)
        //			{
        //				throw new ArgumentNullException("condition");
        //			}
        //			return s_impl.DoWhile(source, condition);
        //		}

        //		public static IObservable<TResult> For<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, IObservable<TResult>> resultSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.For(source, resultSelector);
        //		}

        //		public static IObservable<TResult> If<TResult>(Func<bool> condition, IObservable<TResult> thenSource, IObservable<TResult> elseSource)
        //		{
        //			if (condition == null)
        //			{
        //				throw new ArgumentNullException("condition");
        //			}
        //			if (thenSource == null)
        //			{
        //				throw new ArgumentNullException("thenSource");
        //			}
        //			if (elseSource == null)
        //			{
        //				throw new ArgumentNullException("elseSource");
        //			}
        //			return s_impl.If(condition, thenSource, elseSource);
        //		}

        //		public static IObservable<TResult> If<TResult>(Func<bool> condition, IObservable<TResult> thenSource)
        //		{
        //			if (condition == null)
        //			{
        //				throw new ArgumentNullException("condition");
        //			}
        //			if (thenSource == null)
        //			{
        //				throw new ArgumentNullException("thenSource");
        //			}
        //			return s_impl.If(condition, thenSource);
        //		}

        //		public static IObservable<TResult> If<TResult>(Func<bool> condition, IObservable<TResult> thenSource, IScheduler scheduler)
        //		{
        //			if (condition == null)
        //			{
        //				throw new ArgumentNullException("condition");
        //			}
        //			if (thenSource == null)
        //			{
        //				throw new ArgumentNullException("thenSource");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.If(condition, thenSource, scheduler);
        //		}

        //		public static IObservable<TSource> While<TSource>(Func<bool> condition, IObservable<TSource> source)
        //		{
        //			if (condition == null)
        //			{
        //				throw new ArgumentNullException("condition");
        //			}
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.While(condition, source);
        //		}

        //		public static Pattern<TLeft, TRight> And<TLeft, TRight>(this IObservable<TLeft> left, IObservable<TRight> right)
        //		{
        //			if (left == null)
        //			{
        //				throw new ArgumentNullException("left");
        //			}
        //			if (right == null)
        //			{
        //				throw new ArgumentNullException("right");
        //			}
        //			return s_impl.And(left, right);
        //		}

        //		public static Plan<TResult> Then<TSource, TResult>(this IObservable<TSource> source, Func<TSource, TResult> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.Then(source, selector);
        //		}

        //		public static IObservable<TResult> When<TResult>(params Plan<TResult>[] plans)
        //		{
        //			if (plans == null)
        //			{
        //				throw new ArgumentNullException("plans");
        //			}
        //			return s_impl.When(plans);
        //		}

        //		public static IObservable<TResult> When<TResult>(this IEnumerable<Plan<TResult>> plans)
        //		{
        //			if (plans == null)
        //			{
        //				throw new ArgumentNullException("plans");
        //			}
        //			return s_impl.When(plans);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, Func<TSource1, TSource2, TSource3, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, Func<TSource1, TSource2, TSource3, TSource4, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, source4, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, source4, source5, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, source4, source5, source6, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, source4, source5, source6, source7, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, source4, source5, source6, source7, source8, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, source4, source5, source6, source7, source8, source9, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (source11 == null)
        //			{
        //				throw new ArgumentNullException("source11");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (source11 == null)
        //			{
        //				throw new ArgumentNullException("source11");
        //			}
        //			if (source12 == null)
        //			{
        //				throw new ArgumentNullException("source12");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (source11 == null)
        //			{
        //				throw new ArgumentNullException("source11");
        //			}
        //			if (source12 == null)
        //			{
        //				throw new ArgumentNullException("source12");
        //			}
        //			if (source13 == null)
        //			{
        //				throw new ArgumentNullException("source13");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (source11 == null)
        //			{
        //				throw new ArgumentNullException("source11");
        //			}
        //			if (source12 == null)
        //			{
        //				throw new ArgumentNullException("source12");
        //			}
        //			if (source13 == null)
        //			{
        //				throw new ArgumentNullException("source13");
        //			}
        //			if (source14 == null)
        //			{
        //				throw new ArgumentNullException("source14");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, IObservable<TSource15> source15, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (source11 == null)
        //			{
        //				throw new ArgumentNullException("source11");
        //			}
        //			if (source12 == null)
        //			{
        //				throw new ArgumentNullException("source12");
        //			}
        //			if (source13 == null)
        //			{
        //				throw new ArgumentNullException("source13");
        //			}
        //			if (source14 == null)
        //			{
        //				throw new ArgumentNullException("source14");
        //			}
        //			if (source15 == null)
        //			{
        //				throw new ArgumentNullException("source15");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, source15, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, IObservable<TSource15> source15, IObservable<TSource16> source16, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (source11 == null)
        //			{
        //				throw new ArgumentNullException("source11");
        //			}
        //			if (source12 == null)
        //			{
        //				throw new ArgumentNullException("source12");
        //			}
        //			if (source13 == null)
        //			{
        //				throw new ArgumentNullException("source13");
        //			}
        //			if (source14 == null)
        //			{
        //				throw new ArgumentNullException("source14");
        //			}
        //			if (source15 == null)
        //			{
        //				throw new ArgumentNullException("source15");
        //			}
        //			if (source16 == null)
        //			{
        //				throw new ArgumentNullException("source16");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, source15, source16, resultSelector);
        //		}

        //		public static IObservable<TSource> Amb<TSource>(this IObservable<TSource> first, IObservable<TSource> second)
        //		{
        //			if (first == null)
        //			{
        //				throw new ArgumentNullException("first");
        //			}
        //			if (second == null)
        //			{
        //				throw new ArgumentNullException("second");
        //			}
        //			return s_impl.Amb(first, second);
        //		}

        //		public static IObservable<TSource> Amb<TSource>(params IObservable<TSource>[] sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Amb(sources);
        //		}

        //		public static IObservable<TSource> Amb<TSource>(this IEnumerable<IObservable<TSource>> sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Amb(sources);
        //		}

        //		public static IObservable<IList<TSource>> Buffer<TSource, TBufferClosing>(this IObservable<TSource> source, Func<IObservable<TBufferClosing>> bufferClosingSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (bufferClosingSelector == null)
        //			{
        //				throw new ArgumentNullException("bufferClosingSelector");
        //			}
        //			return s_impl.Buffer(source, bufferClosingSelector);
        //		}

        //		public static IObservable<IList<TSource>> Buffer<TSource, TBufferOpening, TBufferClosing>(this IObservable<TSource> source, IObservable<TBufferOpening> bufferOpenings, Func<TBufferOpening, IObservable<TBufferClosing>> bufferClosingSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (bufferOpenings == null)
        //			{
        //				throw new ArgumentNullException("bufferOpenings");
        //			}
        //			if (bufferClosingSelector == null)
        //			{
        //				throw new ArgumentNullException("bufferClosingSelector");
        //			}
        //			return s_impl.Buffer(source, bufferOpenings, bufferClosingSelector);
        //		}

        //		public static IObservable<IList<TSource>> Buffer<TSource, TBufferBoundary>(this IObservable<TSource> source, IObservable<TBufferBoundary> bufferBoundaries)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (bufferBoundaries == null)
        //			{
        //				throw new ArgumentNullException("bufferBoundaries");
        //			}
        //			return s_impl.Buffer(source, bufferBoundaries);
        //		}

        //		public static IObservable<TSource> Catch<TSource, TException>(this IObservable<TSource> source, Func<TException, IObservable<TSource>> handler) where TException : Exception
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (handler == null)
        //			{
        //				throw new ArgumentNullException("handler");
        //			}
        //			return s_impl.Catch(source, handler);
        //		}

        //		public static IObservable<TSource> Catch<TSource>(this IObservable<TSource> first, IObservable<TSource> second)
        //		{
        //			if (first == null)
        //			{
        //				throw new ArgumentNullException("first");
        //			}
        //			if (second == null)
        //			{
        //				throw new ArgumentNullException("second");
        //			}
        //			return s_impl.Catch(first, second);
        //		}

        //		public static IObservable<TSource> Catch<TSource>(params IObservable<TSource>[] sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Catch(sources);
        //		}

        //		public static IObservable<TSource> Catch<TSource>(this IEnumerable<IObservable<TSource>> sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Catch(sources);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource1, TSource2, TResult>(this IObservable<TSource1> first, IObservable<TSource2> second, Func<TSource1, TSource2, TResult> resultSelector)
        //		{
        //			if (first == null)
        //			{
        //				throw new ArgumentNullException("first");
        //			}
        //			if (second == null)
        //			{
        //				throw new ArgumentNullException("second");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(first, second, resultSelector);
        //		}

        //		public static IObservable<TResult> CombineLatest<TSource, TResult>(this IEnumerable<IObservable<TSource>> sources, Func<IList<TSource>, TResult> resultSelector)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.CombineLatest(sources, resultSelector);
        //		}

        //		public static IObservable<IList<TSource>> CombineLatest<TSource>(this IEnumerable<IObservable<TSource>> sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.CombineLatest(sources);
        //		}

        //		public static IObservable<IList<TSource>> CombineLatest<TSource>(params IObservable<TSource>[] sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.CombineLatest(sources);
        //		}

        //		public static IObservable<TSource> Concat<TSource>(this IObservable<TSource> first, IObservable<TSource> second)
        //		{
        //			if (first == null)
        //			{
        //				throw new ArgumentNullException("first");
        //			}
        //			if (second == null)
        //			{
        //				throw new ArgumentNullException("second");
        //			}
        //			return s_impl.Concat(first, second);
        //		}

        //		public static IObservable<TSource> Concat<TSource>(params IObservable<TSource>[] sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Concat(sources);
        //		}

        //		public static IObservable<TSource> Concat<TSource>(this IEnumerable<IObservable<TSource>> sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Concat(sources);
        //		}

        //		public static IObservable<TSource> Concat<TSource>(this IObservable<IObservable<TSource>> sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Concat(sources);
        //		}

        //		public static IObservable<TSource> Concat<TSource>(this IObservable<Task<TSource>> sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Concat(sources);
        //		}

        //		public static IObservable<TSource> Merge<TSource>(this IObservable<IObservable<TSource>> sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Merge(sources);
        //		}

        //		public static IObservable<TSource> Merge<TSource>(this IObservable<Task<TSource>> sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Merge(sources);
        //		}

        //		public static IObservable<TSource> Merge<TSource>(this IObservable<IObservable<TSource>> sources, int maxConcurrent)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			if (maxConcurrent <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("maxConcurrent");
        //			}
        //			return s_impl.Merge(sources, maxConcurrent);
        //		}

        //		public static IObservable<TSource> Merge<TSource>(this IEnumerable<IObservable<TSource>> sources, int maxConcurrent)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			if (maxConcurrent <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("maxConcurrent");
        //			}
        //			return s_impl.Merge(sources, maxConcurrent);
        //		}

        //		public static IObservable<TSource> Merge<TSource>(this IEnumerable<IObservable<TSource>> sources, int maxConcurrent, IScheduler scheduler)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			if (maxConcurrent <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("maxConcurrent");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Merge(sources, maxConcurrent, scheduler);
        //		}

        //		public static IObservable<TSource> Merge<TSource>(this IObservable<TSource> first, IObservable<TSource> second)
        //		{
        //			if (first == null)
        //			{
        //				throw new ArgumentNullException("first");
        //			}
        //			if (second == null)
        //			{
        //				throw new ArgumentNullException("second");
        //			}
        //			return s_impl.Merge(first, second);
        //		}

        //		public static IObservable<TSource> Merge<TSource>(this IObservable<TSource> first, IObservable<TSource> second, IScheduler scheduler)
        //		{
        //			if (first == null)
        //			{
        //				throw new ArgumentNullException("first");
        //			}
        //			if (second == null)
        //			{
        //				throw new ArgumentNullException("second");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Merge(first, second, scheduler);
        //		}

        //		public static IObservable<TSource> Merge<TSource>(params IObservable<TSource>[] sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Merge(sources);
        //		}

        //		public static IObservable<TSource> Merge<TSource>(IScheduler scheduler, params IObservable<TSource>[] sources)
        //		{
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Merge(scheduler, sources);
        //		}

        //		public static IObservable<TSource> Merge<TSource>(this IEnumerable<IObservable<TSource>> sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Merge(sources);
        //		}

        //		public static IObservable<TSource> Merge<TSource>(this IEnumerable<IObservable<TSource>> sources, IScheduler scheduler)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Merge(sources, scheduler);
        //		}

        //		public static IObservable<TSource> OnErrorResumeNext<TSource>(this IObservable<TSource> first, IObservable<TSource> second)
        //		{
        //			if (first == null)
        //			{
        //				throw new ArgumentNullException("first");
        //			}
        //			if (second == null)
        //			{
        //				throw new ArgumentNullException("second");
        //			}
        //			return s_impl.OnErrorResumeNext(first, second);
        //		}

        //		public static IObservable<TSource> OnErrorResumeNext<TSource>(params IObservable<TSource>[] sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.OnErrorResumeNext(sources);
        //		}

        //		public static IObservable<TSource> OnErrorResumeNext<TSource>(this IEnumerable<IObservable<TSource>> sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.OnErrorResumeNext(sources);
        //		}

        //		public static IObservable<TSource> SkipUntil<TSource, TOther>(this IObservable<TSource> source, IObservable<TOther> other)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (other == null)
        //			{
        //				throw new ArgumentNullException("other");
        //			}
        //			return s_impl.SkipUntil(source, other);
        //		}

        //		public static IObservable<TSource> Switch<TSource>(this IObservable<IObservable<TSource>> sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Switch(sources);
        //		}

        //		public static IObservable<TSource> Switch<TSource>(this IObservable<Task<TSource>> sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Switch(sources);
        //		}

        //		public static IObservable<TSource> TakeUntil<TSource, TOther>(this IObservable<TSource> source, IObservable<TOther> other)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (other == null)
        //			{
        //				throw new ArgumentNullException("other");
        //			}
        //			return s_impl.TakeUntil(source, other);
        //		}

        //		public static IObservable<TSource> TakeUntil<TSource>(this IObservable<TSource> source, Func<TSource, bool> stopPredicate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (stopPredicate == null)
        //			{
        //				throw new ArgumentNullException("stopPredicate");
        //			}
        //			return s_impl.TakeUntil(source, stopPredicate);
        //		}

        //		public static IObservable<IObservable<TSource>> Window<TSource, TWindowClosing>(this IObservable<TSource> source, Func<IObservable<TWindowClosing>> windowClosingSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (windowClosingSelector == null)
        //			{
        //				throw new ArgumentNullException("windowClosingSelector");
        //			}
        //			return s_impl.Window(source, windowClosingSelector);
        //		}

        //		public static IObservable<IObservable<TSource>> Window<TSource, TWindowOpening, TWindowClosing>(this IObservable<TSource> source, IObservable<TWindowOpening> windowOpenings, Func<TWindowOpening, IObservable<TWindowClosing>> windowClosingSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (windowOpenings == null)
        //			{
        //				throw new ArgumentNullException("windowOpenings");
        //			}
        //			if (windowClosingSelector == null)
        //			{
        //				throw new ArgumentNullException("windowClosingSelector");
        //			}
        //			return s_impl.Window(source, windowOpenings, windowClosingSelector);
        //		}

        //		public static IObservable<IObservable<TSource>> Window<TSource, TWindowBoundary>(this IObservable<TSource> source, IObservable<TWindowBoundary> windowBoundaries)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (windowBoundaries == null)
        //			{
        //				throw new ArgumentNullException("windowBoundaries");
        //			}
        //			return s_impl.Window(source, windowBoundaries);
        //		}

        //		public static IObservable<TResult> WithLatestFrom<TFirst, TSecond, TResult>(this IObservable<TFirst> first, IObservable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        //		{
        //			if (first == null)
        //			{
        //				throw new ArgumentNullException("first");
        //			}
        //			if (second == null)
        //			{
        //				throw new ArgumentNullException("second");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.WithLatestFrom(first, second, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TResult>(this IObservable<TSource1> first, IObservable<TSource2> second, Func<TSource1, TSource2, TResult> resultSelector)
        //		{
        //			if (first == null)
        //			{
        //				throw new ArgumentNullException("first");
        //			}
        //			if (second == null)
        //			{
        //				throw new ArgumentNullException("second");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(first, second, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource, TResult>(this IEnumerable<IObservable<TSource>> sources, Func<IList<TSource>, TResult> resultSelector)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(sources, resultSelector);
        //		}

        //		public static IObservable<IList<TSource>> Zip<TSource>(this IEnumerable<IObservable<TSource>> sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Zip(sources);
        //		}

        //		public static IObservable<IList<TSource>> Zip<TSource>(params IObservable<TSource>[] sources)
        //		{
        //			if (sources == null)
        //			{
        //				throw new ArgumentNullException("sources");
        //			}
        //			return s_impl.Zip(sources);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TResult>(this IObservable<TSource1> first, IEnumerable<TSource2> second, Func<TSource1, TSource2, TResult> resultSelector)
        //		{
        //			if (first == null)
        //			{
        //				throw new ArgumentNullException("first");
        //			}
        //			if (second == null)
        //			{
        //				throw new ArgumentNullException("second");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(first, second, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, Func<TSource1, TSource2, TSource3, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, Func<TSource1, TSource2, TSource3, TSource4, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, source4, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, source4, source5, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, source4, source5, source6, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, source4, source5, source6, source7, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, source4, source5, source6, source7, source8, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, source4, source5, source6, source7, source8, source9, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (source11 == null)
        //			{
        //				throw new ArgumentNullException("source11");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (source11 == null)
        //			{
        //				throw new ArgumentNullException("source11");
        //			}
        //			if (source12 == null)
        //			{
        //				throw new ArgumentNullException("source12");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (source11 == null)
        //			{
        //				throw new ArgumentNullException("source11");
        //			}
        //			if (source12 == null)
        //			{
        //				throw new ArgumentNullException("source12");
        //			}
        //			if (source13 == null)
        //			{
        //				throw new ArgumentNullException("source13");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (source11 == null)
        //			{
        //				throw new ArgumentNullException("source11");
        //			}
        //			if (source12 == null)
        //			{
        //				throw new ArgumentNullException("source12");
        //			}
        //			if (source13 == null)
        //			{
        //				throw new ArgumentNullException("source13");
        //			}
        //			if (source14 == null)
        //			{
        //				throw new ArgumentNullException("source14");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, IObservable<TSource15> source15, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (source11 == null)
        //			{
        //				throw new ArgumentNullException("source11");
        //			}
        //			if (source12 == null)
        //			{
        //				throw new ArgumentNullException("source12");
        //			}
        //			if (source13 == null)
        //			{
        //				throw new ArgumentNullException("source13");
        //			}
        //			if (source14 == null)
        //			{
        //				throw new ArgumentNullException("source14");
        //			}
        //			if (source15 == null)
        //			{
        //				throw new ArgumentNullException("source15");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, source15, resultSelector);
        //		}

        //		public static IObservable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4, IObservable<TSource5> source5, IObservable<TSource6> source6, IObservable<TSource7> source7, IObservable<TSource8> source8, IObservable<TSource9> source9, IObservable<TSource10> source10, IObservable<TSource11> source11, IObservable<TSource12> source12, IObservable<TSource13> source13, IObservable<TSource14> source14, IObservable<TSource15> source15, IObservable<TSource16> source16, Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TSource9, TSource10, TSource11, TSource12, TSource13, TSource14, TSource15, TSource16, TResult> resultSelector)
        //		{
        //			if (source1 == null)
        //			{
        //				throw new ArgumentNullException("source1");
        //			}
        //			if (source2 == null)
        //			{
        //				throw new ArgumentNullException("source2");
        //			}
        //			if (source3 == null)
        //			{
        //				throw new ArgumentNullException("source3");
        //			}
        //			if (source4 == null)
        //			{
        //				throw new ArgumentNullException("source4");
        //			}
        //			if (source5 == null)
        //			{
        //				throw new ArgumentNullException("source5");
        //			}
        //			if (source6 == null)
        //			{
        //				throw new ArgumentNullException("source6");
        //			}
        //			if (source7 == null)
        //			{
        //				throw new ArgumentNullException("source7");
        //			}
        //			if (source8 == null)
        //			{
        //				throw new ArgumentNullException("source8");
        //			}
        //			if (source9 == null)
        //			{
        //				throw new ArgumentNullException("source9");
        //			}
        //			if (source10 == null)
        //			{
        //				throw new ArgumentNullException("source10");
        //			}
        //			if (source11 == null)
        //			{
        //				throw new ArgumentNullException("source11");
        //			}
        //			if (source12 == null)
        //			{
        //				throw new ArgumentNullException("source12");
        //			}
        //			if (source13 == null)
        //			{
        //				throw new ArgumentNullException("source13");
        //			}
        //			if (source14 == null)
        //			{
        //				throw new ArgumentNullException("source14");
        //			}
        //			if (source15 == null)
        //			{
        //				throw new ArgumentNullException("source15");
        //			}
        //			if (source16 == null)
        //			{
        //				throw new ArgumentNullException("source16");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Zip(source1, source2, source3, source4, source5, source6, source7, source8, source9, source10, source11, source12, source13, source14, source15, source16, resultSelector);
        //		}

        //		public static IObservable<TSource> Append<TSource>(this IObservable<TSource> source, TSource value)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Append(source, value);
        //		}

        //		public static IObservable<TSource> Append<TSource>(this IObservable<TSource> source, TSource value, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Append(source, value, scheduler);
        //		}

        //		public static IObservable<TSource> AsObservable<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.AsObservable(source);
        //		}

        //		public static IObservable<IList<TSource>> Buffer<TSource>(this IObservable<TSource> source, int count)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (count <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			return s_impl.Buffer(source, count);
        //		}

        //		public static IObservable<IList<TSource>> Buffer<TSource>(this IObservable<TSource> source, int count, int skip)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (count <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			if (skip <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("skip");
        //			}
        //			return s_impl.Buffer(source, count, skip);
        //		}

        //		public static IObservable<TSource> Dematerialize<TSource>(this IObservable<Notification<TSource>> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Dematerialize(source);
        //		}

        //		public static IObservable<TSource> DistinctUntilChanged<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.DistinctUntilChanged(source);
        //		}

        //		public static IObservable<TSource> DistinctUntilChanged<TSource>(this IObservable<TSource> source, IEqualityComparer<TSource> comparer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (comparer == null)
        //			{
        //				throw new ArgumentNullException("comparer");
        //			}
        //			return s_impl.DistinctUntilChanged(source, comparer);
        //		}

        //		public static IObservable<TSource> DistinctUntilChanged<TSource, TKey>(this IObservable<TSource> source, Func<TSource, TKey> keySelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			return s_impl.DistinctUntilChanged(source, keySelector);
        //		}

        //		public static IObservable<TSource> DistinctUntilChanged<TSource, TKey>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (comparer == null)
        //			{
        //				throw new ArgumentNullException("comparer");
        //			}
        //			return s_impl.DistinctUntilChanged(source, keySelector, comparer);
        //		}

        //		public static IObservable<TSource> Do<TSource>(this IObservable<TSource> source, Action<TSource> onNext)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (onNext == null)
        //			{
        //				throw new ArgumentNullException("onNext");
        //			}
        //			return s_impl.Do(source, onNext);
        //		}

        //		public static IObservable<TSource> Do<TSource>(this IObservable<TSource> source, Action<TSource> onNext, Action onCompleted)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (onNext == null)
        //			{
        //				throw new ArgumentNullException("onNext");
        //			}
        //			if (onCompleted == null)
        //			{
        //				throw new ArgumentNullException("onCompleted");
        //			}
        //			return s_impl.Do(source, onNext, onCompleted);
        //		}

        //		public static IObservable<TSource> Do<TSource>(this IObservable<TSource> source, Action<TSource> onNext, Action<Exception> onError)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (onNext == null)
        //			{
        //				throw new ArgumentNullException("onNext");
        //			}
        //			if (onError == null)
        //			{
        //				throw new ArgumentNullException("onError");
        //			}
        //			return s_impl.Do(source, onNext, onError);
        //		}

        //		public static IObservable<TSource> Do<TSource>(this IObservable<TSource> source, Action<TSource> onNext, Action<Exception> onError, Action onCompleted)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (onNext == null)
        //			{
        //				throw new ArgumentNullException("onNext");
        //			}
        //			if (onError == null)
        //			{
        //				throw new ArgumentNullException("onError");
        //			}
        //			if (onCompleted == null)
        //			{
        //				throw new ArgumentNullException("onCompleted");
        //			}
        //			return s_impl.Do(source, onNext, onError, onCompleted);
        //		}

        //		public static IObservable<TSource> Do<TSource>(this IObservable<TSource> source, IObserver<TSource> observer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (observer == null)
        //			{
        //				throw new ArgumentNullException("observer");
        //			}
        //			return s_impl.Do(source, observer);
        //		}

        //		public static IObservable<TSource> Finally<TSource>(this IObservable<TSource> source, Action finallyAction)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (finallyAction == null)
        //			{
        //				throw new ArgumentNullException("finallyAction");
        //			}
        //			return s_impl.Finally(source, finallyAction);
        //		}

        //		public static IObservable<TSource> IgnoreElements<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.IgnoreElements(source);
        //		}

        //		public static IObservable<Notification<TSource>> Materialize<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Materialize(source);
        //		}

        //		public static IObservable<TSource> Prepend<TSource>(this IObservable<TSource> source, TSource value)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Prepend(source, value);
        //		}

        //		public static IObservable<TSource> Prepend<TSource>(this IObservable<TSource> source, TSource value, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Prepend(source, value, scheduler);
        //		}

        //		public static IObservable<TSource> Repeat<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Repeat(source);
        //		}

        //		public static IObservable<TSource> Repeat<TSource>(this IObservable<TSource> source, int repeatCount)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (repeatCount < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("repeatCount");
        //			}
        //			return s_impl.Repeat(source, repeatCount);
        //		}

        //		public static IObservable<TSource> RepeatWhen<TSource, TSignal>(this IObservable<TSource> source, Func<IObservable<object>, IObservable<TSignal>> handler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (handler == null)
        //			{
        //				throw new ArgumentNullException("handler");
        //			}
        //			return s_impl.RepeatWhen(source, handler);
        //		}

        //		public static IObservable<TSource> Retry<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Retry(source);
        //		}

        //		public static IObservable<TSource> Retry<TSource>(this IObservable<TSource> source, int retryCount)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (retryCount < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("retryCount");
        //			}
        //			return s_impl.Retry(source, retryCount);
        //		}

        //		public static IObservable<TSource> RetryWhen<TSource, TSignal>(this IObservable<TSource> source, Func<IObservable<Exception>, IObservable<TSignal>> handler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (handler == null)
        //			{
        //				throw new ArgumentNullException("handler");
        //			}
        //			return s_impl.RetryWhen(source, handler);
        //		}

        //		public static IObservable<TAccumulate> Scan<TSource, TAccumulate>(this IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (accumulator == null)
        //			{
        //				throw new ArgumentNullException("accumulator");
        //			}
        //			return s_impl.Scan(source, seed, accumulator);
        //		}

        //		public static IObservable<TSource> Scan<TSource>(this IObservable<TSource> source, Func<TSource, TSource, TSource> accumulator)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (accumulator == null)
        //			{
        //				throw new ArgumentNullException("accumulator");
        //			}
        //			return s_impl.Scan(source, accumulator);
        //		}

        //		public static IObservable<TSource> SkipLast<TSource>(this IObservable<TSource> source, int count)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (count < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			return s_impl.SkipLast(source, count);
        //		}

        //		public static IObservable<TSource> StartWith<TSource>(this IObservable<TSource> source, params TSource[] values)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (values == null)
        //			{
        //				throw new ArgumentNullException("values");
        //			}
        //			return s_impl.StartWith(source, values);
        //		}

        //		public static IObservable<TSource> StartWith<TSource>(this IObservable<TSource> source, IEnumerable<TSource> values)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (values == null)
        //			{
        //				throw new ArgumentNullException("values");
        //			}
        //			return s_impl.StartWith(source, values);
        //		}

        //		public static IObservable<TSource> StartWith<TSource>(this IObservable<TSource> source, IScheduler scheduler, params TSource[] values)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			if (values == null)
        //			{
        //				throw new ArgumentNullException("values");
        //			}
        //			return s_impl.StartWith(source, scheduler, values);
        //		}

        //		public static IObservable<TSource> StartWith<TSource>(this IObservable<TSource> source, IScheduler scheduler, IEnumerable<TSource> values)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			if (values == null)
        //			{
        //				throw new ArgumentNullException("values");
        //			}
        //			return s_impl.StartWith(source, scheduler, values);
        //		}

        //		public static IObservable<TSource> TakeLast<TSource>(this IObservable<TSource> source, int count)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (count < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			return s_impl.TakeLast(source, count);
        //		}

        //		public static IObservable<TSource> TakeLast<TSource>(this IObservable<TSource> source, int count, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (count < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.TakeLast(source, count, scheduler);
        //		}

        //		public static IObservable<IList<TSource>> TakeLastBuffer<TSource>(this IObservable<TSource> source, int count)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (count < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			return s_impl.TakeLastBuffer(source, count);
        //		}

        //		public static IObservable<IObservable<TSource>> Window<TSource>(this IObservable<TSource> source, int count)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (count <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			return s_impl.Window(source, count);
        //		}

        //		public static IObservable<IObservable<TSource>> Window<TSource>(this IObservable<TSource> source, int count, int skip)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (count <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			if (skip <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("skip");
        //			}
        //			return s_impl.Window(source, count, skip);
        //		}

        //		public static IObservable<TResult> Cast<TResult>(this IObservable<object> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Cast<TResult>(source);
        //		}

        //		public static IObservable<TSource?> DefaultIfEmpty<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.DefaultIfEmpty(source);
        //		}

        //		public static IObservable<TSource> DefaultIfEmpty<TSource>(this IObservable<TSource> source, TSource defaultValue)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.DefaultIfEmpty(source, defaultValue);
        //		}

        //		public static IObservable<TSource> Distinct<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Distinct(source);
        //		}

        //		public static IObservable<TSource> Distinct<TSource>(this IObservable<TSource> source, IEqualityComparer<TSource> comparer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (comparer == null)
        //			{
        //				throw new ArgumentNullException("comparer");
        //			}
        //			return s_impl.Distinct(source, comparer);
        //		}

        //		public static IObservable<TSource> Distinct<TSource, TKey>(this IObservable<TSource> source, Func<TSource, TKey> keySelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			return s_impl.Distinct(source, keySelector);
        //		}

        //		public static IObservable<TSource> Distinct<TSource, TKey>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (comparer == null)
        //			{
        //				throw new ArgumentNullException("comparer");
        //			}
        //			return s_impl.Distinct(source, keySelector, comparer);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(this IObservable<TSource> source, Func<TSource, TKey> keySelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			return s_impl.GroupBy(source, keySelector);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (comparer == null)
        //			{
        //				throw new ArgumentNullException("comparer");
        //			}
        //			return s_impl.GroupBy(source, keySelector, comparer);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (elementSelector == null)
        //			{
        //				throw new ArgumentNullException("elementSelector");
        //			}
        //			return s_impl.GroupBy(source, keySelector, elementSelector);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (elementSelector == null)
        //			{
        //				throw new ArgumentNullException("elementSelector");
        //			}
        //			if (comparer == null)
        //			{
        //				throw new ArgumentNullException("comparer");
        //			}
        //			return s_impl.GroupBy(source, keySelector, elementSelector, comparer);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, int capacity)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (capacity < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("capacity");
        //			}
        //			return s_impl.GroupBy(source, keySelector, capacity);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TSource>> GroupBy<TSource, TKey>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, int capacity, IEqualityComparer<TKey> comparer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (capacity < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("capacity");
        //			}
        //			if (comparer == null)
        //			{
        //				throw new ArgumentNullException("comparer");
        //			}
        //			return s_impl.GroupBy(source, keySelector, capacity, comparer);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, int capacity)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (elementSelector == null)
        //			{
        //				throw new ArgumentNullException("elementSelector");
        //			}
        //			if (capacity < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("capacity");
        //			}
        //			return s_impl.GroupBy(source, keySelector, elementSelector, capacity);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TElement>> GroupBy<TSource, TKey, TElement>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, int capacity, IEqualityComparer<TKey> comparer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (elementSelector == null)
        //			{
        //				throw new ArgumentNullException("elementSelector");
        //			}
        //			if (capacity < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("capacity");
        //			}
        //			if (comparer == null)
        //			{
        //				throw new ArgumentNullException("comparer");
        //			}
        //			return s_impl.GroupBy(source, keySelector, elementSelector, capacity, comparer);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TSource, TKey, TElement, TDuration>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>> durationSelector, IEqualityComparer<TKey> comparer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (elementSelector == null)
        //			{
        //				throw new ArgumentNullException("elementSelector");
        //			}
        //			if (durationSelector == null)
        //			{
        //				throw new ArgumentNullException("durationSelector");
        //			}
        //			if (comparer == null)
        //			{
        //				throw new ArgumentNullException("comparer");
        //			}
        //			return s_impl.GroupByUntil(source, keySelector, elementSelector, durationSelector, comparer);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TSource, TKey, TElement, TDuration>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>> durationSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (elementSelector == null)
        //			{
        //				throw new ArgumentNullException("elementSelector");
        //			}
        //			if (durationSelector == null)
        //			{
        //				throw new ArgumentNullException("durationSelector");
        //			}
        //			return s_impl.GroupByUntil(source, keySelector, elementSelector, durationSelector);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TSource, TKey, TDuration>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<IGroupedObservable<TKey, TSource>, IObservable<TDuration>> durationSelector, IEqualityComparer<TKey> comparer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (durationSelector == null)
        //			{
        //				throw new ArgumentNullException("durationSelector");
        //			}
        //			if (comparer == null)
        //			{
        //				throw new ArgumentNullException("comparer");
        //			}
        //			return s_impl.GroupByUntil(source, keySelector, durationSelector, comparer);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TSource, TKey, TDuration>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<IGroupedObservable<TKey, TSource>, IObservable<TDuration>> durationSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (durationSelector == null)
        //			{
        //				throw new ArgumentNullException("durationSelector");
        //			}
        //			return s_impl.GroupByUntil(source, keySelector, durationSelector);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TSource, TKey, TElement, TDuration>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>> durationSelector, int capacity, IEqualityComparer<TKey> comparer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (elementSelector == null)
        //			{
        //				throw new ArgumentNullException("elementSelector");
        //			}
        //			if (durationSelector == null)
        //			{
        //				throw new ArgumentNullException("durationSelector");
        //			}
        //			if (capacity < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("capacity");
        //			}
        //			if (comparer == null)
        //			{
        //				throw new ArgumentNullException("comparer");
        //			}
        //			return s_impl.GroupByUntil(source, keySelector, elementSelector, durationSelector, capacity, comparer);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TSource, TKey, TElement, TDuration>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, Func<IGroupedObservable<TKey, TElement>, IObservable<TDuration>> durationSelector, int capacity)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (elementSelector == null)
        //			{
        //				throw new ArgumentNullException("elementSelector");
        //			}
        //			if (durationSelector == null)
        //			{
        //				throw new ArgumentNullException("durationSelector");
        //			}
        //			if (capacity < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("capacity");
        //			}
        //			return s_impl.GroupByUntil(source, keySelector, elementSelector, durationSelector, capacity);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TSource, TKey, TDuration>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<IGroupedObservable<TKey, TSource>, IObservable<TDuration>> durationSelector, int capacity, IEqualityComparer<TKey> comparer)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (durationSelector == null)
        //			{
        //				throw new ArgumentNullException("durationSelector");
        //			}
        //			if (capacity < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("capacity");
        //			}
        //			if (comparer == null)
        //			{
        //				throw new ArgumentNullException("comparer");
        //			}
        //			return s_impl.GroupByUntil(source, keySelector, durationSelector, capacity, comparer);
        //		}

        //		public static IObservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TSource, TKey, TDuration>(this IObservable<TSource> source, Func<TSource, TKey> keySelector, Func<IGroupedObservable<TKey, TSource>, IObservable<TDuration>> durationSelector, int capacity)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (keySelector == null)
        //			{
        //				throw new ArgumentNullException("keySelector");
        //			}
        //			if (durationSelector == null)
        //			{
        //				throw new ArgumentNullException("durationSelector");
        //			}
        //			if (capacity < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("capacity");
        //			}
        //			return s_impl.GroupByUntil(source, keySelector, durationSelector, capacity);
        //		}

        //		public static IObservable<TResult> GroupJoin<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(this IObservable<TLeft> left, IObservable<TRight> right, Func<TLeft, IObservable<TLeftDuration>> leftDurationSelector, Func<TRight, IObservable<TRightDuration>> rightDurationSelector, Func<TLeft, IObservable<TRight>, TResult> resultSelector)
        //		{
        //			if (left == null)
        //			{
        //				throw new ArgumentNullException("left");
        //			}
        //			if (right == null)
        //			{
        //				throw new ArgumentNullException("right");
        //			}
        //			if (leftDurationSelector == null)
        //			{
        //				throw new ArgumentNullException("leftDurationSelector");
        //			}
        //			if (rightDurationSelector == null)
        //			{
        //				throw new ArgumentNullException("rightDurationSelector");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.GroupJoin(left, right, leftDurationSelector, rightDurationSelector, resultSelector);
        //		}

        //		public static IObservable<TResult> Join<TLeft, TRight, TLeftDuration, TRightDuration, TResult>(this IObservable<TLeft> left, IObservable<TRight> right, Func<TLeft, IObservable<TLeftDuration>> leftDurationSelector, Func<TRight, IObservable<TRightDuration>> rightDurationSelector, Func<TLeft, TRight, TResult> resultSelector)
        //		{
        //			if (left == null)
        //			{
        //				throw new ArgumentNullException("left");
        //			}
        //			if (right == null)
        //			{
        //				throw new ArgumentNullException("right");
        //			}
        //			if (leftDurationSelector == null)
        //			{
        //				throw new ArgumentNullException("leftDurationSelector");
        //			}
        //			if (rightDurationSelector == null)
        //			{
        //				throw new ArgumentNullException("rightDurationSelector");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.Join(left, right, leftDurationSelector, rightDurationSelector, resultSelector);
        //		}

        //		public static IObservable<TResult> OfType<TResult>(this IObservable<object> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.OfType<TResult>(source);
        //		}

        //		public static IObservable<TResult> Select<TSource, TResult>(this IObservable<TSource> source, Func<TSource, TResult> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.Select(source, selector);
        //		}

        //		public static IObservable<TResult> Select<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, TResult> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.Select(source, selector);
        //		}

        //		public static IObservable<TOther> SelectMany<TSource, TOther>(this IObservable<TSource> source, IObservable<TOther> other)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (other == null)
        //			{
        //				throw new ArgumentNullException("other");
        //			}
        //			return s_impl.SelectMany(source, other);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TResult>> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.SelectMany(source, selector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, IObservable<TResult>> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.SelectMany(source, selector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, Task<TResult>> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.SelectMany(source, selector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, Task<TResult>> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.SelectMany(source, selector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, CancellationToken, Task<TResult>> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.SelectMany(source, selector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TResult>> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.SelectMany(source, selector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (collectionSelector == null)
        //			{
        //				throw new ArgumentNullException("collectionSelector");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.SelectMany(source, collectionSelector, resultSelector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, int, IObservable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (collectionSelector == null)
        //			{
        //				throw new ArgumentNullException("collectionSelector");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.SelectMany(source, collectionSelector, resultSelector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, Task<TTaskResult>> taskSelector, Func<TSource, TTaskResult, TResult> resultSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (taskSelector == null)
        //			{
        //				throw new ArgumentNullException("taskSelector");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.SelectMany(source, taskSelector, resultSelector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, int, Task<TTaskResult>> taskSelector, Func<TSource, int, TTaskResult, TResult> resultSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (taskSelector == null)
        //			{
        //				throw new ArgumentNullException("taskSelector");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.SelectMany(source, taskSelector, resultSelector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, CancellationToken, Task<TTaskResult>> taskSelector, Func<TSource, TTaskResult, TResult> resultSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (taskSelector == null)
        //			{
        //				throw new ArgumentNullException("taskSelector");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.SelectMany(source, taskSelector, resultSelector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TTaskResult, TResult>(this IObservable<TSource> source, Func<TSource, int, CancellationToken, Task<TTaskResult>> taskSelector, Func<TSource, int, TTaskResult, TResult> resultSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (taskSelector == null)
        //			{
        //				throw new ArgumentNullException("taskSelector");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.SelectMany(source, taskSelector, resultSelector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IObservable<TResult>> onNext, Func<Exception, IObservable<TResult>> onError, Func<IObservable<TResult>> onCompleted)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (onNext == null)
        //			{
        //				throw new ArgumentNullException("onNext");
        //			}
        //			if (onError == null)
        //			{
        //				throw new ArgumentNullException("onError");
        //			}
        //			if (onCompleted == null)
        //			{
        //				throw new ArgumentNullException("onCompleted");
        //			}
        //			return s_impl.SelectMany(source, onNext, onError, onCompleted);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, IObservable<TResult>> onNext, Func<Exception, IObservable<TResult>> onError, Func<IObservable<TResult>> onCompleted)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (onNext == null)
        //			{
        //				throw new ArgumentNullException("onNext");
        //			}
        //			if (onError == null)
        //			{
        //				throw new ArgumentNullException("onError");
        //			}
        //			if (onCompleted == null)
        //			{
        //				throw new ArgumentNullException("onCompleted");
        //			}
        //			return s_impl.SelectMany(source, onNext, onError, onCompleted);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.SelectMany(source, selector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (selector == null)
        //			{
        //				throw new ArgumentNullException("selector");
        //			}
        //			return s_impl.SelectMany(source, selector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (collectionSelector == null)
        //			{
        //				throw new ArgumentNullException("collectionSelector");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.SelectMany(source, collectionSelector, resultSelector);
        //		}

        //		public static IObservable<TResult> SelectMany<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (collectionSelector == null)
        //			{
        //				throw new ArgumentNullException("collectionSelector");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			return s_impl.SelectMany(source, collectionSelector, resultSelector);
        //		}

        //		public static IObservable<TSource> Skip<TSource>(this IObservable<TSource> source, int count)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (count < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			return s_impl.Skip(source, count);
        //		}

        //		public static IObservable<TSource> SkipWhile<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (predicate == null)
        //			{
        //				throw new ArgumentNullException("predicate");
        //			}
        //			return s_impl.SkipWhile(source, predicate);
        //		}

        //		public static IObservable<TSource> SkipWhile<TSource>(this IObservable<TSource> source, Func<TSource, int, bool> predicate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (predicate == null)
        //			{
        //				throw new ArgumentNullException("predicate");
        //			}
        //			return s_impl.SkipWhile(source, predicate);
        //		}

        //		public static IObservable<TSource> Take<TSource>(this IObservable<TSource> source, int count)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (count < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			return s_impl.Take(source, count);
        //		}

        //		public static IObservable<TSource> Take<TSource>(this IObservable<TSource> source, int count, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (count < 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Take(source, count, scheduler);
        //		}

        //		public static IObservable<TSource> TakeWhile<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (predicate == null)
        //			{
        //				throw new ArgumentNullException("predicate");
        //			}
        //			return s_impl.TakeWhile(source, predicate);
        //		}

        //		public static IObservable<TSource> TakeWhile<TSource>(this IObservable<TSource> source, Func<TSource, int, bool> predicate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (predicate == null)
        //			{
        //				throw new ArgumentNullException("predicate");
        //			}
        //			return s_impl.TakeWhile(source, predicate);
        //		}

        //		public static IObservable<TSource> Where<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (predicate == null)
        //			{
        //				throw new ArgumentNullException("predicate");
        //			}
        //			return s_impl.Where(source, predicate);
        //		}

        //		public static IObservable<TSource> Where<TSource>(this IObservable<TSource> source, Func<TSource, int, bool> predicate)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (predicate == null)
        //			{
        //				throw new ArgumentNullException("predicate");
        //			}
        //			return s_impl.Where(source, predicate);
        //		}

        //		public static IObservable<IList<TSource>> Buffer<TSource>(this IObservable<TSource> source, TimeSpan timeSpan)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeSpan < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeSpan");
        //			}
        //			return s_impl.Buffer(source, timeSpan);
        //		}

        //		public static IObservable<IList<TSource>> Buffer<TSource>(this IObservable<TSource> source, TimeSpan timeSpan, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeSpan < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeSpan");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Buffer(source, timeSpan, scheduler);
        //		}

        //		public static IObservable<IList<TSource>> Buffer<TSource>(this IObservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeSpan < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeSpan");
        //			}
        //			if (timeShift < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeShift");
        //			}
        //			return s_impl.Buffer(source, timeSpan, timeShift);
        //		}

        //		public static IObservable<IList<TSource>> Buffer<TSource>(this IObservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeSpan < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeSpan");
        //			}
        //			if (timeShift < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeShift");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Buffer(source, timeSpan, timeShift, scheduler);
        //		}

        //		public static IObservable<IList<TSource>> Buffer<TSource>(this IObservable<TSource> source, TimeSpan timeSpan, int count)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeSpan < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeSpan");
        //			}
        //			if (count <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			return s_impl.Buffer(source, timeSpan, count);
        //		}

        //		public static IObservable<IList<TSource>> Buffer<TSource>(this IObservable<TSource> source, TimeSpan timeSpan, int count, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeSpan < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeSpan");
        //			}
        //			if (count <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Buffer(source, timeSpan, count, scheduler);
        //		}

        //		public static IObservable<TSource> Delay<TSource>(this IObservable<TSource> source, TimeSpan dueTime)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (dueTime < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("dueTime");
        //			}
        //			return s_impl.Delay(source, dueTime);
        //		}

        //		public static IObservable<TSource> Delay<TSource>(this IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (dueTime < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("dueTime");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Delay(source, dueTime, scheduler);
        //		}

        //		public static IObservable<TSource> Delay<TSource>(this IObservable<TSource> source, DateTimeOffset dueTime)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Delay(source, dueTime);
        //		}

        //		public static IObservable<TSource> Delay<TSource>(this IObservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Delay(source, dueTime, scheduler);
        //		}

        //		public static IObservable<TSource> Delay<TSource, TDelay>(this IObservable<TSource> source, Func<TSource, IObservable<TDelay>> delayDurationSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (delayDurationSelector == null)
        //			{
        //				throw new ArgumentNullException("delayDurationSelector");
        //			}
        //			return s_impl.Delay(source, delayDurationSelector);
        //		}

        //		public static IObservable<TSource> Delay<TSource, TDelay>(this IObservable<TSource> source, IObservable<TDelay> subscriptionDelay, Func<TSource, IObservable<TDelay>> delayDurationSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (subscriptionDelay == null)
        //			{
        //				throw new ArgumentNullException("subscriptionDelay");
        //			}
        //			if (delayDurationSelector == null)
        //			{
        //				throw new ArgumentNullException("delayDurationSelector");
        //			}
        //			return s_impl.Delay(source, subscriptionDelay, delayDurationSelector);
        //		}

        //		public static IObservable<TSource> DelaySubscription<TSource>(this IObservable<TSource> source, TimeSpan dueTime)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (dueTime < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("dueTime");
        //			}
        //			return s_impl.DelaySubscription(source, dueTime);
        //		}

        //		public static IObservable<TSource> DelaySubscription<TSource>(this IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (dueTime < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("dueTime");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.DelaySubscription(source, dueTime, scheduler);
        //		}

        //		public static IObservable<TSource> DelaySubscription<TSource>(this IObservable<TSource> source, DateTimeOffset dueTime)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.DelaySubscription(source, dueTime);
        //		}

        //		public static IObservable<TSource> DelaySubscription<TSource>(this IObservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.DelaySubscription(source, dueTime, scheduler);
        //		}

        //		public static IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, TimeSpan> timeSelector)
        //		{
        //			if (condition == null)
        //			{
        //				throw new ArgumentNullException("condition");
        //			}
        //			if (iterate == null)
        //			{
        //				throw new ArgumentNullException("iterate");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			if (timeSelector == null)
        //			{
        //				throw new ArgumentNullException("timeSelector");
        //			}
        //			return s_impl.Generate(initialState, condition, iterate, resultSelector, timeSelector);
        //		}

        //		public static IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, TimeSpan> timeSelector, IScheduler scheduler)
        //		{
        //			if (condition == null)
        //			{
        //				throw new ArgumentNullException("condition");
        //			}
        //			if (iterate == null)
        //			{
        //				throw new ArgumentNullException("iterate");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			if (timeSelector == null)
        //			{
        //				throw new ArgumentNullException("timeSelector");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Generate(initialState, condition, iterate, resultSelector, timeSelector, scheduler);
        //		}

        //		public static IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, DateTimeOffset> timeSelector)
        //		{
        //			if (condition == null)
        //			{
        //				throw new ArgumentNullException("condition");
        //			}
        //			if (iterate == null)
        //			{
        //				throw new ArgumentNullException("iterate");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			if (timeSelector == null)
        //			{
        //				throw new ArgumentNullException("timeSelector");
        //			}
        //			return s_impl.Generate(initialState, condition, iterate, resultSelector, timeSelector);
        //		}

        //		public static IObservable<TResult> Generate<TState, TResult>(TState initialState, Func<TState, bool> condition, Func<TState, TState> iterate, Func<TState, TResult> resultSelector, Func<TState, DateTimeOffset> timeSelector, IScheduler scheduler)
        //		{
        //			if (condition == null)
        //			{
        //				throw new ArgumentNullException("condition");
        //			}
        //			if (iterate == null)
        //			{
        //				throw new ArgumentNullException("iterate");
        //			}
        //			if (resultSelector == null)
        //			{
        //				throw new ArgumentNullException("resultSelector");
        //			}
        //			if (timeSelector == null)
        //			{
        //				throw new ArgumentNullException("timeSelector");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Generate(initialState, condition, iterate, resultSelector, timeSelector, scheduler);
        //		}

        //		public static IObservable<long> Interval(TimeSpan period)
        //		{
        //			if (period < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("period");
        //			}
        //			return s_impl.Interval(period);
        //		}

        //		public static IObservable<long> Interval(TimeSpan period, IScheduler scheduler)
        //		{
        //			if (period < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("period");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Interval(period, scheduler);
        //		}

        //		public static IObservable<TSource> Sample<TSource>(this IObservable<TSource> source, TimeSpan interval)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (interval < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("interval");
        //			}
        //			return s_impl.Sample(source, interval);
        //		}

        //		public static IObservable<TSource> Sample<TSource>(this IObservable<TSource> source, TimeSpan interval, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (interval < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("interval");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Sample(source, interval, scheduler);
        //		}

        //		public static IObservable<TSource> Sample<TSource, TSample>(this IObservable<TSource> source, IObservable<TSample> sampler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (sampler == null)
        //			{
        //				throw new ArgumentNullException("sampler");
        //			}
        //			return s_impl.Sample(source, sampler);
        //		}

        //		public static IObservable<TSource> Skip<TSource>(this IObservable<TSource> source, TimeSpan duration)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (duration < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("duration");
        //			}
        //			return s_impl.Skip(source, duration);
        //		}

        //		public static IObservable<TSource> Skip<TSource>(this IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (duration < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("duration");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Skip(source, duration, scheduler);
        //		}

        //		public static IObservable<TSource> SkipLast<TSource>(this IObservable<TSource> source, TimeSpan duration)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (duration < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("duration");
        //			}
        //			return s_impl.SkipLast(source, duration);
        //		}

        //		public static IObservable<TSource> SkipLast<TSource>(this IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (duration < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("duration");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.SkipLast(source, duration, scheduler);
        //		}

        //		public static IObservable<TSource> SkipUntil<TSource>(this IObservable<TSource> source, DateTimeOffset startTime)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.SkipUntil(source, startTime);
        //		}

        //		public static IObservable<TSource> SkipUntil<TSource>(this IObservable<TSource> source, DateTimeOffset startTime, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.SkipUntil(source, startTime, scheduler);
        //		}

        //		public static IObservable<TSource> Take<TSource>(this IObservable<TSource> source, TimeSpan duration)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (duration < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("duration");
        //			}
        //			return s_impl.Take(source, duration);
        //		}

        //		public static IObservable<TSource> Take<TSource>(this IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (duration < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("duration");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Take(source, duration, scheduler);
        //		}

        //		public static IObservable<TSource> TakeLast<TSource>(this IObservable<TSource> source, TimeSpan duration)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (duration < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("duration");
        //			}
        //			return s_impl.TakeLast(source, duration);
        //		}

        //		public static IObservable<TSource> TakeLast<TSource>(this IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (duration < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("duration");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.TakeLast(source, duration, scheduler);
        //		}

        //		public static IObservable<TSource> TakeLast<TSource>(this IObservable<TSource> source, TimeSpan duration, IScheduler timerScheduler, IScheduler loopScheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (duration < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("duration");
        //			}
        //			if (timerScheduler == null)
        //			{
        //				throw new ArgumentNullException("timerScheduler");
        //			}
        //			if (loopScheduler == null)
        //			{
        //				throw new ArgumentNullException("loopScheduler");
        //			}
        //			return s_impl.TakeLast(source, duration, timerScheduler, loopScheduler);
        //		}

        //		public static IObservable<IList<TSource>> TakeLastBuffer<TSource>(this IObservable<TSource> source, TimeSpan duration)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (duration < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("duration");
        //			}
        //			return s_impl.TakeLastBuffer(source, duration);
        //		}

        //		public static IObservable<IList<TSource>> TakeLastBuffer<TSource>(this IObservable<TSource> source, TimeSpan duration, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (duration < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("duration");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.TakeLastBuffer(source, duration, scheduler);
        //		}

        //		public static IObservable<TSource> TakeUntil<TSource>(this IObservable<TSource> source, DateTimeOffset endTime)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.TakeUntil(source, endTime);
        //		}

        //		public static IObservable<TSource> TakeUntil<TSource>(this IObservable<TSource> source, DateTimeOffset endTime, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.TakeUntil(source, endTime, scheduler);
        //		}

        //		public static IObservable<TSource> Throttle<TSource>(this IObservable<TSource> source, TimeSpan dueTime)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (dueTime < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("dueTime");
        //			}
        //			return s_impl.Throttle(source, dueTime);
        //		}

        //		public static IObservable<TSource> Throttle<TSource>(this IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (dueTime < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("dueTime");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Throttle(source, dueTime, scheduler);
        //		}

        //		public static IObservable<TSource> Throttle<TSource, TThrottle>(this IObservable<TSource> source, Func<TSource, IObservable<TThrottle>> throttleDurationSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (throttleDurationSelector == null)
        //			{
        //				throw new ArgumentNullException("throttleDurationSelector");
        //			}
        //			return s_impl.Throttle(source, throttleDurationSelector);
        //		}

        //		public static IObservable<TimeInterval<TSource>> TimeInterval<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.TimeInterval(source);
        //		}

        //		public static IObservable<TimeInterval<TSource>> TimeInterval<TSource>(this IObservable<TSource> source, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.TimeInterval(source, scheduler);
        //		}

        //		public static IObservable<TSource> Timeout<TSource>(this IObservable<TSource> source, TimeSpan dueTime)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (dueTime < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("dueTime");
        //			}
        //			return s_impl.Timeout(source, dueTime);
        //		}

        //		public static IObservable<TSource> Timeout<TSource>(this IObservable<TSource> source, TimeSpan dueTime, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (dueTime < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("dueTime");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Timeout(source, dueTime, scheduler);
        //		}

        //		public static IObservable<TSource> Timeout<TSource>(this IObservable<TSource> source, TimeSpan dueTime, IObservable<TSource> other)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (dueTime < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("dueTime");
        //			}
        //			if (other == null)
        //			{
        //				throw new ArgumentNullException("other");
        //			}
        //			return s_impl.Timeout(source, dueTime, other);
        //		}

        //		public static IObservable<TSource> Timeout<TSource>(this IObservable<TSource> source, TimeSpan dueTime, IObservable<TSource> other, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (dueTime < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("dueTime");
        //			}
        //			if (other == null)
        //			{
        //				throw new ArgumentNullException("other");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Timeout(source, dueTime, other, scheduler);
        //		}

        //		public static IObservable<TSource> Timeout<TSource>(this IObservable<TSource> source, DateTimeOffset dueTime)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Timeout(source, dueTime);
        //		}

        //		public static IObservable<TSource> Timeout<TSource>(this IObservable<TSource> source, DateTimeOffset dueTime, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Timeout(source, dueTime, scheduler);
        //		}

        //		public static IObservable<TSource> Timeout<TSource>(this IObservable<TSource> source, DateTimeOffset dueTime, IObservable<TSource> other)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (other == null)
        //			{
        //				throw new ArgumentNullException("other");
        //			}
        //			return s_impl.Timeout(source, dueTime, other);
        //		}

        //		public static IObservable<TSource> Timeout<TSource>(this IObservable<TSource> source, DateTimeOffset dueTime, IObservable<TSource> other, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			if (other == null)
        //			{
        //				throw new ArgumentNullException("other");
        //			}
        //			return s_impl.Timeout(source, dueTime, other, scheduler);
        //		}

        //		public static IObservable<TSource> Timeout<TSource, TTimeout>(this IObservable<TSource> source, Func<TSource, IObservable<TTimeout>> timeoutDurationSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeoutDurationSelector == null)
        //			{
        //				throw new ArgumentNullException("timeoutDurationSelector");
        //			}
        //			return s_impl.Timeout(source, timeoutDurationSelector);
        //		}

        //		public static IObservable<TSource> Timeout<TSource, TTimeout>(this IObservable<TSource> source, Func<TSource, IObservable<TTimeout>> timeoutDurationSelector, IObservable<TSource> other)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeoutDurationSelector == null)
        //			{
        //				throw new ArgumentNullException("timeoutDurationSelector");
        //			}
        //			if (other == null)
        //			{
        //				throw new ArgumentNullException("other");
        //			}
        //			return s_impl.Timeout(source, timeoutDurationSelector, other);
        //		}

        //		public static IObservable<TSource> Timeout<TSource, TTimeout>(this IObservable<TSource> source, IObservable<TTimeout> firstTimeout, Func<TSource, IObservable<TTimeout>> timeoutDurationSelector)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (firstTimeout == null)
        //			{
        //				throw new ArgumentNullException("firstTimeout");
        //			}
        //			if (timeoutDurationSelector == null)
        //			{
        //				throw new ArgumentNullException("timeoutDurationSelector");
        //			}
        //			return s_impl.Timeout(source, firstTimeout, timeoutDurationSelector);
        //		}

        //		public static IObservable<TSource> Timeout<TSource, TTimeout>(this IObservable<TSource> source, IObservable<TTimeout> firstTimeout, Func<TSource, IObservable<TTimeout>> timeoutDurationSelector, IObservable<TSource> other)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (firstTimeout == null)
        //			{
        //				throw new ArgumentNullException("firstTimeout");
        //			}
        //			if (timeoutDurationSelector == null)
        //			{
        //				throw new ArgumentNullException("timeoutDurationSelector");
        //			}
        //			if (other == null)
        //			{
        //				throw new ArgumentNullException("other");
        //			}
        //			return s_impl.Timeout(source, firstTimeout, timeoutDurationSelector, other);
        //		}

        //		public static IObservable<long> Timer(TimeSpan dueTime)
        //		{
        //			return s_impl.Timer(dueTime);
        //		}

        //		public static IObservable<long> Timer(DateTimeOffset dueTime)
        //		{
        //			return s_impl.Timer(dueTime);
        //		}

        //		public static IObservable<long> Timer(TimeSpan dueTime, TimeSpan period)
        //		{
        //			if (period < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("period");
        //			}
        //			return s_impl.Timer(dueTime, period);
        //		}

        //		public static IObservable<long> Timer(DateTimeOffset dueTime, TimeSpan period)
        //		{
        //			if (period < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("period");
        //			}
        //			return s_impl.Timer(dueTime, period);
        //		}

        //		public static IObservable<long> Timer(TimeSpan dueTime, IScheduler scheduler)
        //		{
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Timer(dueTime, scheduler);
        //		}

        //		public static IObservable<long> Timer(DateTimeOffset dueTime, IScheduler scheduler)
        //		{
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Timer(dueTime, scheduler);
        //		}

        //		public static IObservable<long> Timer(TimeSpan dueTime, TimeSpan period, IScheduler scheduler)
        //		{
        //			if (period < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("period");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Timer(dueTime, period, scheduler);
        //		}

        //		public static IObservable<long> Timer(DateTimeOffset dueTime, TimeSpan period, IScheduler scheduler)
        //		{
        //			if (period < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("period");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Timer(dueTime, period, scheduler);
        //		}

        //		public static IObservable<Timestamped<TSource>> Timestamp<TSource>(this IObservable<TSource> source)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			return s_impl.Timestamp(source);
        //		}

        //		public static IObservable<Timestamped<TSource>> Timestamp<TSource>(this IObservable<TSource> source, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Timestamp(source, scheduler);
        //		}

        //		public static IObservable<IObservable<TSource>> Window<TSource>(this IObservable<TSource> source, TimeSpan timeSpan)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeSpan < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeSpan");
        //			}
        //			return s_impl.Window(source, timeSpan);
        //		}

        //		public static IObservable<IObservable<TSource>> Window<TSource>(this IObservable<TSource> source, TimeSpan timeSpan, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeSpan < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeSpan");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Window(source, timeSpan, scheduler);
        //		}

        //		public static IObservable<IObservable<TSource>> Window<TSource>(this IObservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeSpan < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeSpan");
        //			}
        //			if (timeShift < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeShift");
        //			}
        //			return s_impl.Window(source, timeSpan, timeShift);
        //		}

        //		public static IObservable<IObservable<TSource>> Window<TSource>(this IObservable<TSource> source, TimeSpan timeSpan, TimeSpan timeShift, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeSpan < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeSpan");
        //			}
        //			if (timeShift < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeShift");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Window(source, timeSpan, timeShift, scheduler);
        //		}

        //		public static IObservable<IObservable<TSource>> Window<TSource>(this IObservable<TSource> source, TimeSpan timeSpan, int count)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeSpan < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeSpan");
        //			}
        //			if (count <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			return s_impl.Window(source, timeSpan, count);
        //		}

        //		public static IObservable<IObservable<TSource>> Window<TSource>(this IObservable<TSource> source, TimeSpan timeSpan, int count, IScheduler scheduler)
        //		{
        //			if (source == null)
        //			{
        //				throw new ArgumentNullException("source");
        //			}
        //			if (timeSpan < TimeSpan.Zero)
        //			{
        //				throw new ArgumentOutOfRangeException("timeSpan");
        //			}
        //			if (count <= 0)
        //			{
        //				throw new ArgumentOutOfRangeException("count");
        //			}
        //			if (scheduler == null)
        //			{
        //				throw new ArgumentNullException("scheduler");
        //			}
        //			return s_impl.Window(source, timeSpan, count, scheduler);
        //		}
    }
}
