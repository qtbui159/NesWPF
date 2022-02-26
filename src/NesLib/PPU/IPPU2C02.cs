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
        /// Temporary VRAM address (15 bits); can also be thought of as the address of the top left onscreen tile.
        /// </summary>
        VRAMAddrRegister T { get; set; }

        /// <summary>
        /// Fine X scroll (3 bits)
        /// </summary>
        byte FineXScroll { get; set; }

        /// <summary>
        /// 显存地址
        /// V寄存器
        /// </summary>
        VRAMAddrRegister Addr { get; set; }

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
        /// First or second write toggle (1 bit)
        /// </summary>
        bool WriteX2Flag { get; set; }

        /// <summary>
        /// 切换当前PPU的命名表映射规则
        /// </summary>
        /// <param name="mirroringMode"></param>
        void SwitchNameTableMirroring(MirroringMode mirroringMode);
        void Ticktock();
    }
}