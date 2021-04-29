using SharpRepository.Repository.Configuration;

namespace AkkaTest.JsonRepo
{
    public class JsonRepositoryConfiguration : RepositoryConfiguration
    {
        public JsonRepositoryConfiguration(string name, string directory)
        {
            Name = name;
            Directory = directory;
            Factory = typeof (JsonConfigRepositoryFactory);
        }

        public string Directory
        {
            set { Attributes["directory"] = value; }
        }
    }
}
