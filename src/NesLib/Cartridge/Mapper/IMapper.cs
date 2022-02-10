using NesLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.Cartridge.Mapper
{
    public interface IMapper : IReadWrite8Bit
    {
        int Version { get; }
    }
}
