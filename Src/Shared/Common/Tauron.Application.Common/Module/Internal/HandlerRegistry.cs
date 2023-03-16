namespace Tauron.Module.Internal;

public static class HandlerRegistry
{
    public static IModuleHandler ModuleHandler { get; set; } = new DefaultModuleHandler();
}