using NesLib.Bus;
using NesLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

//参考资料：
//1*)https://wiki.nesdev.org/w/index.php?title=PPU_power_up_state
//2*)https://wiki.nesdev.org/w/index.php?title=Mirroring#Nametable_Mirroring
//3*)https://wiki.nesdev.org/w/index.php?title=PPU_nametables
//4*)https://wiki.nesdev.org/w/index.php?title=PPU_attribute_tables
//5*)https://wiki.nesdev.org/w/index.php?title=PPU_pattern_tables
//6*)https://wiki.nesdev.org/w/index.php/PPU_registers#The_PPUDATA_read_buffer_.28post-fetch.29

namespace NesLib.PPU
{
    class PPU2C02 : IPPU2C02
    {
        private readonly IPPUBus m_PPUBus;

        private MirroringMode m_MirroringMode;

        public CTRLRegister CTRL { get; private set; }
        public MASKRegister MASK { get; private set; }
        public STATUSRegister STATUS { get; private set; }

        public ushort Addr { get; set; }

        public byte[] OAM => m_OAM;
        private byte[] m_OAM;

        public byte OAMAddr { get; set; }
        public bool WriteX2Flag { get; set; }
        public byte ReadBuffer { get; set; }

        public PPU2C02(IPPUBus ppuBus)
        {
            m_PPUBus = ppuBus;

            CTRL = new CTRLRegister();
            MASK = new MASKRegister();
            STATUS = new STATUSRegister();
            m_OAM = new byte[256];
            OAMAddr = 0;
            WriteX2Flag = false;
        }

        public void WriteByte(ushort addr, byte data)
        {
            m_PPUBus.WriteByte(addr, data);
        }

        public byte ReadByte(ushort addr)
        {
            byte data = m_PPUBus.ReadByte(addr);
            if (addr >= 0x3F00 && addr < 0x4000)
            {
                //根据参考资料6，实现更新ReadBuffer
                ReadBuffer = data;
            }

            return data;
        }

        public int GetBackgroundPixel(int x, int y)
        {
            //PPU将画面(256*240)分割为32*30的小块，一共960个小块，每1个小块为一个tile(瓦片)，每个tile包含8个像素信息
            //tile坐标系为横坐标0-31,纵坐标0-29,并且每4*4的tile为一个大tile,一共64个大tile
            //开头的一个大tile为左上角(0,0),右下角(3,3)
            //一个tile为8*8像素=>推断 256*240=61440像素=960*pixels=>pixels=64 

            //1.首先要确定像素p(x,y)投影在瓦片t(tx,ty)的坐标
            int tx = x / 8;
            int ty = y / 8;

            //2.计算出这个tile在vram中的偏移地址,根据瓦片t坐标系
            //然后根据mirroringType判断要取第几屏的数据，参考资料3*)
            int vramOffset = ty * 32 + tx;
            //开发模式下默认取第一屏的，所以起始地址为0x2000
            vramOffset += 0x2000;

            //3.根据vram中的偏移地址，得到pattern table中的数据地址
            //因为pattern talbe中每16字节为单位，所以需要乘起来
            int patternTableOffset = m_PPUBus.ReadByte((ushort)vramOffset);
            patternTableOffset *= 16;

            //4.根据CTRL寄存器中的B标识符，判断是在pattern table中的前4k还是后4k
            if (CTRL.B == 1)
            {
                patternTableOffset += 0x1000;
            }
            else if (CTRL.B == 0)
            {
                patternTableOffset += 0x0000;
            }

            //5.pattern table的数据16字节为一组，分为前8组（调色板的bit0)，和后8组(调色板的bit1)
            //回到最开头说的大tile,里面包含了16个小tile,因为一个调色板需要4个bit进行定位
            //pattern table确定了bit1和bit0，bit3和bit4由 跟着name table的attribute决定(64字节，与大title个数一致)
            byte[] patternData = ReadBlock((ushort)patternTableOffset, 16);
            byte[] lowPatternData = patternData.Take(8).ToArray();
            byte[] highPatternData = patternData.Skip(8).ToArray();
            Direction direction = GetDirection(tx, ty);
            int attributeIndex = GetAttributeIndex(tx, ty);

            //6.根据资料4*),得到attribute的地址和值
            int attributeOffset = 0x2000 + 960 + attributeIndex;
            byte attributeData = m_PPUBus.ReadByte((ushort)attributeOffset);

            //7.根据方位取得attributeData上的某2位，和pattern name取出来的数据进行组合，得到一个tile的全部颜色
            byte highPalette;
            if (direction is Direction.LeftTop)
            {
                //左上,第0和第1位
                highPalette = (byte)(attributeData & 0x03);
            }
            else if (direction is Direction.RightTop)
            {
                //右上,第2和第3位
                highPalette = (byte)((attributeData & 0x0C) >> 2);
            }
            else if (direction is Direction.LeftBottom)
            {
                //左下,第5和第4位
                highPalette = (byte)((attributeData & 0x30) >> 4);
            }
            else
            {
                //右下,第7和第6位
                highPalette = (byte)((attributeData & 0xC0) >> 6);
            }

            highPalette <<= 2; //调色板高位2位确定

            int[][] r = new int[8][];
            for (int i = 0; i < lowPatternData.Length; ++i)
            {
                r[i] = new int[8];

                for (int j = 7; j >= 0; --j)
                {
                    int bit0 = BitService.GetBit(lowPatternData[i], j);
                    int bit1 = BitService.GetBit(highPatternData[i], j);
                    int paletteOffset = highPalette | (bit1 << 1) | bit0;
                    r[i][7 - j] = GetBackgroundColor(paletteOffset);
                }
            }

            x = x % 7;
            y = y % 7;
            return r[y][x];
        }

