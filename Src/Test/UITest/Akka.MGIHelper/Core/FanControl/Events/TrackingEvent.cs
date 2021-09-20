namespace Akka.MGIHelper.Core.FanControl.Events
{
    public sealed record TrackingEvent(bool Error, string Reason, int Power = 0, State State = State.Error, double Pidout = 0, int PidSetValue = 0, int Pt1000 = 0)
    {
        public TrackingEvent(int power, State state, double pidout, int pidSetValue, int pt1000)
            : this(Error: false, string.Empty, power, state, pidout, pidSetValue, pt1000) { }
    }
}