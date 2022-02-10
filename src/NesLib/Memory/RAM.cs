using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.Memory
{
    class RAM : IRAM
    {
        private const int RAM_SIZE = 0x800;
        private readonly byte[] m_Data;

        public RAM()
        {
            m_Data = new byte[RAM_SIZE];
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
            //参考资料，https://wiki.nesdev.org/w/index.php?title=CPU_memory_map
            //因为3次镜像，所以这里需要取真实的地址
            return (ushort)(addr & 0x7FF);
        }
    }
}
