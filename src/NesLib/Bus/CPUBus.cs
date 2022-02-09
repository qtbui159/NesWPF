using NesLib.Cartridge;
using NesLib.Memory;
using NesLib.PPU;
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
        private IPPU2C02 m_PPU;

        public void ConnectRAM(IRAM ram)
        {
            m_RAM = ram;
        }

        public void ConnectCartridge(ICartridge cartridge)
        {
            m_Cartridge = cartridge;
        }

        public void ConnectPPU(IPPU2C02 ppu)
        {
            m_PPU = ppu;
        }

        public byte ReadByte(ushort addr)
        {
            if (addr < 0x2000)
            {
                return m_RAM.ReadByte(addr);
            }
            else if (addr >= 0x2000 && addr < 0x4020)
            {
                addr = GetIORegisterRealAddr(addr);
                if (addr == 0x2000)
                {
                    throw new Exception("该地址不支持读取");
                }
                else if (addr == 0x2001)
                {
                    throw new Exception("该地址不支持读取");
                }
                else if (addr == 0x2002)
                {
                    // 读取后会清除VBlank状态
                    byte data= m_PPU.STATUS.Value;
                    m_PPU.STATUS.V = 0;
                    return data;
                }
                else if (addr == 0x2003)
                {
                    throw new Exception("该地址不支持读取");
                }
                else if (addr == 0x2004)
                {
                    return m_PPU.OAM[m_PPU.OAMAddr++];
                }
                else if (addr == 0x2005)
                {
                    throw new Exception("该地址不支持读取");
                }
                else if (addr == 0x2006)
                {
                    throw new Exception("该地址不支持读取");
                }
                else if (addr == 0x2007)
                {
                    m_PPU.ReadByte(m_PPU.Addr);

                    if (m_PPU.CTRL.I == 1)
                    {
                        m_PPU.Addr += 32;
                    }
                    else
                    {
                        m_PPU.Addr += 1;
                    }
                }
                else if (addr == 0x4014)
                {
                    throw new Exception("该地址不支持读取");
                }
                else
                {

                }
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
            else if (addr >= 0x2000 && addr < 0x4020)
            {
                throw new Exception("不支持的地址");
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
                addr = GetIORegisterRealAddr(addr);
                if (addr == 0x2000)
                {
                    m_PPU.CTRL.SetValue(data);
                }
                else if (addr == 0x2001)
                {
                    m_PPU.MASK.SetValue(data);
                }
                else if (addr == 0x2002)
                {
                    throw new Exception("该地址不支持写入");
                }
                else if (addr == 0x2003)
                {
                    m_PPU.OAMAddr = data;
                }
                else if (addr == 0x2004)
                {
                }
                else if (addr == 0x2005)
                {
                    throw new Exception("该地址不支持读取");
                }
                else if (addr == 0x2006)
                {
                    throw new Exception("该地址不支持读取");
                }
                else if (addr == 0x2007)
                {
                    m_PPU.ReadByte(m_PPU.Addr);

                    if (m_PPU.CTRL.I == 1)
                    {
                        m_PPU.Addr += 32;
                    }
                    else
                    {
                        m_PPU.Addr += 1;
                    }
                }
                else if (addr == 0x4014)
                {
                    throw new Exception("该地址不支持读取");
                }
                else
                {

                }
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
            else if (addr >= 0x2000 && addr < 0x4020)
            {
                throw new Exception("不支持的地址");
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

        private ushort GetIORegisterRealAddr(ushort addr)
        {
            return (ushort)(addr & 0x2007);
        }
    }
}
