using System;

namespace Tauron.Application.AkkaNode.Services.FileTransfer
{
    public interface ITransferData : IDisposable
    {
        #pragma warning disable EPS02
        int Read(byte[] buffer, in int offset, in int count);
        void Write(byte[] buffer, in int offset, in int count);
        #pragma warning restore EPS02
    }
}