using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace NesLib.CPU
{
    /// <summary>
    /// 处理器状态寄存器
    /// https://www.nesdev.org/obelisk-6502-guide/registers.html#C
    /// https://wiki.nesdev.org/w/index.php?title=Status_flags
    /// </summary>
    class ProcessorStatusRegister
    {
        public byte Value { get; private set; } = 0x24;

        public byte CarryFlag { get => BitService.GetBit(Value, 0); set => Handle(0, value); }
        public byte ZeroFlag { get => BitService.GetBit(Value, 1); set => Handle(1, value); }
        public byte InterruptDisable { get => BitService.GetBit(Value, 2); set => Handle(2, value); }
        public byte DecimalMode { get => BitService.GetBit(Value, 3); set => Handle(3, value); }
        public byte BreakCommand { get => BitService.GetBit(Value, 4); set => Handle(4, value); }
        public byte OverflowFlag { get => BitService.GetBit(Value, 6); set => Handle(6, value); }
        public byte NegativeFlag { get => BitService.GetBit(Value, 7); set => Handle(7, value); }

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
