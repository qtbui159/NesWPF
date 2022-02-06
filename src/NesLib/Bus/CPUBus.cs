using NesLib.Cartridge;
using NesLib.IO;
using NesLib.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.Bus
{
    class CPUBus : ICPUBus
    {
        private IRAM m_RAM;
        private ICartridge m_Cartridge;
        private IIORegister m_IORegister;

        public void ConnectRAM(IRAM ram)
        {
            m_RAM = ram;
        }

        public void ConnectCartridge(ICartridge cartridge)
        {
            m_Cartridge = cartridge;
        }

        public void ConnectIORegister(IIORegister io)
        {
            m_IORegister = io;
        }

        public byte ReadByte(ushort addr)
        {
            if (addr < 0x2000)
            {
                return m_RAM.ReadByte(addr);
            }
            else if (addr >= 0x2000 && addr < 0x4020)
            {
                return m_IORegister.ReadByte(addr);
            }
            else if (addr >= 0x6000)
            {
                return m_Cartridge.Mapper.ReadByte(addr);
            }
            else
            {
                throw new Exception("不支持的地址");
            }
        }

        public ushort ReadWord(ushort addr)
        {
            if (addr < 0x2000)
            {
                return m_RAM.ReadWord(addr);
            }
            else if (addr >= 0x6000)
            {
                return m_Cartridge.Mapper.ReadWord(addr);
            }
            else
            { 
                throw new Exception("不支持的地址");
            }
        }

        public void WriteByte(ushort addr, byte data)
        {
            if (addr < 0x2000)
            {
                m_RAM.WriteByte(addr, data);
            }
            else if (addr >= 0x2000 && addr < 0x4020)
            {
                m_IORegister.WriteByte(addr, data);
            }
            else if (addr >= 0x6000)
            {
                m_Cartridge.Mapper.WriteByte(addr, data);
            }
            else
            { 
                throw new Exception("不支持的地址");
            }
        }

        public void WriteWord(ushort addr, ushort data)
        {
            if (addr < 0x2000)
            {
                m_RAM.WriteWord(addr, data);
            }
            else if (addr >= 0x6000)
            {
                m_Cartridge.Mapper.WriteWord(addr, data);
            }
            else
            { 
                throw new Exception("不支持的地址");
            }
        }
    }
}
