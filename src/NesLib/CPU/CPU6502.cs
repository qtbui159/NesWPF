using NesLib.Bus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

//参考资料：
//1*)https://www.nesdev.org/obelisk-6502-guide/reference.html
//2*)http://users.telenet.be/kim1-6502/6502/proman.html
//3*)https://wiki.nesdev.org/w/index.php?title=CPU_unofficial_opcodes
//4*)https://wiki.nesdev.org/w/index.php?title=Programming_with_unofficial_opcodes
//5*)https://wiki.nesdev.org/w/index.php?title=CPU_power_up_state

namespace NesLib.CPU
{
    class CPU6502 : ICPU6502
    {
        private ICPUBus m_CPUBus;
        private readonly Dictionary<int, Action<byte>> m_OPCodeMapImpl;

        public long Cycles { get; set; }

        /// <summary>
        /// 累加寄存器
        /// </summary>
        public byte A { get; private set; }
        /// <summary>
        /// 循环计数器寄存器
        /// </summary>
        public byte X { get; private set; }
        /// <summary>
        /// 循环计数器寄存器
        /// </summary>
        public byte Y { get; private set; }
        /// <summary>
        /// 程序计数器
        /// </summary>
        public ushort PC { get; private set; }
        /// <summary>
        /// 堆栈寄存器，对应着 CPU 总线上的 0x100 ~ 0x1FF
        /// </summary>
        public byte SP { get; private set; } = 0xFF;
        /// <summary>
        /// 标志寄存器
        /// </summary>
        public ProcessorStatusRegister P { get; private set; }

        public CPU6502(ICPUBus cpuBus)
        {
            m_CPUBus = cpuBus;

            P = new ProcessorStatusRegister();
            m_OPCodeMapImpl = new Dictionary<int, Action<byte>>();
            InitOPCode();
        }

        /// <summary>
        /// 非官方指令ANC,ALR,ARR,XAA,AHX,TAS,LAS,AXS未实现
        /// </summary>
        private void InitOPCode()
        {
            AddToDictionary(ADC, 0x69, 0x65, 0x75, 0x6D, 0x7D, 0x79, 0x61, 0x71);
            AddToDictionary(AND, 0x29, 0x25, 0x35, 0x2D, 0x3D, 0x39, 0x21, 0x31);
            AddToDictionary(ASL, 0x0A, 0x06, 0x16, 0x0E, 0x1E);
            AddToDictionary(BCC, 0x90);
            AddToDictionary(BCS, 0xB0);
            AddToDictionary(BEQ, 0xF0);
            AddToDictionary(BIT, 0x24, 0x2C);
            AddToDictionary(BMI, 0x30);
            AddToDictionary(BNE, 0xD0);
            AddToDictionary(BPL, 0x10);
            AddToDictionary(BRK, 0x00);
            AddToDictionary(BVC, 0x50);
            AddToDictionary(BVS, 0x70);
            AddToDictionary(CLC, 0x18);
            AddToDictionary(CLD, 0xD8);
            AddToDictionary(CLI, 0x58);
            AddToDictionary(CLV, 0xB8);
            AddToDictionary(CMP, 0xC9, 0xC5, 0xD5, 0xCD, 0xDD, 0xD9, 0xC1, 0xD1);
            AddToDictionary(CPX, 0xE0, 0xE4, 0xEC);
            AddToDictionary(CPY, 0xC0, 0xC4, 0xCC);
            AddToDictionary(DEC, 0xC6, 0xD6, 0xCE, 0xDE);
            AddToDictionary(DEX, 0xCA);
            AddToDictionary(DEY, 0x88);
            AddToDictionary(EOR, 0x49, 0x45, 0x55, 0x4D, 0x5D, 0x59, 0x41, 0x51);
            AddToDictionary(INC, 0xE6, 0xF6, 0xEE, 0xFE);
            AddToDictionary(INX, 0xE8);
            AddToDictionary(INY, 0xC8);
            AddToDictionary(JMP, 0x4C, 0x6C);
            AddToDictionary(JSR, 0x20);
            AddToDictionary(LDA, 0xA9, 0xA5, 0xB5, 0xAD, 0xBD, 0xB9, 0xA1, 0xB1);
            AddToDictionary(LDX, 0xA2, 0xA6, 0xB6, 0xAE, 0xBE);
            AddToDictionary(LDY, 0xA0, 0xA4, 0xB4, 0xAC, 0xBC);
            AddToDictionary(LSR, 0x4A, 0x46, 0x56, 0x4E, 0x5E);
            AddToDictionary(NOP, 0xEA);
            AddToDictionary(ORA, 0x09, 0x05, 0x15, 0x0D, 0x1D, 0x19, 0x01, 0x11);
            AddToDictionary(PHA, 0x48);
            AddToDictionary(PHP, 0x08);
            AddToDictionary(PLA, 0x68);
            AddToDictionary(PLP, 0x28);
            AddToDictionary(ROL, 0x2A, 0x26, 0x36, 0x2E, 0x3E);
            AddToDictionary(ROR, 0x6A, 0x66, 0x76, 0x6E, 0x7E);
            AddToDictionary(RTI, 0x40);
            AddToDictionary(RTS, 0x60);
            AddToDictionary(SBC, 0xE9, 0xE5, 0xF5, 0xED, 0xFD, 0xF9, 0xE1, 0xF1);
            AddToDictionary(SEC, 0x38);
            AddToDictionary(SED, 0xF8);
            AddToDictionary(SEI, 0x78);
            AddToDictionary(STA, 0x85, 0x95, 0x8D, 0x9D, 0x99, 0x81, 0x91);
            AddToDictionary(STX, 0x86, 0x96, 0x8E);
            AddToDictionary(STY, 0x84, 0x94, 0x8C);
            AddToDictionary(TAX, 0xAA);
            AddToDictionary(TAY, 0xA8);
            AddToDictionary(TSX, 0xBA);
            AddToDictionary(TXA, 0x8A);
            AddToDictionary(TXS, 0x9A);
            AddToDictionary(TYA, 0x98);
            AddToDictionary(UNOFFICAL_NOP, 0x80, 0x04, 0x44, 0x64, 0x0C, 0x14, 0x34, 0x54, 0x74, 0xD4, 0xF4, 0x1C, 0x3C, 0x5C, 0x7C, 0xDC, 0xFC, 0x89, 0x82, 0xC2, 0xE2, 0x1A, 0x3A, 0x5A, 0x7A, 0xDA, 0xFA);
            AddToDictionary(UNOFFICAL_LAX, 0xA3, 0xA7, 0xAB, 0xAF, 0xB3, 0xB7, 0xBF);
            AddToDictionary(UNOFFICAL_SAX, 0x83, 0x87, 0x8F, 0x97);
            AddToDictionary(UNOFFICAL_SBC, 0xEB);
            AddToDictionary(UNOFFICAL_DCP, 0xC3, 0xC7, 0xCF, 0xD3, 0xD7, 0xDB, 0xDF);
            AddToDictionary(UNOFFICAL_ISCorISB, 0xE3, 0xE7, 0xEF, 0xF3, 0xF7, 0xFB, 0xFF);
            AddToDictionary(UNOFFICAL_SLO, 0x03, 0x07, 0x0F, 0x13, 0x17, 0x1B, 0x1F);
            AddToDictionary(UNOFFICAL_RLA, 0x23, 0x27, 0x2F, 0x33, 0x37, 0x3B, 0x3F);
            AddToDictionary(UNOFFICAL_SRE, 0x43, 0x47, 0x4F, 0x53, 0x57, 0x5B, 0x5F);
            AddToDictionary(UNOFFICAL_RRA, 0x63, 0x67, 0x6F, 0x73, 0x77, 0x7B, 0x7F);
        }

        private void AddToDictionary(Action<byte> action, params int[] opCodes)
        {
            if (action is null || opCodes is null)
            {
                throw new ArgumentNullException();
            }

            foreach (var opCode in opCodes)
            {
                m_OPCodeMapImpl.Add(opCode, action);
            }
        }

        public void IRQ()
        {
            const ushort IRQ_ADDR = 0xFFFE;
        }

        public void NMI()
        {
            const ushort NMI_ADDR = 0xFFFA;
            
            Push((byte)((PC >> 8) & 0xFF));
            Push((byte)(PC & 0xFF));
            byte p = P.Value;
            p = BitService.SetBit(p, 5);
            Push(p);
            P.InterruptDisable = 1;

            PC = ReadWord(NMI_ADDR);
        }

