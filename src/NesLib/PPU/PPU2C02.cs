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
//7*)https://wiki.nesdev.org/w/index.php?title=Sprite_overflow_games
//8*)https://wiki.nesdev.org/w/index.php?title=PPU_sprite_evaluation
//9*)https://wiki.nesdev.org/w/index.php?title=PPU_scrolling#At_dot_256_of_each_scanline
//10*)https://wiki.nesdev.org/w/images/4/4f/Ppu.svg

namespace NesLib.PPU
{
    class PPU2C02 : IPPU2C02
    {
        private const int SCAN_LINE_COLUMN = 341;

        private readonly IPPUBus m_PPUBus;

        private MirroringMode m_MirroringMode;

        public CTRLRegister CTRL { get; private set; }
        public MASKRegister MASK { get; private set; }
        public STATUSRegister STATUS { get; private set; }
        public VRAMAddrRegister T { get; set; }
        public VRAMAddrRegister Addr { get; set; }
        public byte X { get; set; }

        /// <summary>
        /// 这里只实现了一级OAM，实际PPU中还有一个二级OAM，和sprite overflow bug息息相关
        /// 参考资料7*,8*
        /// </summary>
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
            Addr = new VRAMAddrRegister();
            T = new VRAMAddrRegister();
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
            vramOffset = GetVRAMRealAddr((ushort)vramOffset);

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
            int attributeOffset = GetVRAMBaseRealAddr((ushort)vramOffset) + 960 + attributeIndex;
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

        public int[][] GetSpriteTileColor(int count, out int x, out int y)
        {
            if (CTRL.H == 0)
            {
                int block = count * 4;
                y = OAM[block];
                x = OAM[block + 3];
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
                byte horizentalFlip = BitService.GetBit(OAM[block + 2], 6);
                byte verticalFlip = BitService.GetBit(OAM[block + 2], 7);
                int[][] r = new int[8][];
                for (int i = 0; i < lowPatternData.Length; ++i)
                {
                    r[i] = new int[8];

                    for (int j = 7; j >= 0; --j)
                    {
                        int bit0 = BitService.GetBit(lowPatternData[i], j);
                        int bit1 = BitService.GetBit(highPatternData[i], j);
                        int paletteOffset = highPalette | (bit1 << 1) | bit0;
                        r[i][7 - j] = GetSpriteColor(paletteOffset);
                    }
                }

                if (count == 0)
                {
                    STATUS.S = 1;
                }

                if (horizentalFlip == 1)
                {
                    r = HorizentalFlip(r);
                }
                if (verticalFlip == 1)
                {
                    r = VerticalFlip(r);
                }
                return r;
            }
            x = 0;
            y = 0;
            return null;
        }

        private int[][] GetSpriteTileColor(int count, out int x, out int y, out bool visible)
        {
            if (CTRL.H == 0)
            {
                int block = count * 4;
                y = OAM[block];
                x = OAM[block + 3];
                int offset = 0;
                if (CTRL.S == 1)
                {
                    offset = 0x1000;
                }

                offset += OAM[block + 1] * 16;
                byte[] patternData = ReadBlock((ushort)offset, 16);
                byte highPalette = (byte)((OAM[block + 2] & 0x3) << 2);
                byte horizentalFlip = BitService.GetBit(OAM[block + 2], 6);
                byte verticalFlip = BitService.GetBit(OAM[block + 2], 7);
                int[][] r = new int[8][];
                for (int i = 0; i < 8; ++i)
                {
                    r[i] = new int[8];

                    for (int j = 7; j >= 0; --j)
                    {
                        int bit0 = BitService.GetBit(patternData[i], j);
                        int bit1 = BitService.GetBit(patternData[i + 8], j);
                        int paletteOffset = highPalette | (bit1 << 1) | bit0;
                        r[i][7 - j] = GetSpriteColor(paletteOffset);
                    }
                }

                if (horizentalFlip == 1)
                {
                    r = HorizentalFlip(r);
                }
                if (verticalFlip == 1)
                {
                    r = VerticalFlip(r);
                }
                visible = BitService.GetBit(OAM[block + 2], 5) == 0;
                return r;
            }
            x = 0;
            y = 0;
            visible = false;
            return null;
        }

