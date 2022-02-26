using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.CPU
{
    interface ICPU6502
    {
        long Cycles { get; }

        /// <summary>
        /// 执行一条指令
        /// </summary>
        void TickTock();
        /// <summary>
        /// 平滑执行一条指令，
        /// 本意是一条指令需要2个周期,那么调用2次TickTockFlatCycle才能完成
        /// 不过实际不能执行半条指令，此操作用于和ppu同步
        /// 2个指令周期调用2次的处理细节
        /// 第1次调用完成指令全部功能
        /// 第2个调用为等待，所以一共使用2个指令周期
        /// </summary>
        void TickTockFlatCycle();

        /// <summary>
        /// 执行n条扫描线的指令周期，1条扫描线为113.6667指令周期
        /// </summary>
        /// <param name="scanlineCount"></param>
        void TickTock(int scanlineCount);

        /// <summary>
        /// 重置cpu指令周期
        /// </summary>
        void ResetCycles();

        /// <summary>
        /// Reset中断
        /// </summary>
        void RESET();

        /// <summary>
        /// Nmi中断
        /// </summary>
        void NMI();

        /// <summary>
        /// Irq中断
        /// </summary>
        void IRQ();
    }
}
