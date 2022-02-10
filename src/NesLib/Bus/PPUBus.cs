using NesLib.Cartridge;
using NesLib.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.Bus
{
    class PPUBus : IPPUBus
    {
        private IRAM m_VRAM;
        private ICartridge m_Cartridge;

        public void ConnectVRAM(IRAM ram)
        {
            m_VRAM = ram;
        }

        public void ConnectCartridge(ICartridge cartridge)
        {
            m_Cartridge = cartridge;
        }

        public byte ReadByte(ushort addr)
        {
            addr = GetRealAddr(addr);
            if (addr < 0x2000)
            {
                return m_Cartridge.CHRRom[addr];
            }
            else if (addr >= 0x2000 && addr < 0x3F00)
            {
                
            }
            else if (addr >= 0x3F00 && addr < 0x4000)
            {
            }
            else 
            {
                throw new Exception("未知地址");
            }

            return 0;
        }

        public void WriteByte(ushort addr, byte data)
        {
            addr = GetRealAddr(addr);
        }

        private ushort GetRealAddr(ushort addr)
        {
            return (ushort)(addr & 0x3FFF);
        }
    }
}
