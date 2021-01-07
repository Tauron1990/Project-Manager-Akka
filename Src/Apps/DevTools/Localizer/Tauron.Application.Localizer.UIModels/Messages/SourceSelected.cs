using System;
using Tauron.Application.Localizer.UIModels.Views;

namespace Tauron.Application.Localizer.UIModels.Messages
{
    public sealed record SourceSelected(string? Source, OpenFileMode Mode)
    {
        public static SourceSelected From(string? data, OpenFileMode mode) 
            => new(data, mode);
    }
}