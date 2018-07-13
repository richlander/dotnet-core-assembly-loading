using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace interfaces
{
    public interface IProvider
    {
        Task<int> ProcessTextAsync(Task<string> text);
        Dictionary<string,object> GetReport();
    }
}
