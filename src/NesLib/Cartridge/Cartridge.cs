using NesLib.Cartridge.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.Cartridge
{
    class Cartridge : ICartridge
    {
        public IMapper Mapper { get; internal set; }
        public byte[] PRGRam { get; internal set; }
        public byte[] PRGRom { get; internal set; }
        public byte[] CHRRom { get; internal set; }

        internal static ICartridge New(byte[] prgRom, byte[] chrRom, int mapperVersion)
        {
            byte[] prgRam = new byte[0x2000];
            IMapper mapper;
            if (mapperVersion == 0)
            {
                mapper = new Mapper000(prgRam, prgRom, chrRom);
            }
            else 
            {
                throw new Exception($"不支持Mapper{mapperVersion}");
            }

            return new Cartridge()
            {
                PRGRam = prgRam,
                CHRRom = chrRom,
                PRGRom = prgRom,
                Mapper = mapper,
            };
        }
    }
}
