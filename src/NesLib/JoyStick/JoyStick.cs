using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

/**
 * 参考资料
 * 1*)https://wiki.nesdev.org/w/index.php?title=Standard_controller
 */

namespace NesLib.JoyStick
{
    class JoyStick : IJoyStick
    {
        private byte m_Value = 0;
        private int m_Offset = 0;

        public void A(bool pressDown)
        {
            const byte BIT_POS = 7;
            Press(pressDown, BIT_POS);
        }

        public void B(bool pressDown)
        {
            const byte BIT_POS = 6;
            Press(pressDown, BIT_POS);
        }

        public void Down(bool pressDown)
        {
            const byte BIT_POS = 2;
            Press(pressDown, BIT_POS);
        }

        public void Left(bool pressDown)
        {
            const byte BIT_POS = 1;
            Press(pressDown, BIT_POS);
        }

        public void Right(bool pressDown)
        {
            const byte BIT_POS = 0;
            Press(pressDown, BIT_POS);
        }

        public void Select(bool pressDown)
        {
            const byte BIT_POS = 5;
            Press(pressDown, BIT_POS);
        }

        public void Start(bool pressDown)
        {
            const byte BIT_POS = 4;
            Press(pressDown, BIT_POS);
        }

        public void Up(bool pressDown)
        {
            const byte BIT_POS = 3;
            Press(pressDown, BIT_POS);
        }

        private void Press(bool pressDown, byte pos)
        {
            if (pressDown)
            {
                m_Value = BitService.SetBit(m_Value, pos);
            }
            else
            {
                m_Value = BitService.ClearBit(m_Value, pos);
            }
        }

        public void ClearOffset()
        {
            m_Offset = 7;
        }

        public byte GetValue()
        {
            return BitService.GetBit(m_Value, m_Offset--);
        }
    }
}
