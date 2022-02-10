using NesLib.Cartridge;
using NesLib.Memory;
using NesLib.PPU;
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
        private IPalette m_Palette;

        public void ConnectVRAM(IRAM ram)
        {
            m_VRAM = ram;
        }

        public void ConnectCartridge(ICartridge cartridge)
        {
            m_Cartridge = cartridge;
        }

        public void ConnectPalette(IPalette palette)
        {
            m_Palette = palette;
        }

        public byte ReadByte(ushort addr)
        {
            addr = GetRealAddr(addr);
            if (addr < 0x2000)
            {
                //这里实现暂时有问题，应该用Mapper去读取，因为可能包含镜像
                return m_Cartridge.CHRRom[addr];
            }
            else if (addr >= 0x2000 && addr < 0x3F00)
            {
                return m_VRAM.ReadByte(addr);   
            }
            else if (addr >= 0x3F00 && addr < 0x4000)
            {
                return m_Palette.ReadByte(addr);
            }
            else
            {
                throw new Exception("未知地址");
            }
        }

        public void WriteByte(ushort addr, byte data)
        {
            addr = GetRealAddr(addr);

            if (addr < 0x2000)
            {
                throw new Exception("该地址不支持写入操作");
            }
            else if (addr >= 0x2000 && addr < 0x3F00)
            {
                m_VRAM.WriteByte(addr, data);
            }
            else if (addr >= 0x3F00 && addr < 0x4000)
            {
                m_Palette.WriteByte(addr, data);
            }
            else
            {
                throw new Exception("未知地址");
            }
        }

        private ushort GetRealAddr(ushort addr)
        {
            return (ushort)(addr & 0x3FFF);
        }
    }
}
