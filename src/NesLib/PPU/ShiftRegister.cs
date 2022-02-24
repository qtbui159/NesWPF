using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.PPU
{
    class ShiftRegister
    {
        public ushort TileHighByte { get; set; }
        public ushort TileLowByte { get; set; }
        public ushort AttributeLowByte { get; set; }
        public ushort AttributeHighByte { get; set; }
    }
}
