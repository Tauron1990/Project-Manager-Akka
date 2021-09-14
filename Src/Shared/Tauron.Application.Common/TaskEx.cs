using System.Threading.Tasks;

namespace Tauron
{
    public static class TaskEx
    {
        public static void Ignore(this Task _)
        {
            
        }
        
        public static void Ignore<T>(this Task<T> _)
        {
            
        }
    }
}