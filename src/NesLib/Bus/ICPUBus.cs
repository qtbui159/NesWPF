using NesLib.Cartridge;
using NesLib.Common;
using NesLib.JoyStick;
using NesLib.Memory;
using NesLib.PPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.Bus
{
    interface ICPUBus : IReadWrite8Bit
    {
        void ConnectRAM(IRAM ram);
        void ConnectCartridge(ICartridge cartidge);
        void ConnectPPU(IPPU2C02 ppu);
        void ConnectJoyStock(IJoyStick p1Joystick, IJoyStick p2Joystick);
    }
}
