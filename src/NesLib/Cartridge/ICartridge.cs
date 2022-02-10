using NesLib.Cartridge.Mapper;

namespace NesLib.Cartridge
{
    /// <summary>
    /// 卡带，包含Mapper，PRG，CHR信息
    /// </summary>
    public interface ICartridge
    {
        byte[] CHRRom { get; }
        IMapper Mapper { get; }
        byte[] PRGRom { get; }
    }
}