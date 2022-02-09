using NesLib.Cartridge;
using NesLib.Memory;
using NesLib.PPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * 参考资料
 * 1*)https://wiki.nesdev.org/w/index.php?title=PPU_registers#PPUSCROLL
 * 2*)https://wiki.nesdev.org/w/index.php?title=PPU_registers#Address_.28.242006.29_.3E.3E_write_x2
 */
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
                    byte data = m_PPU.ReadByte(m_PPU.Addr);

                    if (m_PPU.CTRL.I == 1)
                    {
                        m_PPU.Addr += 32;
                    }
                    else
                    {
                        m_PPU.Addr += 1;
                    }
                    return data;
                }
                else if (addr == 0x4014)
                {
                    throw new Exception("该地址不支持读取");
                }
                else
                {
                    throw new Exception("该地址不支持读取");
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
                    m_PPU.OAM[m_PPU.OAMAddr++] = data;
                }
                else if (addr == 0x2005)
                {
                    //双写操作，参考资料1*)
                    if (m_PPU.WriteX2Flag)
                    {
                        //二写
                    }
                    else
                    {
                        //一写
                    }

                    m_PPU.WriteX2Flag = !m_PPU.WriteX2Flag;
                }
                else if (addr == 0x2006)
                {
                    //双写操作，参考资料2*)
                    if (m_PPU.WriteX2Flag)
                    {
                        //二写，再写低位
                        ushort tmpAddr = m_PPU.Addr;
                        tmpAddr = (ushort)(tmpAddr & 0xFF00);
                        tmpAddr = (ushort)(tmpAddr | data);
                        m_PPU.Addr = tmpAddr;
                    }
                    else
                    {
                        //一写，先写高位
                        ushort tmpAddr = m_PPU.Addr;
                        tmpAddr = (ushort)(tmpAddr & 0x00FF);
                        tmpAddr = (ushort)((data << 8) | tmpAddr);
                        m_PPU.Addr = tmpAddr;
                    }

                    m_PPU.WriteX2Flag = !m_PPU.WriteX2Flag;
                }
                else if (addr == 0x2007)
                {
                    m_PPU.WriteByte(addr, data);

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
                    ushort startAddr = (ushort)(data << 8);
                    ushort endAddr = (ushort)(startAddr | 0xFF);
                    List<byte> dataList = new List<byte>();
                    for (ushort i = startAddr; i < endAddr; ++i)
                    {
                        byte b = ReadByte(i);
                        dataList.Add(b);
                    }
                    byte[] tmpData = dataList.ToArray();

                    Array.Copy(tmpData, m_PPU.OAM, tmpData.Length);
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
