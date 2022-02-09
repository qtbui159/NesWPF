using NesLib.Bus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//参考资料：
//1*)https://wiki.nesdev.org/w/index.php?title=PPU_power_up_state

namespace NesLib.PPU
{
    class PPU2C02 : IPPU2C02
    {
        private readonly IPPUBus m_PPUBus;

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
    }
}
