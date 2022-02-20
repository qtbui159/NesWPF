using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

/*
 * 参考资料
 * 1*)https://wiki.nesdev.org/w/index.php?title=PPU_scrolling#.242005_first_write_.28w_is_0.29
 */
namespace NesLib.PPU
{
    class VRAMAddrRegister
    {
        public ushort Value { get; private set; }

        /// <summary>
        /// 粗略X滚动偏移（按tile瓦片,8像素)
        /// 0-4bit
        /// </summary>
        public byte CoarseXScroll => (byte)(Value & 0x1F);

        /// <summary>
        /// 粗略Y滚动偏移（按tile瓦片,8像素)
        /// 5-9bit
        /// </summary>
        public byte CoarseYScroll => (byte)((Value >> 5) & 0x1F);

        /// <summary>
        /// 命名表选择
        /// 10-11bit
        /// </summary>
        public byte NameTable => (byte)((Value >> 10) & 0x3);

        /// <summary>
        /// Y精确滚动偏移（按1像素）
        /// 12-14bit
        /// </summary>
        public byte FineYScroll => (byte)((Value >> 12) & 0x7);

        public void SetValue(ushort value)
        {
            Value = value;
        }

        public void UpdateBit(byte value, int pos)
        {
            if (pos < 0 || pos > 14)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }

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
                throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}
