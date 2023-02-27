using System.Diagnostics;

namespace SimpleProjectManager.Operation.Client.Device.MGI.ProcessManager
{
    public sealed class ProcessStateChange
    {
        public ProcessStateChange(ProcessChange change, string name, int id, Process process)
        {
            Change = change;
            Name = name;
            Id = id;
            Process = process;
        }

        public ProcessChange Change { get; }

        public string Name { get; }

        public int Id { get; }

        public Process Process { get; }
    }
}