using NesLib.Cartridge;
using NesLib.JoyStick;
using NesLib.Memory;
using NesLib.PPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

/**
 * 参考资料
 * 1*)https://wiki.nesdev.org/w/index.php?title=PPU_registers#PPUSCROLL
 * 2*)https://wiki.nesdev.org/w/index.php?title=PPU_registers#Address_.28.242006.29_.3E.3E_write_x2
 * 3*)https://wiki.nesdev.org/w/index.php/PPU_registers#The_PPUDATA_read_buffer_.28post-fetch.29
 * 4*)https://wiki.nesdev.org/w/index.php?title=PPU_scrolling
 */
namespace NesLib.Bus
{
    class CPUBus : ICPUBus
    {
        private IRAM m_RAM;
        private ICartridge m_Cartridge;
        private IPPU2C02 m_PPU;
        private IJoyStick m_P1JoyStick;
        private IJoyStick m_P2JoyStick;
        private Action m_DMACycle;

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

        public void ConnectJoyStock(IJoyStick p1Joystick, IJoyStick p2Joystick)
        {
            m_P1JoyStick = p1Joystick;
            m_P2JoyStick = p2Joystick;
        }

        public byte ReadByte(ushort addr)
        {
            if (addr < 0x2000)
            {
                return m_RAM.ReadByte(addr);
            }
            else if (addr >= 0x2000 && addr < 0x4020)
            {
                if (addr < 0x4000)
                {
                    ushort ioRealAddr = GetIORegisterRealAddr(addr);
                    if (ioRealAddr == 0x2000)
                    {
                        throw new Exception("该地址不支持读取");
                    }
                    else if (ioRealAddr == 0x2001)
                    {
                        throw new Exception("该地址不支持读取");
                    }
                    else if (ioRealAddr == 0x2002)
                    {
                        // 读取后会清除VBlank状态
                        byte data = m_PPU.STATUS.Value;
                        m_PPU.STATUS.V = 0;

                        //参考资料4*)
                        m_PPU.WriteX2Flag = false;
                        return data;
                    }
                    else if (ioRealAddr == 0x2003)
                    {
                        throw new Exception("该地址不支持读取");
                    }
                    else if (ioRealAddr == 0x2004)
                    {
                        return m_PPU.OAM[m_PPU.OAMAddr++];
                    }
                    else if (ioRealAddr == 0x2005)
                    {
                        throw new Exception("该地址不支持读取");
                    }
                    else if (ioRealAddr == 0x2006)
                    {
                        throw new Exception("该地址不支持读取");
                    }
                    else if (ioRealAddr == 0x2007)
                    {
                        //参考资料4*),这里读取的永远都是缓存值
                        byte oldData = m_PPU.ReadBuffer;
                        ushort vramAddr = m_PPU.Addr.Value;
                        byte data = m_PPU.ReadByte(vramAddr);
                        m_PPU.ReadBuffer = data;
                        if (m_PPU.CTRL.I == 1)
                        {
                            vramAddr += 32;
                        }
                        else
                        {
                            vramAddr += 1;
                        }
                        m_PPU.Addr.SetValue(vramAddr);
                        return oldData;
                    }
                    else
                    {
                        throw new Exception("该地址不支持读取");
                    }
                }
                else if (addr == 0x4014)
                {
                    throw new Exception("该地址不支持读取");
                }
                else if (addr == 0x4016)
                {
                    //P1手柄
                    return m_P1JoyStick.GetValue();
                }
                else if (addr == 0x4017)
                {
                    //P2手柄
                    return m_P2JoyStick.GetValue();
                }
                else
                {
                    return 0;
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

        public void WriteByte(ushort addr, byte data)
        {
            if (addr < 0x2000)
            {
                m_RAM.WriteByte(addr, data);
            }
            else if (addr >= 0x2000 && addr < 0x4020)
            {
                if (addr < 0x4000)
                {
                    ushort ioRealAddr = GetIORegisterRealAddr(addr);

                    if (ioRealAddr == 0x2000)
                    {
                        m_PPU.CTRL.SetValue(data);

                        m_PPU.T.UpdateBit(BitService.GetBit(data, 0), 10);
                        m_PPU.T.UpdateBit(BitService.GetBit(data, 1), 11);
                    }
                    else if (ioRealAddr == 0x2001)
                    {
                        m_PPU.MASK.SetValue(data);
                    }
                    else if (ioRealAddr == 0x2002)
                    {
                        throw new Exception("该地址不支持写入");
                    }
                    else if (ioRealAddr == 0x2003)
                    {
                        m_PPU.OAMAddr = data;
                    }
                    else if (ioRealAddr == 0x2004)
                    {
                        m_PPU.OAM[m_PPU.OAMAddr++] = data;
                    }
                    else if (ioRealAddr == 0x2005)
                    {
                        //双写操作，参考资料1*)
                        if (m_PPU.WriteX2Flag)
                        {
                            //二写
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 0), 12);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 1), 13);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 2), 14);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 3), 5);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 4), 6);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 5), 7);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 6), 8);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 7), 9);
                        }
                        else
                        {
                            //一写
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 3), 0);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 4), 1);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 5), 2);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 6), 3);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 7), 4);

                            m_PPU.X = (byte)(data & 0x7);
                        }

                        m_PPU.WriteX2Flag = !m_PPU.WriteX2Flag;
                    }
                    else if (ioRealAddr == 0x2006)
                    {
                        //双写操作，参考资料2*)
                        if (m_PPU.WriteX2Flag)
                        {
                            //二写，再写低位
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 0), 0);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 1), 1);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 2), 2);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 3), 3);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 4), 4);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 5), 5);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 6), 6);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 7), 7);

                            m_PPU.Addr.SetValue(m_PPU.T.Value);
                        }
                        else
                        {
                            //一写，先写高位
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 0), 8);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 1), 9);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 2), 10);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 3), 11);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 4), 12);
                            m_PPU.T.UpdateBit(BitService.GetBit(data, 5), 13);
                            m_PPU.T.UpdateBit(0, 14);
                        }

                        m_PPU.WriteX2Flag = !m_PPU.WriteX2Flag;
                    }
                    else if (ioRealAddr == 0x2007)
                    {
                        ushort vramAddr = m_PPU.Addr.Value;
                        m_PPU.WriteByte(vramAddr, data);

                        if (m_PPU.CTRL.I == 1)
                        {
                            vramAddr += 32;
                        }
                        else
                        {
                            vramAddr += 1;
                        }
                        m_PPU.Addr.SetValue(vramAddr);
                    }
                }
                else if(addr == 0x4014)
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

                    m_DMACycle?.Invoke();
                }
                else if (addr == 0x4016)
                {
                    byte strobe = BitService.GetBit(data, 0);
                    if (strobe == 0)
                    {
                        m_P1JoyStick.ClearOffset();
                        m_P2JoyStick.ClearOffset();
                    }
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


        private ushort GetIORegisterRealAddr(ushort addr)
        {
            return (ushort)(addr & 0x2007);
        }

        public void SetDMACycles(Action dmaCycle)
        {
            m_DMACycle = dmaCycle;
        }
    }
}