        public int[][] GetBackgroundTileColor(int tx, int ty)
        {
            //2.计算出这个tile在vram中的偏移地址,根据瓦片t坐标系
            //然后根据mirroringType判断要取第几屏的数据，参考资料3*)
            int vramOffset = ty * 32 + tx;
            //开发模式下默认取第一屏的，所以起始地址为0x2000
            vramOffset += 0x2000;

            //3.根据vram中的偏移地址，得到pattern table中的数据地址
            //因为pattern talbe中每16字节为单位，所以需要乘起来
            int patternTableOffset = m_PPUBus.ReadByte((ushort)vramOffset);
            patternTableOffset *= 16;

            //4.根据CTRL寄存器中的B标识符，判断是在pattern table中的前4k还是后4k
            if (CTRL.B == 1)
            {
                patternTableOffset += 0x1000;
            }
            else if (CTRL.B == 0)
            {
                patternTableOffset += 0x0000;
            }

            //5.pattern table的数据16字节为一组，分为前8组（调色板的bit0)，和后8组(调色板的bit1)
            //回到最开头说的大tile,里面包含了16个小tile,因为一个调色板需要4个bit进行定位
            //pattern table确定了bit1和bit0，bit3和bit4由 跟着name table的attribute决定(64字节，与大title个数一致)
            byte[] patternData = ReadBlock((ushort)patternTableOffset, 16);
            byte[] lowPatternData = patternData.Take(8).ToArray();
            byte[] highPatternData = patternData.Skip(8).ToArray();
            Direction direction = GetDirection(tx, ty);
            int attributeIndex = GetAttributeIndex(tx, ty);

            //6.根据资料4*),得到attribute的地址和值
            int attributeOffset = 0x2000 + 960 + attributeIndex;
            byte attributeData = m_PPUBus.ReadByte((ushort)attributeOffset);

            //7.根据方位取得attributeData上的某2位，和pattern name取出来的数据进行组合，得到一个tile的全部颜色
            byte highPalette;
            if (direction is Direction.LeftTop)
            {
                //左上,第0和第1位
                highPalette = (byte)(attributeData & 0x03);
            }
            else if (direction is Direction.RightTop)
            {
                //右上,第2和第3位
                highPalette = (byte)((attributeData & 0x0C) >> 2);
            }
            else if (direction is Direction.LeftBottom)
            {
                //左下,第5和第4位
                highPalette = (byte)((attributeData & 0x30) >> 4);
            }
            else
            {
                //右下,第7和第6位
                highPalette = (byte)((attributeData & 0xC0) >> 6);
            }

            highPalette <<= 2; //调色板高位2位确定

            int[][] r = new int[8][];
            for (int i = 0; i < lowPatternData.Length; ++i)
            {
                r[i] = new int[8];

                for (int j = 7; j >= 0; --j)
                {
                    int bit0 = BitService.GetBit(lowPatternData[i], j);
                    int bit1 = BitService.GetBit(highPatternData[i], j);
                    int paletteOffset = highPalette | (bit1 << 1) | bit0;
                    r[i][7 - j] = GetBackgroundColor(paletteOffset);
                }
            }

            return r;
        }

