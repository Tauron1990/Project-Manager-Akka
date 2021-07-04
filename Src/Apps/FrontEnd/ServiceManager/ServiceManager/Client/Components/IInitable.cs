using System.Threading.Tasks;

namespace ServiceManager.Client.Components
{
    public interface IInitable
    {
        Task Init();
    }
}