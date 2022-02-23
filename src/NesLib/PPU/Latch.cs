using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.PPU
{
    class Latch
    {
        public ushort NameTableTileIndex { get; set; }
        public byte PaletteHigh { get; set; }

        public byte BackgroundTileLowByte { get; set; }
        public byte BackgroundTileHighByte { get; set; }
    }
}
