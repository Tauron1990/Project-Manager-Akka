namespace SimpleProjectManager.Operation.Client.Device.MGI.UVLamp
{
    public sealed class MgiOptions
    {
        public int ClockTimeMs { get; set; } = 1000;

        //set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        public string Ip { get; set; } = "192.168.187.48";
    }
}