        public void PreRenderLine()
        {
            if (MASK.b == 0 || MASK.s == 0)
            {
                return;
            }

            for (int x = 0; x < 256; ++x)
            {
                if (x % 8 == 0 && x != 0)
                {
                    IncX();
                }
            }
            //256dot
            IncX();
            IncY();

            //257dot
            Addr.UpdateBit(BitService.GetBit(T.Value, 0), 0);
            Addr.UpdateBit(BitService.GetBit(T.Value, 1), 1);
            Addr.UpdateBit(BitService.GetBit(T.Value, 2), 2);
            Addr.UpdateBit(BitService.GetBit(T.Value, 3), 3);
            Addr.UpdateBit(BitService.GetBit(T.Value, 4), 4);
            Addr.UpdateBit(BitService.GetBit(T.Value, 10), 10);

            //280-304dot

            Addr.UpdateBit(BitService.GetBit(T.Value, 5), 5);
            Addr.UpdateBit(BitService.GetBit(T.Value, 6), 6);
            Addr.UpdateBit(BitService.GetBit(T.Value, 7), 7);
            Addr.UpdateBit(BitService.GetBit(T.Value, 8), 8);
            Addr.UpdateBit(BitService.GetBit(T.Value, 9), 9);

            Addr.UpdateBit(BitService.GetBit(T.Value, 11), 11);
            Addr.UpdateBit(BitService.GetBit(T.Value, 12), 12);
            Addr.UpdateBit(BitService.GetBit(T.Value, 13), 13);
            Addr.UpdateBit(BitService.GetBit(T.Value, 14), 14);

            //328,336dot
            IncX();
            IncX();
        }

        public int[] PaintScanLine(int y, ref bool spriteHit)
        {
            if (y < 0 || y > 239)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }

            if (MASK.b == 0)
            {
                return Enumerable.Repeat(0x000000FF, 256).ToArray();
            }
            
            List<int> rgba = new List<int>();
            for (int x = 0; x < 256; x += 8)
            {
                if (x % 8 == 0 && x != 0)
                {
                    IncX();
                }
                
                VRAMAddrRegister vram = Addr;
                ushort tileAddress = GetVRAMRealAddr((ushort)(0x2000 | (vram.Value & 0x0FFF)));
                ushort attributeAddress = (ushort)(0x23C0 | (vram.Value & 0x0C00) | ((vram.Value >> 4) & 0x38) | ((vram.Value >> 2) & 0x07));

                //3.根据vram中的偏移地址，得到pattern table中的数据地址
                //因为pattern talbe中每16字节为单位，所以需要乘起来
                int patternTableOffset = m_PPUBus.ReadByte(tileAddress);
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
                Direction direction = GetDirection(vram.CoarseXScroll, vram.CoarseYScroll);

                //6.根据资料4*),得到attribute的地址和值
                byte attributeData = m_PPUBus.ReadByte(attributeAddress);

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

                int currentY = y % 8;
                for (int j = 7; j >= 0; --j)
                {
                    int bit0 = BitService.GetBit(patternData[currentY], j);
                    int bit1 = BitService.GetBit(patternData[8 + currentY], j);
                    int paletteOffset = highPalette | (bit1 << 1) | bit0;
                    rgba.Add(GetBackgroundColor(paletteOffset));
                }
            }

            //256 dot
            IncX();
            IncY();
            //257dot
            Addr.UpdateBit(BitService.GetBit(T.Value, 0), 0);
            Addr.UpdateBit(BitService.GetBit(T.Value, 1), 1);
            Addr.UpdateBit(BitService.GetBit(T.Value, 2), 2);
            Addr.UpdateBit(BitService.GetBit(T.Value, 3), 3);
            Addr.UpdateBit(BitService.GetBit(T.Value, 4), 4);
            Addr.UpdateBit(BitService.GetBit(T.Value, 10), 10);

            //328&336
            IncX();
            IncX();


            if (!spriteHit)
            {
                int bbb = Palette.GetRGBAColor(m_PPUBus.ReadByte(0x3F00));
                int[][] sprite = GetSpriteTileColor(0, out int sx, out int sy, out bool visible);
                //判断当前y和精灵y是否有交集
                if (y >= sy && y <= sy + 7)
                {
                    for (int i = 0; i < 8; ++i)
                    {
                        if (sprite[y - sy][i] != bbb && rgba[sx + i] != bbb)
                        {
                            if (MASK.s == 1)
                            {
                                STATUS.S = 1;
                                spriteHit = true;
                            }
                        }
                    }
                }
            }

            return rgba.ToArray();
        }
        public void PaintSprite(int[][] background)
        {
            if (MASK.s == 0)
            {
                return;
            }
            int bbb = Palette.GetRGBAColor(m_PPUBus.ReadByte(0x3F00));

            for (int i = 63; i >= 0; --i)
            {
                int[][] sprite = GetSpriteTileColor(i, out int x, out int y, out bool visible);
                if (!visible)
                {
                    continue;
                }
                if (y >= 240 - 7 || x >= 256 - 7)
                {
                    continue;
                }
                if (MASK.M == 0 && x == 0)
                {
                    continue;
                }

                for (int py = 0; py < 8; ++py)
                {
                    for (int px = 0; px < 8; ++px)
                    {
                        if (sprite[py][px] != bbb)
                        {
                            background[y + py][x + px] = sprite[py][px];
                        }
                    }
                }
            }
        }

