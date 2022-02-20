using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class BitService
    {
        /// <summary>
        /// 获取bit值，结果1或者0
        /// </summary>
        /// <param name="b">字节</param>
        /// <param name="pos">位置,0-7</param>
        /// <returns></returns>
        public static byte GetBit(byte b, int pos)
        {
            if (pos < 0 || pos > 7)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }

            return (byte)((b & (1 << pos)) != 0 ? 1 : 0);
        }

        public static byte GetBit(ushort b, int pos)
        {
            if (pos < 0 || pos > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(pos));
            }

            return (byte)((b & (1 << pos)) != 0 ? 1 : 0);
        }

        /// <summary>
        /// 置bit
        /// </summary>
        /// <param name="b"></param>
        /// <param name="pos"></param>
        /// <param name="value"></param>
        public static byte SetBit(byte b, int pos)
        {
            return (byte)(b | (1 << pos));
        }

        public static ushort SetBit(ushort b, int pos)
        {
            return (ushort)(b | (1 << pos));
        }

        /// <summary>
        /// 清bit
        /// </summary>
        /// <param name="b"></param>
        /// <param name="pos"></param>
        public static byte ClearBit(byte b, int pos)
        {
            return (byte)(b & ~(1 << pos));
        }

        public static ushort ClearBit(ushort b, int pos)
        {
            return (ushort)(b & ~(1 << pos));
        }
    }
}
