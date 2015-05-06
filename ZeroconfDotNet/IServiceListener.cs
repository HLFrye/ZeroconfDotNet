using System;
namespace ZeroconfDotNet
{
    public interface IServiceListener
    {
        void Dispose();
        event FindServicesDelegate FindServices;
        void Start();
        void Stop();
    }
}
