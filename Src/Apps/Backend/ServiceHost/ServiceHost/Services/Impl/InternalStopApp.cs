namespace ServiceHost.Services.Impl
{
    public record InternalStopApp(bool Restart)
    {
        public InternalStopApp()
            : this(Restart: false)
        {
            
        }
    }
}