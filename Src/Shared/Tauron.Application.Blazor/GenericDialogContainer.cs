using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MudBlazor;
using Tauron.Application.CommonUI.Dialogs;

namespace Tauron.Application.Blazor
{
    public abstract class GenericDialogContainerBase
    {
        protected internal abstract void Show(Func<Func<IDialogReference>, Task<IDialogReference>> invoker);
    }

    [PublicAPI]
    public class GenericDialogContainer<TResult, TInput, TComponent> : GenericDialogContainerBase, IBaseDialog<TResult, TInput>
        where TComponent : GenericDialogBase<TResult, TInput>
    {
        private readonly IDialogService _service;
        private readonly TaskCompletionSource<TResult> _completionSource = new();
        private readonly TaskCompletionSource<TInput> _inputSource = new();

        public GenericDialogContainer(IDialogService service) => _service = service;

        protected internal sealed override async void Show(Func<Func<IDialogReference>, Task<IDialogReference>> invoker)
        {
            try
            {
                var initalData = await _inputSource.Task;
                Parameters.Add("InitialData", initalData);

                var dialog = await invoker(() => _service.Show<TComponent>(Title, Parameters, Options));

                var dialogObject = await Task.Run(async () =>
                                                  {
                                                      while (dialog.Dialog == null)
                                                      {
                                                          await Task.Delay(500);
                                                      }

                                                      return dialog.Dialog;
                                                  });
                var task = await Task.WhenAny(((IBaseDialog<TResult, TInput>) dialogObject).Init(initalData), dialog.Result);

                switch (task)
                {
                    case Task<TResult> resultTask:
                        var result = await resultTask;
                        _completionSource.SetResult(result);
                        break;
                    case Task<DialogResult> dialogTask:
                        var dr = await dialogTask;
                        if (dr.Cancelled)
                            #pragma warning disable EX006
                            throw new TaskCanceledException(dialogTask);
                        #pragma warning restore EX006
                        _completionSource.SetCanceled();
                        break;
                    default:
                        #pragma warning disable EX006
                        throw new InvalidOperationException("Task Casting Failed");
                    #pragma warning restore EX006
                }
            }
            catch (Exception e)
            {
                _completionSource.TrySetException(e);
            }
            finally
            {
                if (!_completionSource.Task.IsCompleted)
                    _completionSource.SetCanceled();
            }

        }

        public Task<TResult> Init(TInput initalData)
        {
            _inputSource.TrySetResult(initalData);
            return _completionSource.Task;
        }

        protected virtual string Title { get; } = string.Empty;

        protected virtual DialogParameters Parameters { get; } = new();

        protected virtual DialogOptions Options { get; } = new()
                                                           {
                                                               CloseButton = false,
                                                               DisableBackdropClick = true
                                                           };
    }
}