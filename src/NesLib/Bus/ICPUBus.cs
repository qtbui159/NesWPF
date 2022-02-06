using NesLib.Cartridge;
using NesLib.Common;
using NesLib.IO;
using NesLib.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.Bus
{
    interface ICPUBus : IReadWrite8Bit, IReadWrite16Bit
    {
        void ConnectRAM(IRAM ram);
        void ConnectCartridge(ICartridge cartidge);
        void ConnectIORegister(IIORegister io);
    }
}
