using NesLib.Bus;
using NesLib.Cartridge;
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

        public Nes()
        {
            m_CPUBus = new CPUBus();
            m_CPU6502 = new CPU6502(m_CPUBus);
            m_RAM = new RAM();
            m_PPUBus = new PPUBus();
            m_PPU2C02 = new PPU2C02(m_PPUBus);
            m_VRAM = new VRAM();
            m_Palette = new Palette();
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

            while (true)
            {
                //时序参考资料2*)

                //1.0-239行，共240行visible 扫描线
                int[][] frame = new int[240][];
                bool hit = false;
                
                for (int i = 0; i < 240; ++i)
                {
                    int[] scanline = m_PPU2C02.PaintScanLine(i, ref hit);
                    m_CPU6502.TickTock(1);
                    frame[i] = scanline;
                }

                m_PPU2C02.PaintSprite(frame);
                paintCallback?.Invoke(frame);

                //2.240-260行，空行,241行,第1个点（0开始算) set vblank flag
                //这里不需要太精确，直接先vblank然后跑21行空行

                //VBLANK，参考资料1*)
                for (int i = 0; i < 21; ++i)
                {
                    m_CPU6502.TickTock(1);
                    if (i == 1)
                    {
                        m_PPU2C02.STATUS.V = 1;
                        if (m_PPU2C02.CTRL.V == 1)
                        {
                            m_CPU6502.NMI();
                        }
                        m_PPU2C02.Addr.SetValue(m_PPU2C02.T.Value);
                    }
                }

                //3.261行，预扫描行，第1个点（0开始算）clear vblank flag; sprite 0 hits;sprite overflow;
                m_PPU2C02.PreRenderLine();
                m_PPU2C02.STATUS.SetValue(0);
                m_CPU6502.TickTock(1);

                m_CPU6502.ResetCycles();
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
    }
}
