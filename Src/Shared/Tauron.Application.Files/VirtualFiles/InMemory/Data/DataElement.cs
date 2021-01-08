namespace Tauron.Application.Files.VirtualFiles.InMemory.Data
{
    public abstract class DataElement
    {
        protected DataElement(string name) => Name = name;

        public string Name { get; set; }
    }
}