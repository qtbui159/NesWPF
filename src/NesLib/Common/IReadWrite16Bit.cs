using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.Common
{
    public interface IReadWrite16Bit : IReadWrite8Bit
    {
        void WriteWord(ushort addr, ushort data);
        ushort ReadWord(ushort addr);
    }
}
