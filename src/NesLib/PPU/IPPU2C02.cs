using NesLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * 参考资料
 * https://wiki.nesdev.org/w/index.php?title=PPU_OAM
 */

namespace NesLib.PPU
{
    interface IPPU2C02 : IReadWrite8Bit
    {
        CTRLRegister CTRL { get; }
        MASKRegister MASK { get; }
        STATUSRegister STATUS { get; }

        /// <summary>
        /// 显存地址
        /// </summary>
        ushort Addr { get; set; }

        /// <summary>
        /// 读缓存
        /// </summary>
        byte ReadBuffer { get; set; }

        /// <summary>
        /// 精灵RAM内存，存在PPU内，一共256字节
        /// 不存在总线上，写还可以通过DMA(CPU总线$4014)一次性复制256字节，读写可以直接单字节操作
        /// </summary>
        byte[] OAM { get; }

        /// <summary>
        /// Object Attribute Memory地址
        /// </summary>
        byte OAMAddr { get; set; }

        /// <summary>
        /// 双写操作，初始化为false
        /// </summary>
        bool WriteX2Flag { get; set; }

        /// <summary>
        /// 切换当前PPU的命名表映射规则
        /// </summary>
        /// <param name="mirroringMode"></param>
        void SwitchNameTableMirroring(MirroringMode mirroringMode);

        /// <summary>
        /// 获取背景色
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        int GetBackgroundPixel(int x, int y);

        /// <summary>
        /// 获取背景tile，每个tile 8*8像素,共32*30
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        int[][] GetBackgroundTileColor(int tx, int ty);

        /// <summary>
        /// 获取精灵tile
        /// </summary>
        /// <returns></returns>
        int[][] GetSpriteTileColor(int count, out int x, out int y);
    }
}