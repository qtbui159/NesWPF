using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.Cartridge.Mapper
{
    /// <summary>
    /// 最原始版本
    /// https://wiki.nesdev.org/w/index.php/NROM
    /// </summary>
    class Mapper000 : IMapper
    {
        /// <summary>
        /// 位于地址0x6000-0x7FFF，大小0x2000
        /// </summary>
        private readonly byte[] m_PRGRam;
        private readonly byte[] m_PRGRom;
        private readonly byte[] m_CHRRom;

        public int Version => 0;

        public Mapper000(byte[] prgRam, byte[] prgRom, byte[] chrRom)
        {
            m_PRGRam = prgRam;
            m_PRGRom = prgRom;
            m_CHRRom = chrRom;
        }

        public byte ReadByte(ushort addr)
        {
            if (addr < 0x2000)
            {
                return m_CHRRom[addr];
            }
            if (addr >= 0x6000 && addr <= 0x7FFF)
            {
                addr = GetPRGRamRealAddr(addr);
                return m_PRGRam[addr];
            }
            else if (addr >= 0x8000 && addr <= 0xFFFF)
            {
                addr = GetPRGRomRealAddr(addr);
                return m_PRGRom[addr];
            }
            else
            {
                throw new Exception("未知地址");
            }
        }

        public void WriteByte(ushort addr, byte data)
        {
            //目前暂不支持写ram
            //rom不可写，所以啥都不用管
        }

        private ushort GetPRGRomRealAddr(ushort addr)
        {
            const int NROM128 = 16 * 1024;
            const int NROM256 = 32 * 1024;

            if (m_PRGRom.Length == NROM128)
            {
                return (ushort)((addr & 0xBFFF) - 0x8000);
            }
            else if (m_PRGRom.Length == NROM256)
            {
                return (ushort)(addr - 0x8000);
            }
            throw new Exception("不支持的PRGRom大小");
        }

        private ushort GetPRGRamRealAddr(ushort addr)
        {
            return (ushort)(addr - 0x6000);
        }
    }
}
