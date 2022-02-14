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
            ushort readlAddr = GetRealAddr(addr);
            if (readlAddr < 0x2000)
            {
                //这里实现暂时有问题，应该用Mapper去读取，因为可能包含镜像
                return m_Cartridge.CHRRom[readlAddr];
            }
            else if (readlAddr >= 0x2000 && readlAddr < 0x3F00)
            {
                return m_VRAM.ReadByte(readlAddr);   
            }
            else if (readlAddr >= 0x3F00 && readlAddr < 0x4000)
            {
                return m_Palette.ReadByte(readlAddr);
            }
            else
            {
                throw new Exception("未知地址");
            }
        }

        public void WriteByte(ushort addr, byte data)
        {
            ushort readAddr = GetRealAddr(addr);

            if (readAddr < 0x2000)
            {
                //throw new Exception("该地址不支持写入操作");
                m_Cartridge.CHRRom[readAddr] = data;
            }
            else if (readAddr >= 0x2000 && readAddr < 0x3F00)
            {
                m_VRAM.WriteByte(readAddr, data);
            }
            else if (readAddr >= 0x3F00 && readAddr < 0x4000)
            {
                m_Palette.WriteByte(readAddr, data);
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