        public int[][] GetSpriteTileColor()
        {
            if (CTRL.H == 0)
            {
                int block = 0 * 4;
                int y = OAM[block];
                int x = OAM[block + 3];
                int offset = 0;
                if (CTRL.S == 1)
                {
                    offset = 0x1000;
                }

                offset += OAM[block + 1] * 16;
                byte[] patternData = ReadBlock((ushort)offset, 16);
                byte[] lowPatternData = patternData.Take(8).ToArray();
                byte[] highPatternData = patternData.Skip(8).ToArray();
                byte highPalette = (byte)((OAM[block + 2] & 0x3) << 2);

                int[][] r = new int[8][];
                for (int i = 0; i < lowPatternData.Length; ++i)
                {
                    r[i] = new int[8];

                    for (int j = 7; j >= 0; --j)
                    {
                        int bit0 = BitService.GetBit(lowPatternData[i], j);
                        int bit1 = BitService.GetBit(highPatternData[i], j);
                        int paletteOffset = highPalette | (bit1 << 1) | bit0;
                        r[i][7 - j] = GetBackgroundColor(paletteOffset);
                    }
                }

                return r;
            }
            return null;
        }

        private byte[] ReadBlock(ushort addr, int length)
        {
            byte[] r = new byte[length];
            for (int i = 0; i < length; ++i)
            {
                r[i] = ReadByte((ushort)(addr + i));
            }
            return r;
        }

        public void SwitchNameTableMirroring(MirroringMode mirroringMode)
        {
            //参考资料2*)
            m_MirroringMode = mirroringMode;
        }

        /// <summary>
        /// 获取该tile在大tile上的方向，左上，右上，左下，右下
        /// </summary>
        /// <param name="tx">tile坐标系的x轴</param>
        /// <param name="ty">tile坐标系的y轴</param>
        /// <returns>方位</returns>
        private Direction GetDirection(int tx, int ty)
        {
            int modX = tx % 4;
            int modY = ty % 4;

            bool isLeft = (modX == 0 || modX == 1);
            bool isTop = (modY == 0 || modY == 1);

            if (isLeft && isTop)
            {
                return Direction.LeftTop;
            }
            else if (isLeft && !isTop)
            {
                return Direction.LeftBottom;
            }
            else if (!isLeft && isTop)
            {
                return Direction.RightTop;
            }
            return Direction.RightBottom;
        }

        /// <summary>
        /// 获取该tile在大tile上的index，横着一排有8个大tile
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        private int GetAttributeIndex(int tx, int ty)
        {
            int x = tx / 4;
            int y = ty / 4;
            return y * 8 + x;
        }

        private int GetBackgroundColor(int paletteOffset)
        {
            byte offset = m_PPUBus.ReadByte((ushort)(0x3F00 + paletteOffset));
            return Palette.GetRGBAColor(offset);
        }
    }
}