        public int[][] PaintFrame()
        {
            int bbb = Palette.GetRGBAColor(m_PPUBus.ReadByte(0x3F00));

            int[][] frame = new int[240][];
            for (int i = 0; i < 240; ++i)
            {
                frame[i] = new int[256];
            }

            //1.先取出背景
            for (int ty = 0; ty < 30; ++ty)
            {
                for (int tx = 0; tx < 32; ++tx)
                {
                    int[][] tile = GetBackgroundTileColor(tx, ty);

                    for (int y = 0; y < 8; ++y)
                    {
                        Array.Copy(tile[y], 0, frame[ty * 8 + y], tx * 8, 8);
                    }
                }
            }

            //2.取出精灵
            for (int i = 63; i >= 0; --i)
            {
                int[][] sprite = GetSpriteTileColor(i, out int x, out int y, out bool visible);
                if (!visible)
                {
                    continue;
                }
                if (y >= 240 - 7 || x >= 256 - 7)
                {
                    continue;
                }

                //for (int py = 0; py < 8; ++py)
                //{
                //    Array.Copy(sprite[py], 0, frame[y + py], x, 8);
                //}
                for (int py = 0; py < 8; ++py)
                {
                    for (int px = 0; px < 8; ++px)
                    {
                        if (sprite[py][px] != bbb)
                        {
                            frame[y + py][x + px] = sprite[py][px];
                        }
                    }
                }
            }

            return frame;
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

        private int GetSpriteColor(int paletteOffset)
        {
            byte offset = m_PPUBus.ReadByte((ushort)(0x3F10 + paletteOffset));
            int value = Palette.GetRGBAColor(offset);
            return value;
        }

        private int[][] HorizentalFlip(int[][] data)
        {
            for (int y = 0; y < 8; ++y)
            {
                for (int x = 0; x < 4; ++x)
                {
                    if (data[y][x] != data[y][7 - x])
                    {
                        int tmp = data[y][7 - x];
                        data[y][7 - x] = data[y][x];
                        data[y][x] = tmp;
                    }
                }
            }

            return data;
        }

        private int[][] VerticalFlip(int[][] data)
        {
            for (int y = 0; y < 8; ++y)
            {
                for (int x = 0; x < 4; ++x)
                {
                    if (data[y][x] != data[7 - y][x])
                    {
                        int tmp = data[7 - y][x];
                        data[7 - y][x] = data[y][x];
                        data[y][x] = tmp;
                    }
                }
            }

            return data;
        }

        private ushort GetVRAMRealAddr(ushort addr)
        {
            if (m_MirroringMode is MirroringMode.Horizontal)
            {
                if (addr < 0x2800)
                {
                    return (ushort)(addr & 0x23FF);
                }
                else
                {
                    return (ushort)(addr & 0x2BFF);
                }
            }
            else if (m_MirroringMode is MirroringMode.Vertical)
            {
                if (addr >= 0x2000 && addr < 0x2800)
                {
                    return addr;
                }
                else
                {
                    return (ushort)(addr - 0x800);
                }
            }
            throw new Exception("未知镜像");
        }

        private ushort GetVRAMBaseRealAddr(ushort addr)
        {
            if (m_MirroringMode is MirroringMode.Horizontal)
            {
                if (addr < 0x2800)
                {
                    return 0x2000;
                }
                else
                {
                    return 0x2800;
                }
            }
            else if (m_MirroringMode is MirroringMode.Vertical)
            {
                if (addr < 0x2400)
                {
                    return 0x2000;
                }
                else if (addr < 0x2800)
                {
                    return 0x2400;
                }
                else if (addr < 0x2C00)
                {
                    return 0x2000;
                }
                else 
                {
                    return 0x2400;
                }
            }
            throw new Exception("未知镜像");
        }

        public void IncX()
        {
            ushort tmpAddr = Addr.Value;
            if ((tmpAddr & 0x1F) == 31)
            {
                tmpAddr = (ushort)(tmpAddr & ~0x001F);
                tmpAddr ^= 0x400;
                Addr.SetValue(tmpAddr);
            }
            else
            {
                Addr.SetValue(++tmpAddr);
            }
        }

        public void IncY()
        {
            ushort tmpAddr = Addr.Value;
            if ((tmpAddr & 0x7000) != 0x7000)
            {
                tmpAddr += 0x1000;
            }
            else
            {
                tmpAddr = (ushort)(tmpAddr & ~0x7000);
                int y = (tmpAddr & 0x03E0) >> 5;

                if (y == 29)
                {
                    y = 0;
                    tmpAddr ^= 0x0800;
                }
                else if (y == 31)
                {
                    y = 0;
                }
                else
                {
                    y += 1;
                }

                tmpAddr = (ushort)((tmpAddr & ~0x03E0) | (y << 5));
            }

            Addr.SetValue(tmpAddr);
        }
    }
}
