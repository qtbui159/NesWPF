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
        void TickTockByCount(ref long count);

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
