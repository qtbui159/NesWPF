using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.CPU
{
    interface ICPU6502
    {
        /// <summary>
        /// 执行一条指令
        /// </summary>
        void TickTock();

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
