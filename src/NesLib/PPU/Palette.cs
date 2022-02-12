using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * 参考资料
 * 1*)https://wiki.nesdev.org/w/index.php?title=PPU_palettes
 */

namespace NesLib.PPU
{
    /// <summary>
    /// 调色板索引
    /// PPU总线上0x3F00----0x3FFF
    /// </summary>
    class Palette : IPalette
    {
        private static readonly Dictionary<int, int> m_OffsetMapRGBA;
        private readonly byte[] m_Data;

        static Palette()
        {
            m_OffsetMapRGBA = new Dictionary<int, int>();
            byte[] data = File.ReadAllBytes(@"C:\Users\Spike\Desktop\ntscpalette.pal");
            for (int i = 0; i < data.Length / 3; ++i)
            {
                byte r = data[i*3];
                byte g = data[i*3 + 1];
                byte b = data[i*3 + 2];
                byte a = 0xFF;

                int value = (r << 24) | (g << 16) | (b << 8) | a;
                m_OffsetMapRGBA.Add(i, value);
            }
        }

        public Palette()
        {
            m_Data = new byte[0x20];
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
            addr = (ushort)(addr & 0x3F1F);
            //3F10,3F14,3F18,3F1C为3F00,3F04,3F08,3F0C的镜像
            if (addr == 0x3F10 || addr == 0x3F14 || addr == 0x3F18 || addr == 0x3F1C)
            {
                addr -= 0x10;
            }
            return (ushort)(addr - 0x3F00);
        }

        public static int GetRGBAColor(byte paletteOffset)
        {
            return m_OffsetMapRGBA[paletteOffset];
        }
    }
}
