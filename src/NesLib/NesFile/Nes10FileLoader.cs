using NesLib.Cartridge;
using NesLib.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace NesLib.NesFile
{
    /// <summary>
    /// NES1.0版本的文件
    /// </summary>
    class Nes10FileLoader : IFileLoader
    {
        public Task<ICartridge> LoadAsync(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            return ParseAsync(data);
        }

        private async Task<ICartridge> ParseAsync(byte[] data)
        {
            //参考：https://wiki.nesdev.org/w/index.php/INES
            //nes1.0文件组成
            //header+trainer+PRG rom+CHR rom
            //header为16字节
            //header为0，trainer为0字节
            //header为1，trainer为512字节
            //PRGRom，16kb*n，n在header中指定
            //CHRRom，8kb*n，n为header中指定
            //所以data至少的大小为16+0+16kb+8kb
            const int MIN_SIZE = 16 + 0 + 16 * 1024 + 8 * 1024;
            if (data is null || data.Length < MIN_SIZE)
            {
                throw new Exception("无法解析该NES数据");
            }

            byte[] headerFlag = new byte[] { 0x4E, 0x45, 0x53, 0x1A };
            byte[] currentHeader = data.Take(headerFlag.Length).ToArray();
            if (!Enumerable.SequenceEqual(headerFlag, currentHeader))
            {
                throw new Exception("NES头文件不合法");
            }

            int amountOfPRGBlock = data[4]; //指明PRG块的数量
            int amountOfCHRBlock = data[5]; //指明CHR块的数量
            byte flag1 = data[6]; //https://wiki.nesdev.org/w/index.php/INES#Flags_6
            byte flag2 = data[7]; //https://wiki.nesdev.org/w/index.php/INES#Flags_7
            MirroringMode mirroringMode = MirroringMode.Horizontal;
            byte bMirroringMode = BitService.GetBit(flag1, 0);
            if (bMirroringMode == 0)
            {
                mirroringMode = MirroringMode.Horizontal;
            }
            else if (bMirroringMode == 1)
            {
                mirroringMode = MirroringMode.Vertical;
            }

            bool trainerPresent = BitService.GetBit(flag1, 2) == 1;
            int trainerSize = trainerPresent ? 512 : 0;
            int mapperLow = flag1 & 0xF0;
            int mapperHigh = flag2 & 0xF0;
            int mapper = (mapperHigh << 4) | mapperLow;

            byte[] prgData = data.Skip(16 + trainerSize).Take(amountOfPRGBlock * 16 * 1024).ToArray();
            byte[] chrData = data.Skip(16 + trainerSize + amountOfPRGBlock * 16 * 1024).Take(amountOfCHRBlock * 8 * 1024).ToArray();

            await Task.CompletedTask.ConfigureAwait(false);
            return Cartridge.Cartridge.New(mirroringMode, prgData, chrData, mapper);
        }
    }
}
