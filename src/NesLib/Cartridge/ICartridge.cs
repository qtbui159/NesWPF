using NesLib.Cartridge.Mapper;
using NesLib.Common;

namespace NesLib.Cartridge
{
    /// <summary>
    /// 卡带，包含Mapper，PRG，CHR信息
    /// </summary>
    interface ICartridge
    {
        MirroringMode MirroringMode { get; }
        byte[] CHRRom { get; }
        IMapper Mapper { get; }
        byte[] PRGRom { get; }
    }
}