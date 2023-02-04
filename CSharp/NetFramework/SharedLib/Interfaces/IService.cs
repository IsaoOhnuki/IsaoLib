using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLib.Interfaces
{
    public interface IService
    {
        void Startup(IServiceCollection services);
        void Finish(IServiceCollection services);
    }
}
