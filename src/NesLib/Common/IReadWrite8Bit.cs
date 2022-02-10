using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.Common
{
    public interface IReadWrite8Bit
    {
        void WriteByte(ushort addr, byte data);
        byte ReadByte(ushort addr);
    }
}
