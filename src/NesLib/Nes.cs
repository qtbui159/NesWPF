using NesLib.Bus;
using NesLib.Cartridge;
using NesLib.Common;
using NesLib.CPU;
using NesLib.JoyStick;
using NesLib.Memory;
using NesLib.NesFile;
using NesLib.PPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
/**
 *参考资料
 *1*)https://wiki.nesdev.org/w/index.php?title=NMI
 *2*)https://wiki.nesdev.org/w/index.php?title=File:Ntsc_timing.png
 */

namespace NesLib
{
    class Nes : INes
    {
        private readonly ICPUBus m_CPUBus;
        private readonly ICPU6502 m_CPU6502;
        private readonly IRAM m_RAM;
        private readonly IPPUBus m_PPUBus;
        private readonly IPPU2C02 m_PPU2C02;
        private readonly IRAM m_VRAM;
        private readonly IPalette m_Palette;
        private readonly IJoyStick m_Joytick1;
        private readonly IJoyStick m_Joytick2;
        private ICartridge m_Cartridge;

        private Action<int[][]> fuck;

        public Nes()
        {
            m_CPUBus = new CPUBus();
            m_CPU6502 = new CPU6502(m_CPUBus);
            m_RAM = new RAM();
            m_PPUBus = new PPUBus();
            m_PPU2C02 = new PPU2C02(m_PPUBus, () => m_CPU6502.NMI(), x => fuck(x));
            m_VRAM = new VRAM();
            m_Palette = new Palette(m_PPU2C02);
            m_Joytick1 = new JoyStick.JoyStick();
            m_Joytick2 = new JoyStick.JoyStick();
        }

        public async Task InsertCartidgeAsync(string nesFile)
        {
            IFileLoader fileLoader = new Nes10FileLoader();
            ICartridge cartridge = await fileLoader.LoadAsync(nesFile);
            m_Cartridge = cartridge;
            m_CPUBus.ConnectCartridge(m_Cartridge);
            m_PPUBus.ConnectCartridge(m_Cartridge);
            m_PPU2C02.SwitchNameTableMirroring(m_Cartridge.MirroringMode);
        }

        public void PowerUp(Action<int[][]> paintCallback)
        {
            m_CPUBus.ConnectRAM(m_RAM);
            m_CPUBus.ConnectPPU(m_PPU2C02);
            m_CPUBus.ConnectJoyStock(m_Joytick1, m_Joytick2);
            m_PPUBus.ConnectVRAM(m_VRAM);
            m_PPUBus.ConnectPalette(m_Palette);

            m_CPU6502.RESET();

            fuck = paintCallback;
            while (true)
            {
                m_CPU6502.TickTockByCount();

                m_PPU2C02.Ticktock();
                m_PPU2C02.Ticktock();
                m_PPU2C02.Ticktock();
            }
        }

        public int GetBackgroundColor(int x, int y)
        {
            return m_PPU2C02.GetBackgroundPixel(x, y);
        }

        public int[] GetPalette()
        {
            List<int> r = new List<int>();
            for (int i = 0; i < 0x20; ++i)
            {
                byte offset = m_PPUBus.ReadByte((ushort)(0x3F00 + i));
                r.Add(Palette.GetRGBAColor(offset));
            }
            return r.ToArray();
        }

        public int[][] GetBackgroundTileColor(int tx, int ty)
        {
            return m_PPU2C02.GetBackgroundTileColor(tx, ty);
        }

        public int[][] GetSpriteTileColor(int count, out int x, out int y)
        {
            return m_PPU2C02.GetSpriteTileColor(count, out x, out y);
        }

        public int[][] PaintFrame()
        {
            return m_PPU2C02.PaintFrame();
        }

        public void Right(bool pressDown)
        {
            m_Joytick1.Right(pressDown);
        }

        public void Down(bool pressDown)
        {
            m_Joytick1.Down(pressDown);
        }

        public void Start(bool pressDown)
        {
            m_Joytick1.Start(pressDown);
        }

        public void Select(bool pressDown)
        {
            m_Joytick1.Select(pressDown);
        }

        public void P1JoystickKey(JoystickButton jb, bool pressDown)
        {
            if (jb is JoystickButton.A)
            {
                m_Joytick1.A(pressDown);
            }
            else if (jb is JoystickButton.B)
            {
                m_Joytick1.B(pressDown);
            }
            else if (jb is JoystickButton.Down)
            {
                m_Joytick1.Down(pressDown);
            }
            else if (jb is JoystickButton.Left)
            {
                m_Joytick1.Left(pressDown);
            }
            else if (jb is JoystickButton.Right)
            {
                m_Joytick1.Right(pressDown);
            }
            else if (jb is JoystickButton.Select)
            {
                m_Joytick1.Select(pressDown);
            }
            else if (jb is JoystickButton.Start)
            {
                m_Joytick1.Start(pressDown);
            }
            else if (jb is JoystickButton.Up)
            {
                m_Joytick1.Up(pressDown);
            }
        }

        public void P2JoystickKey(JoystickButton jb, bool pressDown)
        {
            if (jb is JoystickButton.A)
            {
                m_Joytick2.A(pressDown);
            }
            else if (jb is JoystickButton.B)
            {
                m_Joytick2.B(pressDown);
            }
            else if (jb is JoystickButton.Down)
            {
                m_Joytick2.Down(pressDown);
            }
            else if (jb is JoystickButton.Left)
            {
                m_Joytick2.Left(pressDown);
            }
            else if (jb is JoystickButton.Right)
            {
                m_Joytick2.Right(pressDown);
            }
            else if (jb is JoystickButton.Select)
            {
                m_Joytick2.Select(pressDown);
            }
            else if (jb is JoystickButton.Start)
            {
                m_Joytick2.Start(pressDown);
            }
            else if (jb is JoystickButton.Up)
            {
                m_Joytick2.Up(pressDown);
            }
        }
    }
}
