using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.Memory
{
    class VRAM : IRAM
    {
        public byte ReadByte(ushort addr)
        {
            throw new NotImplementedException();
        }

        public void WriteByte(ushort addr, byte data)
        {
            throw new NotImplementedException();
        }
    }
}
