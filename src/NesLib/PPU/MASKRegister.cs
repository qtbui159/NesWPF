using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace NesLib.PPU
{
    class MASKRegister
    {
        public byte Value { get; private set; } = 0;

        /// <summary>
        /// 显示模式
        /// 0(彩色) 1(灰阶)
        /// </summary>
        public byte Greyscale { get => BitService.GetBit(Value, 0); private set => Handle(0, value); }

        /// <summary>
        /// 背景掩码
        /// 0(不显示最左边那列, 8像素)的背景，1显示
        /// </summary>
        public byte m { get => BitService.GetBit(Value, 1); private set => Handle(1, value); }

        /// <summary>
        /// 精灵掩码
        /// 0(不显示最左边那列, 8像素)的精灵，1显示
        /// </summary>
        public byte M { get => BitService.GetBit(Value, 2); private set => Handle(2, value); }

        /// <summary>
        /// 背景显示使能标志位
        /// 1(显示背景)
        /// </summary>
        public byte b { get => BitService.GetBit(Value, 3); private set => Handle(3, value); }

        /// <summary>
        /// 精灵显示使能标志位
        /// 1(显示精灵)
        /// </summary>
        public byte s { get => BitService.GetBit(Value, 4); private set => Handle(4, value); }

        /// <summary>
        /// red (green on PAL/Dendy)
        /// </summary>
        public byte R { get => BitService.GetBit(Value, 5); private set => Handle(5, value); }
        /// <summary>
        /// Emphasize green (red on PAL/Dendy)
        /// </summary>
        public byte G { get => BitService.GetBit(Value, 6); private set => Handle(6, value); }
        /// <summary>
        /// Emphasize blue
        /// </summary>
        public byte B { get => BitService.GetBit(Value, 7); private set => Handle(7, value); }

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
