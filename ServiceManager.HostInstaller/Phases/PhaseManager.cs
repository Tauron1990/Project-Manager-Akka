namespace ServiceManager.HostInstaller.Phases
{
    public sealed class PhaseManager<TContext>
    {
        private readonly Phase<TContext>[] _phases;

        private int Pos { get; set; }

        //public bool Completed => Pos == _phases.Length;

        public PhaseManager(params Phase<TContext>[] phases) => _phases = phases;

        public void RunNext(TContext context)
        {
            if(context is IHasTimeout {IsTimeedOut: true})
                return;

            var phase = _phases[Pos];
            Pos++;
            phase.Run(context, this);
        }
    }
}