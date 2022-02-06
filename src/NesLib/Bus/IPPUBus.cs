using NesLib.Cartridge;
using NesLib.Common;
using NesLib.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.Bus
{
    interface IPPUBus : IReadWrite8Bit, IReadWrite16Bit
    {
        void ConnectVRAM(IRAM ram);
        void ConnectCartridge(ICartridge cartridge);
    }
}
