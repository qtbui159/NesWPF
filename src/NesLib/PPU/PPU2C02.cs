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
        public CTRLRegister CTRL { get; private set; }
        public MASKRegister MASK { get; private set; }
        public STATUSRegister STATUS { get; private set; }

        public ushort Addr { get; set; } = 0;

        public byte[] OAM => m_OAM;
        private byte[] m_OAM = new byte[256];

        public byte OAMAddr { get; set; }

        public PPU2C02()
        {
            CTRL = new CTRLRegister();
            MASK = new MASKRegister();
            STATUS = new STATUSRegister();
        }

        public void WriteByte(ushort addr, byte data)
        {
            
        }

        public byte ReadByte(ushort addr)
        {
            return 0;
        }
    }
}