        private StreamWriter sw;
        public void RESET()
        {
            const ushort RESET_ADDR = 0xFFFC;
            PC = ReadWord(RESET_ADDR);
            //PC = 0xC000;
            SP = 0xFD;
            A = 0;
            X = 0;
            Y = 0;
            P.SetValue(0x34);

            sw = new StreamWriter(new FileStream("D:/1.txt", FileMode.Create), Encoding.UTF8);
        }

        public void TickTock()
        {
            //sw.WriteLine($"{PC:X2}     A:{A:X2} X:{X:X2} Y:{Y:X2} P:{P.Value:X2} SP:{SP:X2}");
            //if (m_Cycles > 30000)
            //{
            //    sw.Close();
            //    return;
            //}

            //if (PC == 0xC66E)
            //{
            //    sw.Close();
            //}

            byte opCode = m_CPUBus.ReadByte(PC++);
            if (!m_OPCodeMapImpl.ContainsKey(opCode))
            {
                sw.Close();
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            Action<byte> impl = m_OPCodeMapImpl[opCode];
            impl.Invoke(opCode);
        }

        public void TickTock(int scanlineCount)
        {
            //一条扫描线的cpu指令周期为113.666667
            int needRunCycles = (int)Math.Ceiling(113.0 * 2 / 3 * scanlineCount);
            long endCycles = Cycles + needRunCycles;
            while (Cycles <= endCycles)
            {
                TickTock();
            }
        }

        public void ResetCycles()
        {
            Cycles = 0;
        }

        /// <summary>
        /// A,Z,C,N = A+M+C
        /// This instruction adds the contents of a memory location to the accumulator together with the carry bit.
        /// If overflow occurs the carry bit is set, this enables multiple byte addition to be performed.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        /// <exception cref="NotImplementedException"></exception>
        private void ADC(byte opCode)
        {
            ushort addr;
            if (opCode == 0x69)
            {
                addr = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0x65)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0x75)
            {
                addr = ZeroPageX();
                Cycles += 4;
            }
            else if (opCode == 0x6d)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else if (opCode == 0x7d)
            {
                addr = AbsoluteX(out bool crossPage);
                Cycles += 4;

                if (crossPage)
                {
                    //跨页了
                    Cycles += 1;
                }
            }
            else if (opCode == 0x79)
            {
                addr = AbsoluteY(out bool crossPage);
                Cycles += 4;

                if (crossPage)
                {
                    //跨页了
                    Cycles += 1;
                }
            }
            else if (opCode == 0x61)
            {
                addr = IndexedIndirect();
                Cycles += 6;
            }
            else if (opCode == 0x71)
            {
                addr = IndirectIndexed(out bool crossPage);
                Cycles += 5;

                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            byte oldA = A;
            ushort carryValue = (ushort)(A + data + P.CarryFlag);
            A = (byte)(A + data + P.CarryFlag);

            //标记的参考资料的2*
            P.ZeroFlag = BoolToBit(A == 0);
            P.NegativeFlag = BitService.GetBit(A, 7);
            P.OverflowFlag = BoolToBit(CheckAddOverflowPresent(oldA, data, P.CarryFlag));
            P.CarryFlag = BoolToBit((carryValue >> 8) != 0);
        }

        /// <summary>
        /// A,Z,N = A&M
        /// A logical AND is performed, bit by bit, on the accumulator contents using the contents of a byte of memory.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void AND(byte opCode)
        {
            ushort addr;
            if (opCode == 0x29)
            {
                addr = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0x25)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0x35)
            {
                addr = ZeroPageX();
                Cycles += 4;
            }
            else if (opCode == 0x2D)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else if (opCode == 0x3D)
            {
                addr = AbsoluteX(out bool crossPage);

                Cycles += 4;
                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else if (opCode == 0x39)
            {
                addr = AbsoluteY(out bool crossPage);

                Cycles += 4;
                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else if (opCode == 0x21)
            {
                addr = IndexedIndirect();
                Cycles += 6;
            }
            else if (opCode == 0x31)
            {
                addr = IndirectIndexed(out bool crossPage);

                Cycles += 5;
                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);

            A = (byte)(A & data);
            P.ZeroFlag = BoolToBit(A == 0);
            P.NegativeFlag = BitService.GetBit(A, 7);
        }

        /// <summary>
        /// A,Z,C,N = M*2 or M,Z,C,N = M*2
        /// This operation shifts all the bits of the accumulator or memory contents one bit left.
        /// Bit 0 is set to 0 and bit 7 is placed in the carry flag.The effect of this operation is to multiply the memory contents by 2 (ignoring 2's complement considerations), 
        /// setting the carry if the result will not fit in 8 bits.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void ASL(byte opCode)
        {
            ushort addr;
            if (opCode == 0x0A)
            {
                byte newCarryFlag = BitService.GetBit(A, 7);
                A <<= 1;

                P.ZeroFlag = BoolToBit(A == 0);
                P.NegativeFlag = BitService.GetBit(A, 7);
                P.CarryFlag = newCarryFlag;
                Cycles += 2;
                return;
            }
            else if (opCode == 0x06)
            {
                addr = ZeroPage();
                Cycles += 5;
            }
            else if (opCode == 0x16)
            {
                addr = ZeroPageX();
                Cycles += 6;
            }
            else if (opCode == 0x0E)
            {
                addr = Absolute();
                Cycles += 6;
            }
            else if (opCode == 0x1E)
            {
                addr = AbsoluteX(out _);
                Cycles += 7;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            {
                byte data = m_CPUBus.ReadByte(addr);
                byte newCarryFlag = BitService.GetBit(data, 7);
                data <<= 1;
                m_CPUBus.WriteByte(addr, data);

                P.ZeroFlag = BoolToBit(data == 0);
                P.NegativeFlag = BitService.GetBit(data, 7);
                P.CarryFlag = newCarryFlag;
            }
        }

        /// <summary>
        /// If the carry flag is clear then add the relative displacement to the program counter to cause a branch to a new location.
        /// </summary>
        /// <param name="opCode"></param>
        private void BCC(byte opCode)
        {
            if (opCode == 0x90)
            {
                ushort offset = Relative();
                Cycles += 2;
                if (P.CarryFlag == 0)
                {
                    Cycles += 1;

                    ushort newAddr = (ushort)(PC + (sbyte)offset);
                    if (IsCrossPage(newAddr, PC))
                    {
                        Cycles += 1;
                    }
                    PC = newAddr;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// If the carry flag is set then add the relative displacement to the program counter to cause a branch to a new location.
        /// </summary>
        /// <param name="opCode"></param>
        private void BCS(byte opCode)
        {
            if (opCode == 0xB0)
            {
                ushort offset = Relative();
                Cycles += 2;
                if (P.CarryFlag == 1)
                {
                    Cycles += 1;

                    ushort newAddr = (ushort)(PC + (sbyte)offset);
                    if (IsCrossPage(newAddr, PC))
                    {
                        Cycles += 1;
                    }
                    PC = newAddr;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// If the zero flag is set then add the relative displacement to the program counter to cause a branch to a new location.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void BEQ(byte opCode)
        {
            if (opCode == 0xF0)
            {
                ushort offset = Relative();
                Cycles += 2;
                if (P.ZeroFlag == 1)
                {
                    Cycles += 1;

                    ushort newAddr = (ushort)(PC + (sbyte)offset);
                    if (IsCrossPage(newAddr, PC))
                    {
                        Cycles += 1;
                    }
                    PC = newAddr;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// A & M, N = M7, V = M6
        /// This instructions is used to test if one or more bits are set in a target memory location.
        /// The mask pattern in A is ANDed with the value in memory to set or clear the zero flag, but the result is not kept. 
        /// Bits 7 and 6 of the value from memory are copied into the N and V flags.
        /// </summary>
        /// <param name="opCode"></param>
        private void BIT(byte opCode)
        {
            ushort addr;
            if (opCode == 0x24)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0x2C)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            byte result = (byte)(A & data);
            P.ZeroFlag = BoolToBit(result == 0);
            P.OverflowFlag = BitService.GetBit(data, 6);
            P.NegativeFlag = BitService.GetBit(data, 7);
        }

        /// <summary>
        /// If the negative flag is set then add the relative displacement to the program counter to cause a branch to a new location.
        /// </summary>
        /// <param name="opCode"></param>
        private void BMI(byte opCode)
        {
            if (opCode == 0x30)
            {
                ushort offset = Relative();
                Cycles += 2;
                if (P.NegativeFlag == 1)
                {
                    Cycles += 1;

                    ushort newAddr = (ushort)(PC + (sbyte)offset);
                    if (IsCrossPage(newAddr, PC))
                    {
                        Cycles += 1;
                    }
                    PC = newAddr;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// If the zero flag is clear then add the relative displacement to the program counter to cause a branch to a new location.
        /// </summary>
        /// <param name="opCode"></param>
        private void BNE(byte opCode)
        {
            if (opCode == 0xD0)
            {
                ushort offset = Relative();
                Cycles += 2;
                if (P.ZeroFlag == 0)
                {
                    Cycles += 1;

                    ushort newAddr = (ushort)(PC + (sbyte)offset);
                    if (IsCrossPage(newAddr, PC))
                    {
                        Cycles += 1;
                    }
                    PC = newAddr;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// If the negative flag is clear then add the relative displacement to the program counter to cause a branch to a new location.
        /// </summary>
        /// <param name="opCode"></param>
        private void BPL(byte opCode)
        {
            if (opCode == 0x10)
            {
                ushort offset = Relative();
                Cycles += 2;
                if (P.NegativeFlag == 0)
                {
                    Cycles += 1;

                    ushort newAddr = (ushort)(PC + (sbyte)offset);
                    if (IsCrossPage(newAddr, PC))
                    {
                        Cycles += 1;
                    }
                    PC = newAddr;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// The BRK instruction forces the generation of an interrupt request. 
        /// The program counter and processor status are pushed on the stack then the IRQ interrupt vector at $FFFE/F is loaded into the PC and the break flag in the status set to one.
        /// </summary>
        /// <param name="opCode"></param>
        private void BRK(byte opCode)
        {
            if (opCode == 0x00)
            {
                Push((byte)((PC >> 8) & 0xFF));
                Push((byte)(PC & 0xFF));
                //P寄存器push到堆栈的时候，需要把4和5位都置1，理由暂不明
                byte p = P.Value;
                p = BitService.SetBit(p, 4);
                p = BitService.SetBit(p, 5);
                Push(p);

                P.BreakCommand = 1;
                P.InterruptDisable = 1;
                Cycles += 7;

                PC = ReadWord(0xFFFE);
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// If the overflow flag is clear then add the relative displacement to the program counter to cause a branch to a new location.
        /// </summary>
        /// <param name="opCode"></param>
        private void BVC(byte opCode)
        {
            if (opCode == 0x50)
            {
                ushort offset = Relative();
                Cycles += 2;
                if (P.OverflowFlag == 0)
                {
                    Cycles += 1;

                    ushort newAddr = (ushort)(PC + offset);
                    if (IsCrossPage(newAddr, PC))
                    {
                        Cycles += 1;
                    }
                    PC = newAddr;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// If the overflow flag is set then add the relative displacement to the program counter to cause a branch to a new location.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void BVS(byte opCode)
        {
            if (opCode == 0x70)
            {
                ushort offset = Relative();
                Cycles += 2;
                if (P.OverflowFlag == 1)
                {
                    Cycles += 1;

                    ushort newAddr = (ushort)(PC + (sbyte)offset);
                    if (IsCrossPage(newAddr, PC))
                    {
                        Cycles += 1;
                    }
                    PC = newAddr;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Set the carry flag to zero.
        /// </summary>
        /// <param name="opCode"></param>
        private void CLC(byte opCode)
        {
            if (opCode == 0x18)
            {
                P.CarryFlag = 0;
                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Sets the decimal mode flag to zero.
        /// </summary>
        /// <param name="opCode"></param>
        private void CLD(byte opCode)
        {
            if (opCode == 0xD8)
            {
                P.DecimalMode = 0;
                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Clears the interrupt disable flag allowing normal interrupt requests to be serviced.
        /// </summary>
        /// <param name="opCode"></param>
        private void CLI(byte opCode)
        {
            if (opCode == 0x58)
            {
                P.InterruptDisable = 0;
                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Clears the overflow flag.
        /// </summary>
        /// <param name="opCode"></param>
        private void CLV(byte opCode)
        {
            if (opCode == 0xB8)
            {
                P.OverflowFlag = 0;
                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Z,C,N = A-M
        /// This instruction compares the contents of the accumulator with another memory held value and sets the zero and carry flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        private void CMP(byte opCode)
        {
            ushort addr;
            if (opCode == 0xC9)
            {
                addr = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0xC5)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0xD5)
            {
                addr = ZeroPageX();
                Cycles += 4;
            }
            else if (opCode == 0xCD)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else if (opCode == 0xDD)
            {
                addr = AbsoluteX(out bool crossPage);
                Cycles += 4;

                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else if (opCode == 0xD9)
            {
                addr = AbsoluteY(out bool crossPage);
                Cycles += 4;

                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else if (opCode == 0xC1)
            {
                addr = IndexedIndirect();
                Cycles += 6;
            }
            else if (opCode == 0xD1)
            {
                addr = IndirectIndexed(out bool crossPage);
                Cycles += 5;

                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            byte diff = (byte)(A - data);

            P.CarryFlag = BoolToBit(A >= data);
            P.ZeroFlag = BoolToBit(A == data);
            P.NegativeFlag = BitService.GetBit(diff, 7);
        }

        /// <summary>
        /// Z,C,N = X-M
        /// This instruction compares the contents of the X register with another memory held value and sets the zero and carry flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void CPX(byte opCode)
        {
            ushort addr;
            if (opCode == 0xE0)
            {
                addr = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0xE4)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0xEC)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            byte diff = (byte)(X - data);

            P.CarryFlag = BoolToBit(X >= data);
            P.ZeroFlag = BoolToBit(X == data);
            P.NegativeFlag = BitService.GetBit(diff, 7);
        }

        /// <summary>
        /// Z,C,N = Y-M
        /// This instruction compares the contents of the Y register with another memory held value and sets the zero and carry flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void CPY(byte opCode)
        {
            ushort addr;
            if (opCode == 0xC0)
            {
                addr = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0xC4)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0xCC)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            byte diff = (byte)(Y - data);

            P.CarryFlag = BoolToBit(Y >= data);
            P.ZeroFlag = BoolToBit(Y == data);
            P.NegativeFlag = BitService.GetBit(diff, 7);
        }

        /// <summary>
        /// M,Z,N = M-1
        /// Subtracts one from the value held at a specified memory location setting the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        private void DEC(byte opCode)
        {
            ushort addr;
            if (opCode == 0xC6)
            {
                addr = ZeroPage();
                Cycles += 5;
            }
            else if (opCode == 0xD6)
            {
                addr = ZeroPageX();
                Cycles += 6;
            }
            else if (opCode == 0xCE)
            {
                addr = Absolute();
                Cycles += 6;
            }
            else if (opCode == 0xDE)
            {
                addr = AbsoluteX(out _);
                Cycles += 7;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            data -= 1;
            m_CPUBus.WriteByte(addr, data);

            P.ZeroFlag = BoolToBit(data == 0);
            P.NegativeFlag = BitService.GetBit(data, 7);
        }

        /// <summary>
        /// X,Z,N = X-1
        /// Subtracts one from the X register setting the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void DEX(byte opCode)
        {
            if (opCode == 0xCA)
            {
                X -= 1;

                P.ZeroFlag = BoolToBit(X == 0);
                P.NegativeFlag = BitService.GetBit(X, 7);
                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// X,Z,N = X-1
        /// Subtracts one from the X register setting the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void DEY(byte opCode)
        {
            if (opCode == 0x88)
            {
                Y -= 1;

                P.ZeroFlag = BoolToBit(Y == 0);
                P.NegativeFlag = BitService.GetBit(Y, 7);
                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// A,Z,N = A^M
        /// An exclusive OR is performed, bit by bit, on the accumulator contents using the contents of a byte of memory.
        /// </summary>
        /// <param name="opCode"></param>
        private void EOR(byte opCode)
        {
            ushort addr;
            if (opCode == 0x49)
            {
                addr = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0x45)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0x55)
            {
                addr = ZeroPageX();
                Cycles += 4;
            }
            else if (opCode == 0x4D)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else if (opCode == 0x5D)
            {
                addr = AbsoluteX(out bool crossPage);
                Cycles += 4;
                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else if (opCode == 0x59)
            {
                addr = AbsoluteY(out bool crossPage);
                Cycles += 4;
                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else if (opCode == 0x41)
            {
                addr = IndexedIndirect();
                Cycles += 6;
            }
            else if (opCode == 0x51)
            {
                addr = IndirectIndexed(out bool crossPage);
                Cycles += 5;
                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            A ^= data;
            P.ZeroFlag = BoolToBit(A == 0);
            P.NegativeFlag = BitService.GetBit(A, 7);
        }

        /// <summary>
        /// M,Z,N = M+1
        /// Adds one to the value held at a specified memory location setting the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        private void INC(byte opCode)
        {
            ushort addr;
            if (opCode == 0xE6)
            {
                addr = ZeroPage();
                Cycles += 5;
            }
            else if (opCode == 0xF6)
            {
                addr = ZeroPageX();
                Cycles += 6;
            }
            else if (opCode == 0xEE)
            {
                addr = Absolute();
                Cycles += 6;
            }
            else if (opCode == 0xFE)
            {
                addr = AbsoluteX(out _);
                Cycles += 7;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            data += 1;
            m_CPUBus.WriteByte(addr, data);

            P.ZeroFlag = BoolToBit(data == 0);
            P.NegativeFlag = BitService.GetBit(data, 7);
        }

        /// <summary>
        /// X,Z,N = X+1
        /// Adds one to the X register setting the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        private void INX(byte opCode)
        {
            if (opCode == 0xE8)
            {
                X += 1;

                P.ZeroFlag = BoolToBit(X == 0);
                P.NegativeFlag = BitService.GetBit(X, 7);
                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Y,Z,N = Y+1
        /// Adds one to the Y register setting the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        private void INY(byte opCode)
        {
            if (opCode == 0xC8)
            {
                Y += 1;

                P.ZeroFlag = BoolToBit(Y == 0);
                P.NegativeFlag = BitService.GetBit(Y, 7);
                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Sets the program counter to the address specified by the operand.
        /// </summary>
        /// <param name="opCode"></param>
        private void JMP(byte opCode)
        {
            ushort addr;
            if (opCode == 0x4C)
            {
                addr = Absolute();
                Cycles += 3;
            }
            else if (opCode == 0x6C)
            {
                addr = Indirect();
                Cycles += 5;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            PC = addr;
        }

        /// <summary>
        /// The JSR instruction pushes the address (minus one) of the return point on to the stack and then sets the program counter to the target memory address.
        /// </summary>
        /// <param name="opCode"></param>
        private void JSR(byte opCode)
        {
            if (opCode == 0x20)
            {
                ushort addr = Absolute();
                ushort returnAddr = (ushort)(PC - 1);
                Push((byte)((returnAddr >> 8) & 0xFF));
                Push((byte)(returnAddr & 0xFF));

                PC = addr;
                Cycles += 6;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// A,Z,N = M
        /// Loads a byte of memory into the accumulator setting the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void LDA(byte opCode)
        {
            ushort addr;
            if (opCode == 0xA9)
            {
                addr = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0xA5)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0xB5)
            {
                addr = ZeroPageX();
                Cycles += 4;
            }
            else if (opCode == 0xAD)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else if (opCode == 0xBD)
            {
                addr = AbsoluteX(out bool crossPage);
                Cycles += 4;

                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else if (opCode == 0xB9)
            {
                addr = AbsoluteY(out bool crossPage);
                Cycles += 4;

                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else if (opCode == 0xA1)
            {
                addr = IndexedIndirect();
                Cycles += 6;
            }
            else if (opCode == 0xB1)
            {
                addr = IndirectIndexed(out bool crossPage);
                Cycles += 5;

                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            A = data;
            P.ZeroFlag = BoolToBit(A == 0);
            P.NegativeFlag = BitService.GetBit(A, 7);
        }

        /// <summary>
        /// X,Z,N = M
        /// Loads a byte of memory into the X register setting the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void LDX(byte opCode)
        {
            ushort addr;
            if (opCode == 0xA2)
            {
                addr = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0xA6)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0xB6)
            {
                addr = ZeroPageY();
                Cycles += 4;
            }
            else if (opCode == 0xAE)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else if (opCode == 0xBE)
            {
                addr = AbsoluteY(out bool crossPage);
                Cycles += 4;

                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            X = data;
            P.ZeroFlag = BoolToBit(X == 0);
            P.NegativeFlag = BitService.GetBit(X, 7);
        }

        /// <summary>
        /// Y,Z,N = M
        /// Loads a byte of memory into the Y register setting the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void LDY(byte opCode)
        {
            ushort addr;
            if (opCode == 0xA0)
            {
                addr = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0xA4)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0xB4)
            {
                addr = ZeroPageX();
                Cycles += 4;
            }
            else if (opCode == 0xAC)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else if (opCode == 0xBC)
            {
                addr = AbsoluteX(out bool crossPage);
                Cycles += 4;

                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            Y = data;
            P.ZeroFlag = BoolToBit(Y == 0);
            P.NegativeFlag = BitService.GetBit(Y, 7);
        }

        /// <summary>
        /// A,C,Z,N = A/2 or M,C,Z,N = M/2
        /// Each of the bits in A or M is shift one place to the right.The bit that was in bit 0 is shifted into the carry flag.Bit 7 is set to zero.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void LSR(byte opCode)
        {
            ushort addr;
            if (opCode == 0x4A)
            {
                P.CarryFlag = BitService.GetBit(A, 0);
                A = (byte)(A >> 1);

                P.ZeroFlag = BoolToBit(A == 0);
                P.NegativeFlag = BitService.GetBit(A, 7);

                Cycles += 2;
                return;
            }
            else if (opCode == 0x46)
            {
                addr = ZeroPage();
                Cycles += 5;
            }
            else if (opCode == 0x56)
            {
                addr = ZeroPageX();
                Cycles += 6;
            }
            else if (opCode == 0x4E)
            {
                addr = Absolute();
                Cycles += 6;
            }
            else if (opCode == 0x5E)
            {
                addr = AbsoluteX(out _);
                Cycles += 7;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            P.CarryFlag = BitService.GetBit(data, 0);
            data = (byte)(data >> 1);
            m_CPUBus.WriteByte(addr, data);

            P.ZeroFlag = BoolToBit(data == 0);
            P.NegativeFlag = BitService.GetBit(data, 7);
        }

        /// <summary>
        /// The NOP instruction causes no changes to the processor other than the normal incrementing of the program counter to the next instruction.
        /// </summary>
        /// <param name="opCode"></param>
        private void NOP(byte opCode)
        {
            if (opCode == 0xEA)
            {
                //do nothing
                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// A,Z,N = A|M
        /// An inclusive OR is performed, bit by bit, on the accumulator contents using the contents of a byte of memory.
        /// </summary>
        /// <param name="opCode"></param>
        private void ORA(byte opCode)
        {
            ushort addr;
            if (opCode == 0x09)
            {
                addr = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0x05)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0x15)
            {
                addr = ZeroPageX();
                Cycles += 4;
            }
            else if (opCode == 0x0D)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else if (opCode == 0x1D)
            {
                addr = AbsoluteX(out bool crossPage);
                Cycles += 4;

                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else if (opCode == 0x19)
            {
                addr = AbsoluteY(out bool crossPage);
                Cycles += 4;

                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else if (opCode == 0x01)
            {
                addr = IndexedIndirect();
                Cycles += 6;
            }
            else if (opCode == 0x11)
            {
                addr = IndirectIndexed(out bool crossPage);
                Cycles += 5;

                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            A |= data;

            P.ZeroFlag = BoolToBit(A == 0);
            P.NegativeFlag = BitService.GetBit(A, 7);
        }

        /// <summary>
        /// Pushes a copy of the accumulator on to the stack.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void PHA(byte opCode)
        {
            if (opCode == 0x48)
            {
                Push(A);
                Cycles += 3;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Pushes a copy of the status flags on to the stack.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void PHP(byte opCode)
        {
            if (opCode == 0x08)
            {
                //P寄存器push到堆栈的时候，需要把4和5位都置1，理由暂不明
                byte p = P.Value;
                p = BitService.SetBit(p, 4);
                p = BitService.SetBit(p, 5);
                Push(p);
                Cycles += 3;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Pulls an 8 bit value from the stack and into the accumulator. The zero and negative flags are set as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        private void PLA(byte opCode)
        {
            if (opCode == 0x68)
            {
                A = Pop();
                P.ZeroFlag = BoolToBit(A == 0);
                P.NegativeFlag = BitService.GetBit(A, 7);
                Cycles += 4;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Pulls an 8 bit value from the stack and into the processor flags. The flags will take on new states as determined by the value pulled
        /// </summary>
        /// <param name="opCode"></param>
        private void PLP(byte opCode)
        {
            if (opCode == 0x28)
            {
                byte p = Pop();
                p = BitService.SetBit(p, 5);
                p = BitService.ClearBit(p, 4);
                P.SetValue(p);
                Cycles += 4;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Move each of the bits in either A or M one place to the left. Bit 0 is filled with the current value of the carry flag whilst the old bit 7 becomes the new carry flag value.
        /// </summary>
        /// <param name="opCode"></param>
        private void ROL(byte opCode)
        {
            ushort addr;
            if (opCode == 0x2A)
            {
                byte oldCarryFlag = P.CarryFlag;
                byte newCarryFlag = BitService.GetBit(A, 7);
                A <<= 1;
                if (oldCarryFlag == 1)
                {
                    A = BitService.SetBit(A, 0);
                }
                P.CarryFlag = newCarryFlag;
                P.ZeroFlag = BoolToBit(A == 0);
                P.NegativeFlag = BitService.GetBit(A, 7);
                Cycles += 2;
                return;
            }
            else if (opCode == 0x26)
            {
                addr = ZeroPage();
                Cycles += 5;
            }
            else if (opCode == 0x36)
            {
                addr = ZeroPageX();
                Cycles += 6;
            }
            else if (opCode == 0x2E)
            {
                addr = Absolute();
                Cycles += 6;
            }
            else if (opCode == 0x3E)
            {
                addr = AbsoluteX(out _);
                Cycles += 7;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            {
                byte data = m_CPUBus.ReadByte(addr);

                byte oldCarryFlag = P.CarryFlag;
                byte newCarryFlag = BitService.GetBit(data, 7);
                data <<= 1;
                if (oldCarryFlag == 1)
                {
                    data = BitService.SetBit(data, 0);
                }
                P.CarryFlag = newCarryFlag;
                P.ZeroFlag = BoolToBit(data == 0);
                P.NegativeFlag = BitService.GetBit(data, 7);
                m_CPUBus.WriteByte(addr, data);
            }
        }

        /// <summary>
        /// Move each of the bits in either A or M one place to the right. Bit 7 is filled with the current value of the carry flag whilst the old bit 0 becomes the new carry flag value.
        /// </summary>
        /// <param name="opCode"></param>
        private void ROR(byte opCode)
        {
            ushort addr;
            if (opCode == 0x6A)
            {
                byte oldCarryFlag = P.CarryFlag;
                byte newCarryFlag = BitService.GetBit(A, 0);
                A >>= 1;
                if (oldCarryFlag == 1)
                {
                    A = BitService.SetBit(A, 7);
                }
                P.CarryFlag = newCarryFlag;
                P.ZeroFlag = BoolToBit(A == 0);
                P.NegativeFlag = BitService.GetBit(A, 7);
                Cycles += 2;
                return;
            }
            else if (opCode == 0x66)
            {
                addr = ZeroPage();
                Cycles += 5;
            }
            else if (opCode == 0x76)
            {
                addr = ZeroPageX();
                Cycles += 6;
            }
            else if (opCode == 0x6E)
            {
                addr = Absolute();
                Cycles += 6;
            }
            else if (opCode == 0x7E)
            {
                addr = AbsoluteX(out _);
                Cycles += 7;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            {
                byte data = m_CPUBus.ReadByte(addr);

                byte oldCarryFlag = P.CarryFlag;
                byte newCarryFlag = BitService.GetBit(data, 0);
                data >>= 1;
                if (oldCarryFlag == 1)
                {
                    data = BitService.SetBit(data, 7);
                }
                P.CarryFlag = newCarryFlag;
                P.ZeroFlag = BoolToBit(data == 0);
                P.NegativeFlag = BitService.GetBit(data, 7);
                m_CPUBus.WriteByte(addr, data);
            }
        }

        /// <summary>
        /// The RTI instruction is used at the end of an interrupt processing routine. It pulls the processor flags from the stack followed by the program counter.
        /// </summary>
        /// <param name="opCode"></param>
        private void RTI(byte opCode)
        {
            if (opCode == 0x40)
            {
                byte p = Pop();
                p = BitService.SetBit(p, 5);
                p = BitService.ClearBit(p, 4);
                P.SetValue(p);
                byte low = Pop();
                byte high = Pop();
                PC = (ushort)(high << 8 | low);

                Cycles += 6;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// The RTS instruction is used at the end of a subroutine to return to the calling routine. It pulls the program counter (minus one) from the stack.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void RTS(byte opCode)
        {
            if (opCode == 0x60)
            {
                byte low = Pop();
                byte high = Pop();
                PC = (ushort)(high << 8 | low);
                ++PC;

                Cycles += 6;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// A,Z,C,N = A-M-(1-C)
        /// This instruction subtracts the contents of a memory location to the accumulator together with the not of the carry bit.
        /// If overflow occurs the carry bit is clear, this enables multiple byte subtraction to be performed.
        /// </summary>
        /// <param name="opCode"></param>
        private void SBC(byte opCode)
        {
            ushort addr;
            if (opCode == 0xE9)
            {
                addr = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0xE5)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0xF5)
            {
                addr = ZeroPageX();
                Cycles += 4;
            }
            else if (opCode == 0xED)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else if (opCode == 0xFD)
            {
                addr = AbsoluteX(out bool crossPage);
                Cycles += 4;
                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else if (opCode == 0xF9)
            {
                addr = AbsoluteY(out bool crossPage);
                Cycles += 4;
                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else if (opCode == 0xE1)
            {
                addr = IndexedIndirect();
                Cycles += 6;
            }
            else if (opCode == 0xF1)
            {
                addr = IndirectIndexed(out bool crossPage);
                Cycles += 5;
                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            byte data = m_CPUBus.ReadByte(addr);
            byte oldA = A;
            short carryValue = (short)(A - data - (1 - P.CarryFlag));
            A = (byte)(A - data - (1 - P.CarryFlag));

            P.ZeroFlag = BoolToBit(A == 0);
            P.NegativeFlag = BitService.GetBit(A, 7);
            P.OverflowFlag = BoolToBit(CheckSubOverflowPresent(oldA, data, (byte)(1 - P.CarryFlag)));
            P.CarryFlag = BoolToBit(carryValue >= 0);
        }

        /// <summary>
        /// Set the carry flag to one.
        /// </summary>
        /// <param name="opCode"></param>
        private void SEC(byte opCode)
        {
            if (opCode == 0x38)
            {
                P.CarryFlag = 1;
                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Set the decimal mode flag to one.
        /// </summary>
        /// <param name="opCode"></param>
        private void SED(byte opCode)
        {
            if (opCode == 0xF8)
            {
                P.DecimalMode = 1;
                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Set the interrupt disable flag to one.
        /// </summary>
        /// <param name="opCode"></param>
        private void SEI(byte opCode)
        {
            if (opCode == 0x78)
            {
                P.InterruptDisable = 1;
                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// M = A
        /// Stores the contents of the accumulator into memory.
        /// </summary>
        /// <param name="opCode"></param>
        private void STA(byte opCode)
        {
            ushort addr;
            if (opCode == 0x85)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0x95)
            {
                addr = ZeroPageX();
                Cycles += 4;
            }
            else if (opCode == 0x8D)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else if (opCode == 0x9D)
            {
                addr = AbsoluteX(out _);
                Cycles += 5;
            }
            else if (opCode == 0x99)
            {
                addr = AbsoluteY(out _);
                Cycles += 5;
            }
            else if (opCode == 0x81)
            {
                addr = IndexedIndirect();
                Cycles += 6;
            }
            else if (opCode == 0x91)
            {
                addr = IndirectIndexed(out _);
                Cycles += 6;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            m_CPUBus.WriteByte(addr, A);
        }

        /// <summary>
        /// M = X
        /// Stores the contents of the X register into memory.
        /// </summary>
        /// <param name="opCode"></param>
        private void STX(byte opCode)
        {
            ushort addr;
            if (opCode == 0x86)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0x96)
            {
                addr = ZeroPageY();
                Cycles += 4;
            }
            else if (opCode == 0x8E)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            m_CPUBus.WriteByte(addr, X);
        }

        /// <summary>
        /// M = Y
        /// Stores the contents of the Y register into memory.
        /// </summary>
        /// <param name="opCode"></param>
        private void STY(byte opCode)
        {
            ushort addr;
            if (opCode == 0x84)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0x94)
            {
                addr = ZeroPageX();
                Cycles += 4;
            }
            else if (opCode == 0x8C)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            m_CPUBus.WriteByte(addr, Y);
        }

        /// <summary>
        /// X = A
        /// Copies the current contents of the accumulator into the X register and sets the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        private void TAX(byte opCode)
        {
            if (opCode == 0xAA)
            {
                X = A;
                P.ZeroFlag = BoolToBit(X == 0);
                P.NegativeFlag = BitService.GetBit(X, 7);

                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Y = A
        /// Copies the current contents of the accumulator into the Y register and sets the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        private void TAY(byte opCode)
        {
            if (opCode == 0xA8)
            {
                Y = A;
                P.ZeroFlag = BoolToBit(Y == 0);
                P.NegativeFlag = BitService.GetBit(Y, 7);

                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// X = S
        /// Copies the current contents of the stack register into the X register and sets the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        /// <exception cref="Exception"></exception>
        private void TSX(byte opCode)
        {
            if (opCode == 0xBA)
            {
                X = SP;
                P.ZeroFlag = BoolToBit(X == 0);
                P.NegativeFlag = BitService.GetBit(X, 7);

                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// A = X
        /// Copies the current contents of the X register into the accumulator and sets the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        private void TXA(byte opCode)
        {
            if (opCode == 0x8A)
            {
                A = X;
                P.ZeroFlag = BoolToBit(A == 0);
                P.NegativeFlag = BitService.GetBit(A, 7);

                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// S = X
        /// Copies the current contents of the X register into the stack register.
        /// </summary>
        /// <param name="opCode"></param>
        private void TXS(byte opCode)
        {
            if (opCode == 0x9A)
            {
                SP = X;

                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// A = Y
        /// Copies the current contents of the Y register into the accumulator and sets the zero and negative flags as appropriate.
        /// </summary>
        /// <param name="opCode"></param>
        private void TYA(byte opCode)
        {
            if (opCode == 0x98)
            {
                A = Y;
                P.ZeroFlag = BoolToBit(A == 0);
                P.NegativeFlag = BitService.GetBit(A, 7);

                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// do nothing
        /// </summary>
        /// <param name="opCode"></param>
        private void UNOFFICAL_NOP(byte opCode)
        {
            //参考*3)，指令时间参考nestest.log
            if (opCode == 0x80)
            {
                _ = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0x04 || opCode == 0x44 || opCode == 0x64)
            {
                _ = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0x0C)
            {
                _ = Absolute();
                Cycles += 4;
            }
            else if (opCode == 0x14 || opCode == 0x34 || opCode == 0x54 || opCode == 0x74 || opCode == 0xD4 || opCode == 0xF4)
            {
                _ = ZeroPageX();
                Cycles += 4;
            }
            else if (opCode == 0x1C || opCode == 0x3C || opCode == 0x5C || opCode == 0x7C || opCode == 0xDC || opCode == 0xFC)
            {
                _ = AbsoluteX(out bool crossPage);
                Cycles += 4;
                if (crossPage)
                {
                    Cycles += 1;
                }
            }
            else if (opCode == 0x89)
            {
                _ = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0x82 || opCode == 0xC2 || opCode == 0xE2)
            {
                _ = Immediate();
                Cycles += 2;
            }
            else if (opCode == 0x1A || opCode == 0x3A || opCode == 0x5A || opCode == 0x7A || opCode == 0xDA || opCode == 0xFA)
            {
                Cycles += 2;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Shortcut for LDA value then TAX.参考4*)
        /// </summary>
        /// <param name="opCode"></param>
        private void UNOFFICAL_LAX(byte opCode)
        {
            long tmpCycle = Cycles; //因为这里的cycle不是简单的指令里面的相加，所以这里重新计算

            if (opCode == 0xA3)
            {
                LDA(0xA1);
                tmpCycle += 6;
            }
            else if (opCode == 0xA7)
            {
                LDA(0xA5);
                tmpCycle += 3;
            }
            else if (opCode == 0xAB)
            {
                LDA(0xA9);
                tmpCycle += 2;
            }
            else if (opCode == 0xAF)
            {
                LDA(0xAD);
                tmpCycle += 4;
            }
            else if (opCode == 0xB3)
            {
                LDA(0xB1);
                tmpCycle += 5;
            }
            else if (opCode == 0xB7)
            {
                //LDA没有zeroPageY模式，所以放这里实现
                ushort addr = ZeroPageY();
                tmpCycle += 4;

                byte data = m_CPUBus.ReadByte(addr);
                A = data;
                P.ZeroFlag = BoolToBit(A == 0);
                P.NegativeFlag = BitService.GetBit(A, 7);
            }
            else if (opCode == 0xBF)
            {
                LDA(0xB9);
                tmpCycle += 4;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            TAX(0xAA);

            Cycles = tmpCycle;
        }

        /// <summary>
        /// Stores the bitwise AND of A and X. As with STA and STX, no flags are affected.
        /// </summary>
        /// <param name="opCode"></param>
        private void UNOFFICAL_SAX(byte opCode)
        {
            ushort addr;
            if (opCode == 0x83)
            {
                addr = IndexedIndirect();
                Cycles += 6;
            }
            else if (opCode == 0x87)
            {
                addr = ZeroPage();
                Cycles += 3;
            }
            else if (opCode == 0x8F)
            {
                addr = Absolute();
                Cycles += 4;
            }
            else if (opCode == 0x97)
            {
                addr = ZeroPageY();
                Cycles += 4;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            m_CPUBus.WriteByte(addr, (byte)(A & X));
        }

        /// <summary>
        /// 与SBC immediate完全一致
        /// </summary>
        /// <param name="opCode"></param>
        private void UNOFFICAL_SBC(byte opCode)
        {
            if (opCode == 0xEB)
            {
                SBC(0xE9);
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }
        }

        /// <summary>
        /// Equivalent to DEC value then CMP value, except supporting more addressing modes. 
        /// LDA #$FF followed by DCP can be used to check if the decrement underflows, which is useful for multi-byte decrements.
        /// </summary>
        /// <param name="opCode"></param>
        private void UNOFFICAL_DCP(byte opCode)
        {
            ushort addr;
            long tmpCycle = Cycles;
            if (opCode == 0xC3)
            {
                addr = IndexedIndirect();
                tmpCycle += 8;
            }
            else if (opCode == 0xC7)
            {
                addr = ZeroPage();
                tmpCycle += 5;
            }
            else if (opCode == 0xCF)
            {
                addr = Absolute();
                tmpCycle += 6;
            }
            else if (opCode == 0xD3)
            {
                addr = IndirectIndexed(out _);
                tmpCycle += 8;
            }
            else if (opCode == 0xD7)
            {
                addr = ZeroPageX();
                tmpCycle += 6;
            }
            else if (opCode == 0xDB)
            {
                addr = AbsoluteY(out _);
                tmpCycle += 7;
            }
            else if (opCode == 0xDF)
            {
                addr = AbsoluteX(out _);
                tmpCycle += 7;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            //DEC
            byte data = m_CPUBus.ReadByte(addr);
            --data;
            m_CPUBus.WriteByte(addr, data);

            //CMP
            byte diff = (byte)(A - data);

            P.CarryFlag = BoolToBit(A >= data);
            P.ZeroFlag = BoolToBit(A == data);
            P.NegativeFlag = BitService.GetBit(diff, 7);

            Cycles = tmpCycle;
        }

        /// <summary>
        /// Equivalent to INC value then SBC value, except supporting more addressing modes.
        /// </summary>
        /// <param name="opCode"></param>
        private void UNOFFICAL_ISCorISB(byte opCode)
        {
            ushort addr;
            long tmpCycle = Cycles;
            if (opCode == 0xE3)
            {
                addr = IndexedIndirect();
                tmpCycle += 8;
            }
            else if (opCode == 0xE7)
            {
                addr = ZeroPage();
                tmpCycle += 5;
            }
            else if (opCode == 0xEF)
            {
                addr = Absolute();
                tmpCycle += 6;
            }
            else if (opCode == 0xF3)
            {
                addr = IndirectIndexed(out _);
                tmpCycle += 8;
            }
            else if (opCode == 0xF7)
            {
                addr = ZeroPageX();
                tmpCycle += 6;
            }
            else if (opCode == 0xFB)
            {
                addr = AbsoluteY(out _);
                tmpCycle += 7;
            }
            else if (opCode == 0xFF)
            {
                addr = AbsoluteX(out _);
                tmpCycle += 7;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            //INC
            byte data = m_CPUBus.ReadByte(addr);
            ++data;
            m_CPUBus.WriteByte(addr, data);

            //SBC
            byte oldA = A;
            short carryValue = (short)(A - data - (1 - P.CarryFlag));
            A = (byte)(A - data - (1 - P.CarryFlag));

            P.ZeroFlag = BoolToBit(A == 0);
            P.NegativeFlag = BitService.GetBit(A, 7);
            P.OverflowFlag = BoolToBit(CheckSubOverflowPresent(oldA, data, (byte)(1 - P.CarryFlag)));
            P.CarryFlag = BoolToBit(carryValue >= 0);

            Cycles = tmpCycle;
        }

        /// <summary>
        /// Equivalent to ASL value then ORA value, except supporting more addressing modes. LDA #0 followed by SLO is an efficient way to shift a variable while also loading it in A.
        /// </summary>
        /// <param name="opCode"></param>
        private void UNOFFICAL_SLO(byte opCode)
        {
            ushort addr;
            long tmpCycle = Cycles;
            if (opCode == 0x03)
            {
                addr = IndexedIndirect();
                tmpCycle += 8;
            }
            else if (opCode == 0x07)
            {
                addr = ZeroPage();
                tmpCycle += 5;
            }
            else if (opCode == 0x0F)
            {
                addr = Absolute();
                tmpCycle += 6;
            }
            else if (opCode == 0x13)
            {
                addr = IndirectIndexed(out _);
                tmpCycle += 8;
            }
            else if (opCode == 0x17)
            {
                addr = ZeroPageX();
                tmpCycle += 6;
            }
            else if (opCode == 0x1B)
            {
                addr = AbsoluteY(out _);
                tmpCycle += 7;
            }
            else if (opCode == 0x1F)
            {
                addr = AbsoluteX(out _);
                tmpCycle += 7;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            //ASL
            byte data = m_CPUBus.ReadByte(addr);
            byte newCarryFlag = BitService.GetBit(data, 7);
            data <<= 1;
            m_CPUBus.WriteByte(addr, data);

            P.ZeroFlag = BoolToBit(data == 0);
            P.NegativeFlag = BitService.GetBit(data, 7);
            P.CarryFlag = newCarryFlag;

            //ORA
            A |= data;

            P.ZeroFlag = BoolToBit(A == 0);
            P.NegativeFlag = BitService.GetBit(A, 7);

            Cycles = tmpCycle;
        }

        /// <summary>
        /// Equivalent to ROL value then AND value, except supporting more addressing modes. LDA #$FF followed by RLA is an efficient way to rotate a variable while also loading it in A.
        /// </summary>
        /// <param name="opCode"></param>
        private void UNOFFICAL_RLA(byte opCode)
        {
            ushort addr;
            long tmpCycle = Cycles;
            if (opCode == 0x23)
            {
                addr = IndexedIndirect();
                tmpCycle += 8;
            }
            else if (opCode == 0x27)
            {
                addr = ZeroPage();
                tmpCycle += 5;
            }
            else if (opCode == 0x2F)
            {
                addr = Absolute();
                tmpCycle += 6;
            }
            else if (opCode == 0x33)
            {
                addr = IndirectIndexed(out _);
                tmpCycle += 8;
            }
            else if (opCode == 0x37)
            {
                addr = ZeroPageX();
                tmpCycle += 6;
            }
            else if (opCode == 0x3B)
            {
                addr = AbsoluteY(out _);
                tmpCycle += 7;
            }
            else if (opCode == 0x3F)
            {
                addr = AbsoluteX(out _);
                tmpCycle += 7;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            //ROL
            byte data = m_CPUBus.ReadByte(addr);
            byte oldCarryFlag = P.CarryFlag;
            byte newCarryFlag = BitService.GetBit(data, 7);
            data <<= 1;
            if (oldCarryFlag == 1)
            {
                data = BitService.SetBit(data, 0);
            }
            m_CPUBus.WriteByte(addr, data);

            P.CarryFlag = newCarryFlag;
            P.ZeroFlag = BoolToBit(data == 0);
            P.NegativeFlag = BitService.GetBit(data, 7);
            //AND
            A = (byte)(A & data);
            P.ZeroFlag = BoolToBit(A == 0);
            P.NegativeFlag = BitService.GetBit(A, 7);

            Cycles = tmpCycle;
        }

        /// <summary>
        /// Equivalent to LSR value then EOR value, except supporting more addressing modes. LDA #0 followed by SRE is an efficient way to shift a variable while also loading it in A.
        /// </summary>
        /// <param name="opCode"></param>
        private void UNOFFICAL_SRE(byte opCode)
        {
            ushort addr;
            long tmpCycle = Cycles;
            if (opCode == 0x43)
            {
                addr = IndexedIndirect();
                tmpCycle += 8;
            }
            else if (opCode == 0x47)
            {
                addr = ZeroPage();
                tmpCycle += 5;
            }
            else if (opCode == 0x4F)
            {
                addr = Absolute();
                tmpCycle += 6;
            }
            else if (opCode == 0x53)
            {
                addr = IndirectIndexed(out _);
                tmpCycle += 8;
            }
            else if (opCode == 0x57)
            {
                addr = ZeroPageX();
                tmpCycle += 6;
            }
            else if (opCode == 0x5B)
            {
                addr = AbsoluteY(out _);
                tmpCycle += 7;
            }
            else if (opCode == 0x5F)
            {
                addr = AbsoluteX(out _);
                tmpCycle += 7;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            //LSR
            byte data = m_CPUBus.ReadByte(addr);
            P.CarryFlag = BitService.GetBit(data, 0);
            data = (byte)(data >> 1);
            m_CPUBus.WriteByte(addr, data);

            P.ZeroFlag = BoolToBit(data == 0);
            P.NegativeFlag = BitService.GetBit(data, 7);

            //EOR
            A ^= data;
            P.ZeroFlag = BoolToBit(A == 0);
            P.NegativeFlag = BitService.GetBit(A, 7);

            Cycles = tmpCycle;
        }

        /// <summary>
        /// Equivalent to ROR value then ADC value, except supporting more addressing modes. Essentially this computes A + value / 2, where value is 9-bit and the division is rounded up.
        /// </summary>
        /// <param name="opCode"></param>
        private void UNOFFICAL_RRA(byte opCode)
        {
            ushort addr;
            long tmpCycle = Cycles;
            if (opCode == 0x63)
            {
                addr = IndexedIndirect();
                tmpCycle += 8;
            }
            else if (opCode == 0x67)
            {
                addr = ZeroPage();
                tmpCycle += 5;
            }
            else if (opCode == 0x6F)
            {
                addr = Absolute();
                tmpCycle += 6;
            }
            else if (opCode == 0x73)
            {
                addr = IndirectIndexed(out _);
                tmpCycle += 8;
            }
            else if (opCode == 0x77)
            {
                addr = ZeroPageX();
                tmpCycle += 6;
            }
            else if (opCode == 0x7B)
            {
                addr = AbsoluteY(out _);
                tmpCycle += 7;
            }
            else if (opCode == 0x7F)
            {
                addr = AbsoluteX(out _);
                tmpCycle += 7;
            }
            else
            {
                throw new Exception($"不支持的opCode,{opCode:X2}");
            }

            //ROR
            byte data = m_CPUBus.ReadByte(addr);

            byte oldCarryFlag = P.CarryFlag;
            byte newCarryFlag = BitService.GetBit(data, 0);
            data >>= 1;
            if (oldCarryFlag == 1)
            {
                data = BitService.SetBit(data, 7);
            }
            m_CPUBus.WriteByte(addr, data);
            P.CarryFlag = newCarryFlag;
            P.ZeroFlag = BoolToBit(data == 0);
            P.NegativeFlag = BitService.GetBit(data, 7);

            //ADC
            byte oldA = A;
            ushort carryValue = (ushort)(A + data + P.CarryFlag);
            A = (byte)(A + data + P.CarryFlag);

            //标记的参考资料的2*
            P.ZeroFlag = BoolToBit(A == 0);
            P.NegativeFlag = BitService.GetBit(A, 7);
            P.OverflowFlag = BoolToBit(CheckAddOverflowPresent(oldA, data, P.CarryFlag));
            P.CarryFlag = BoolToBit((carryValue >> 8) != 0);

            Cycles = tmpCycle;
        }

        /// <summary>
        /// immediate寻址，返回地址
        /// </summary>
        /// <returns></returns>
        private ushort Immediate()
        {
            return PC++;
        }

        /// <summary>
        /// zero page寻址，返回地址
        /// </summary>
        /// <returns></returns>
        private ushort ZeroPage()
        {
            ushort addr = m_CPUBus.ReadByte(PC++);
            return (ushort)(addr & 0x00FF);
        }

        /// <summary>
        /// zero page,x 寻址，返回地址
        /// </summary>
        /// <returns></returns>
        private ushort ZeroPageX()
        {
            ushort addr = m_CPUBus.ReadByte(PC++);
            return (ushort)((addr + X) & 0x00FF);
        }

        /// <summary>
        /// zero page,y寻址，返回地址
        /// </summary>
        /// <returns></returns>
        private ushort ZeroPageY()
        {
            ushort addr = m_CPUBus.ReadByte(PC++);
            return (ushort)((addr + Y) & 0x00FF);
        }

        /// <summary>
        /// absolute寻址，返回地址
        /// </summary>
        /// <returns></returns>
        private ushort Absolute()
        {
            ushort addr = ReadWord(PC);
            PC += 2;
            return addr;
        }

        /// <summary>
        /// absolute,x 寻址，返回地址
        /// </summary>
        /// <param name="crossPage">是否跨页了</param>
        /// <returns></returns>
        private ushort AbsoluteX(out bool crossPage)
        {
            ushort addr = ReadWord(PC);
            PC += 2;
            ushort newAddr = (ushort)(addr + X);
            crossPage = IsCrossPage(addr, newAddr);
            return newAddr;
        }

        /// <summary>
        /// absolute,y 寻址，返回地址
        /// </summary>
        /// <param name="crossPage">是否跨页了</param>
        /// <returns></returns>
        private ushort AbsoluteY(out bool crossPage)
        {
            ushort addr = ReadWord(PC);
            PC += 2;
            ushort newAddr = (ushort)(addr + Y);
            crossPage = IsCrossPage(addr, newAddr);
            return newAddr;
        }

        /// <summary>
        /// relative 相对寻址，返回地址
        /// </summary>
        /// <returns></returns>
        private ushort Relative()
        {
            ushort addr = m_CPUBus.ReadByte(PC++);
            return addr;
        }

        /// <summary>
        /// indirect寻址，返回地址，一般就jmp用
        /// </summary>
        /// <returns></returns>
        private ushort Indirect()
        {
            ushort addr = ReadWord(PC);

            //间接寻址有个bug，低位为FF的时候，即addr如果为 0x10FF的时候
            //应该读取的2个地址为0x10FF和0x1100，由于该指令无法跨页
            //所以读取的2个地址分别为0x10FF(低位)和0x1000(高位)

            if ((addr & 0x00FF) == 0xFF)
            {
                //Bug触发
                byte low = m_CPUBus.ReadByte(addr);
                byte high = m_CPUBus.ReadByte((ushort)(addr & 0xFF00));
                addr = (ushort)((high << 8) | low);
            }
            else
            {
                addr = ReadWord(addr);
            }

            PC += 2;
            return addr;
        }

        /// <summary>
        /// (indrect,x) Indexed Indirect 变址间接寻址，返回地址
        /// </summary>
        /// <returns></returns>
        private ushort IndexedIndirect()
        {
            byte offset = m_CPUBus.ReadByte(PC++);
            ushort addr = (ushort)(byte)(offset + X);
            //踩坑，本来以为只有indirect一个寻址指令有该bug，结果IndexedIndirect也有
            //间接寻址有个bug，低位为FF的时候，即addr如果为 0x10FF的时候
            //应该读取的2个地址为0x10FF和0x1100，由于该指令无法跨页
            //所以读取的2个地址分别为0x10FF(低位)和0x1000(高位)
            if ((addr & 0xFF) == 0xFF)
            {
                byte low = m_CPUBus.ReadByte(addr);
                byte high = m_CPUBus.ReadByte((ushort)(addr & 0xFF00));
                addr = (ushort)((high << 8) | low);
            }
            else
            {
                addr = ReadWord(addr);
            }
            return addr;
        }

        /// <summary>
        /// (indrect),y Indirect Indexed 间接变址寻址，返回地址
        /// </summary>
        /// <param name="crossPage">是否跨页了</param>
        /// <returns></returns>
        private ushort IndirectIndexed(out bool crossPage)
        {
            ushort oldAddr = PC;
            ushort addr = m_CPUBus.ReadByte(PC++);

            //踩坑，本来以为只有indirect一个寻址指令有该bug，结果IndexedIndirect也有
            //间接寻址有个bug，低位为FF的时候，即addr如果为 0x10FF的时候
            //应该读取的2个地址为0x10FF和0x1100，由于该指令无法跨页
            //所以读取的2个地址分别为0x10FF(低位)和0x1000(高位)

            if ((addr & 0xFF) == 0xFF)
            {
                byte low = m_CPUBus.ReadByte(addr);
                byte high = m_CPUBus.ReadByte((ushort)(addr & 0xFF00));
                addr = (ushort)((high << 8) | low);
            }
            else
            {
                addr = ReadWord(addr);
            }

            addr += Y;
            crossPage = IsCrossPage(oldAddr, addr);
            return addr;
        }

        private void Push(byte data)
        {
            ushort addr = (ushort)(0x100 + SP);
            m_CPUBus.WriteByte(addr, data);
            SP--;
        }

        private byte Pop()
        {
            SP++;
            ushort addr = (ushort)(0x100 + SP);
            byte data = m_CPUBus.ReadByte(addr);
            return data;
        }

        private ushort ReadWord(ushort addr)
        {
            byte low = m_CPUBus.ReadByte(addr);
            byte high = m_CPUBus.ReadByte((ushort)(addr + 1));
            return (ushort)((high << 8) | low);
        }

        /// <summary>
        /// 是否跨页了
        /// </summary>
        /// <param name="oldAddr"></param>
        /// <param name="newAddr"></param>
        /// <returns>true表示跨页了,false表示没跨</returns>
        private static bool IsCrossPage(ushort oldAddr, ushort newAddr)
        {
            return (oldAddr & 0xFF00) != (newAddr & 0xFF00);
        }

        private static byte BoolToBit(bool b)
        {
            return b ? (byte)1 : (byte)0;
        }

        private static bool CheckAddOverflowPresent(params byte[] dataArray)
        {
            if (dataArray.Length == 0)
            {
                return false;
            }

            sbyte[] dataList = dataArray.Select(x => (sbyte)x).ToArray();
            sbyte sum = 0;

            for (int i = 0; i < dataList.Length; ++i)
            {
                int t = sum + dataList[i];
                if (t > 127 || t < -128)
                {
                    return true;
                }

                sum += dataList[i];
            }

            return false;
        }

        private static bool CheckSubOverflowPresent(params byte[] dataArray)
        {
            if (dataArray.Length == 0)
            {
                return false;
            }

            sbyte[] dataList = dataArray.Select(x => (sbyte)x).ToArray();
            sbyte sum = dataList[0];

            for (int i = 1; i < dataList.Length; ++i)
            {
                int t = sum - dataList[i];
                if (t > 127 || t < -128)
                {
                    return true;
                }

                sum -= dataList[i];
            }

            return false;
        }
    }
}
