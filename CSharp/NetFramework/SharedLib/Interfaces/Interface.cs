using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLib.Interfaces
{
    public interface INeededParameter<T>
    {
        void SetParameter(T parameter);
    }
}
