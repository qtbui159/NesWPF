using NesLib.Bus;
using NesLib.Cartridge;
using NesLib.CPU;
using NesLib.IO;
using NesLib.Memory;
using NesLib.NesFile;
using NesLib.PPU;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        }

        public async Task InsertCartidgeAsync(string nesFile)
        {
            IFileLoader fileLoader = new Nes10FileLoader();
            ICartridge cartridge = await fileLoader.LoadAsync(@"C:\Users\Spike\Desktop\nestest.nes");
            m_Cartridge = cartridge;
            m_CPUBus.ConnectCartridge(m_Cartridge);
        }

        public void PowerUp()
        {
            m_CPUBus.ConnectRAM(m_RAM);
            m_CPUBus.ConnectPPU(m_PPU2C02);
            m_PPUBus.ConnectVRAM(m_VRAM);
            m_PPUBus.ConnectPalette(m_Palette);

            m_CPU6502.RESET();
            while (true)
            {
                //Thread.Sleep(100);
                m_CPU6502.TickTock();
            }
        }
    }
}
