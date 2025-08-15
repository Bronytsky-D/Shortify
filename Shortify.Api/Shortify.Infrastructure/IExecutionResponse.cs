using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shortify.Infrastructure
{
    public interface IExecutionResponse
    {
        bool Success { get; }
        IEnumerable<string> Errors { get; }
        object Result { get; }
    }
}
