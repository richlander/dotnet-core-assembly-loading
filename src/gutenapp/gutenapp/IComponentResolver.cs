using System.IO;
using ComponentHost;

namespace ComponentHost
{
    public interface IComponentResolver
    {
        ComponentResolution FindLibrary(string library);
        bool SetComponent(string component);

    }    
}
