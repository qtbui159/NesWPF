using NesLib.Bus;
using NesLib.Cartridge;
using NesLib.CPU;
using NesLib.IO;
using NesLib.Memory;
using NesLib.NesFile;
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
        private readonly IIORegister m_IORegister;
        private readonly IPPUBus m_PPUBus;
        private readonly IRAM m_VRAM;
        private ICartridge m_Cartridge;

        public Nes()
        {
            m_CPUBus = new CPUBus();
            m_CPU6502 = new CPU6502(m_CPUBus);
            m_RAM = new RAM();
            m_IORegister = new IORegister();
            m_PPUBus = new PPUBus();
            m_VRAM = new VRAM();
        }

        public async Task InsertCartidgeAsync(string nesFile)
        {
            IFileLoader fileLoader = new Nes10FileLoader();
            ICartridge cartridge = await fileLoader.LoadAsync(@"C:\Users\Spike\Desktop\nestest.nes");
            m_Cartridge = cartridge;
            m_CPUBus.ConnectCartridge(m_Cartridge);
            m_PPUBus.ConnectCartridge(m_Cartridge);
        }

        public void PowerUp()
        {
            m_CPUBus.ConnectRAM(m_RAM);
            m_CPUBus.ConnectIORegister(m_IORegister);
            m_PPUBus.ConnectVRAM(m_VRAM);

            m_CPU6502.RESET();
            while (true)
            {
                //Thread.Sleep(100);
                m_CPU6502.TickTock();
            }
        }
    }
}
