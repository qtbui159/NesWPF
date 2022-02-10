using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * 参考资料
 * 1*)
 */

namespace NesLib.Memory
{
    /// <summary>
    /// 独立显存
    /// PPU总线上0x2000---0x3EFF的地址
    /// </summary>
    class VRAM : IRAM
    {
        private byte[] m_Data;

        public VRAM()
        {
            m_Data = new byte[0x1000];
        }

        public byte ReadByte(ushort addr)
        {
            addr = GetRealAddr(addr);
            return m_Data[addr];
        }

        public void WriteByte(ushort addr, byte data)
        {
            addr = GetRealAddr(addr);
            m_Data[addr] = data;
        }

        private ushort GetRealAddr(ushort addr)
        {
            if (addr >= 0x3000 && addr <= 0x3EFF)
            {
                //$3000-3EFF is usually a mirror of the 2kB region from $2000-2EFF. The PPU does not render from this address range, so this space has negligible utility.
                addr -= 0x1000;
            }
            return (ushort)(addr - 0x2000);
        }
    }
}
