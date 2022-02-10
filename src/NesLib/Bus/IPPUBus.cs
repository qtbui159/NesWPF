using NesLib.Cartridge;
using NesLib.Common;
using NesLib.Memory;
using NesLib.PPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.Bus
{
    interface IPPUBus : IReadWrite8Bit
    {
        void ConnectVRAM(IRAM ram);
        void ConnectCartridge(ICartridge cartridge);
        void ConnectPalette(IPalette palette);
    }
}
