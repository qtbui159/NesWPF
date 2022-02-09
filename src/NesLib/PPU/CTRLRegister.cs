using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

/**
 * 参考资料
 * https://wiki.nesdev.org/w/index.php/PPU_programmer_reference#PPUCTRL
 */

namespace NesLib.PPU
{
    class CTRLRegister
    {
        public byte Value { get; private set; } = 0;

        /// <summary>
        /// nametable地址 低位
        /// 
        /// 和滚动有关，1: Add 256 to the X scroll position
        /// </summary>
        public byte NLow { get => BitService.GetBit(Value, 0); set => Handle(0, value); }
        /// <summary>
        /// nametable地址 高位
        /// 
        /// 和滚动有关，1: Add 240 to the Y scroll position
        /// </summary>
        public byte NHigh { get => BitService.GetBit(Value, 1); set => Handle(1, value); }
        /// <summary>
        /// 命名表地址
        /// </summary>
        public ushort NameTable => (ushort)((NHigh << 8) | NLow);

        /// <summary>
        /// PPU读写显存增量
        /// 0(+1 列模式) 1(+32 行模式)
        /// </summary>
        public byte I { get => BitService.GetBit(Value, 2); set => Handle(2, value); }

        /// <summary>
        /// 精灵用图样表地址
        /// 0($0000) 1($1000)
        /// </summary>
        public byte S { get => BitService.GetBit(Value, 3); set => Handle(3, value); }

        /// <summary>
        /// 背景用图样表地址
        /// 0($0000) 1($1000)
        /// </summary>
        public byte B { get => BitService.GetBit(Value, 4); set => Handle(4, value); }

        /// <summary>
        /// 精灵尺寸(高度)
        /// 0(8x8) 1(8x16)
        /// </summary>
        public byte H { get => BitService.GetBit(Value, 5); set => Handle(5, value); }

        /// <summary>
        /// PPU 主/从模式
        /// FC没有用到
        /// </summary>
        public byte P { get => BitService.GetBit(Value, 6); set => Handle(6, value); }

        /// <summary>
        /// NMI生成使能标志位
        /// 1(在VBlank时触发NMI)
        /// </summary>
        public byte V { get => BitService.GetBit(Value, 7); set => Handle(7, value); }

        private void Handle(int pos, byte value)
        {
            if (value == 1)
            {
                Value = BitService.SetBit(Value, pos);
            }
            else if (value == 0)
            {
                Value = BitService.ClearBit(Value, pos);
            }
            else
            {
                throw new Exception($"value值超出范围");
            }
        }

        public void SetValue(byte value)
        {
            Value = value;
        }
    }
}
