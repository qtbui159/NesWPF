using NesLib.Bus;
using NesLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//参考资料：
//1*)https://wiki.nesdev.org/w/index.php?title=PPU_power_up_state
//2*)https://wiki.nesdev.org/w/index.php?title=Mirroring#Nametable_Mirroring
//3*)https://wiki.nesdev.org/w/index.php?title=PPU_nametables
//4*)https://wiki.nesdev.org/w/index.php?title=PPU_attribute_tables

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
            return m_PPUBus.ReadByte(addr);
        }

        public void GetBackgroundPixel(int x, int y, out byte r, out byte g, out byte b, out byte a)
        {
            //PPU将画面(256*240)分割为32*30的小块，一共960个小块，每1个小块为一个tile(瓦片)，每个tile包含8个像素信息
            //tile坐标系为横坐标0-31,纵坐标0-29,并且每4*4的tile为一个大tile
            //开头的一个大tile为左上角(0,0),右下角(3,3)

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
            //pattern table确定了bit1和bit0，bit3和bit4由
            byte[] patternData = ReadBlock((ushort)patternTableOffset, 16);
            byte[] lowPatternData = patternData.Take(8).ToArray();
            byte[] highPatternData = patternData.Skip(8).ToArray();


            r = 0;
            g = 0;
            b = 0;
            a = 0;
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
    }
}
