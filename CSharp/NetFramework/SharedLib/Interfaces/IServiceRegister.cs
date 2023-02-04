using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLib.Interfaces
{
    public interface IServiceRegister
    {
        void RegisterService(IServiceCollection serviceCollection);
    }
}
