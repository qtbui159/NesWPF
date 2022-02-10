using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.IO
{
    class IORegister : IIORegister
    {
        private const int SIZE = 0x4020 - 0x2000;
        private readonly byte[] m_Data = new byte[SIZE];

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
            if (addr >= 0x4000 && addr < 0x4020)
            {
                return (ushort)(addr - 0x2000);
            }
            else if (addr >= 0x2000 && addr < 0x4000)
            {
                addr &= 0x2000;
                return (ushort)(addr - 0x2000);
            }

            throw new Exception("未知地址");
        }
    }
}
