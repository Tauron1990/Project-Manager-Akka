using System;
using Avalonia;
using Avalonia.Data;
using JetBrains.Annotations;
using Tauron.Application.Avalonia.AppCore;
using Tauron.Application.CommonUI.Helper;
using Tauron.Application.CommonUI.UI;

namespace Tauron.Application.Avalonia.UI
{
    [PublicAPI]
    public class ActorBinding : UpdatableMarkupExtension
    {
        private readonly string _name;

        public ActorBinding(string name) => _name = name;

        public string? Path { get; set; }

        protected override object ProvideValueInternal(IServiceProvider serviceProvider)
        {
            try
            {
                if (!TryGetTargetItems(serviceProvider, out var target, out _))
                    return AvaloniaProperty.UnsetValue;

                if (!ControlBindLogic.FindDataContext(ElementMapper.Create(target).AsOption(), out var model))
                    return AvaloniaProperty.UnsetValue;

                var binding = new Binding();
                if (string.IsNullOrWhiteSpace(Path))
                    binding.Path = "Value";
                else
                    binding.Path = "Value." + Path;

                binding.Source = new DeferredSource(_name, model);

                return binding;
            }
            catch (NullReferenceException)
            {
                return AvaloniaProperty.UnsetValue;
            }
        }
    }
}