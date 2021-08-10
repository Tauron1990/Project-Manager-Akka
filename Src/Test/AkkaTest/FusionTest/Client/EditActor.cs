using Akka.Actor;
using AkkaTest.FusionTest.Data;

namespace AkkaTest.FusionTest.Client
{
    public class EditActor : ReceiveActor
    {
        private readonly IClaimManager _manager;

        public EditActor(IClaimManager manager)
        {
            _manager = manager;
        }
    }
}