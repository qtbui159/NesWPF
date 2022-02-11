using NesLib.Cartridge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.NesFile
{
    interface IFileLoader
    {
        /// <summary>
        /// 载入nes文件，返回解析后的卡带信息
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<ICartridge> LoadAsync(string path);
    }
}
