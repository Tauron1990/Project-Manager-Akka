namespace ServiceHost.Services.Impl
{
    public sealed record InternalRestartApp
    {
        public static readonly InternalRestartApp Inst = new();
    }
}