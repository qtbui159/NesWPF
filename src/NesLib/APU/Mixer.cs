using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//参考资料：
//1*)https://www.nesdev.org/wiki/APU_Pulse
namespace NesLib.APU
{
    /// <summary>
    /// 混音器
    /// </summary>
    class Mixer
    {
        public decimal Mix(Pulse1 pulse1, Pulse2 pulse2, Triangle triangle, Noise noise, DMC dmc)
        {
            //根据参考资料1*)末尾内容[Pulse channel output to mixer]，某些情况下输入值是静音

            return decimal.Zero;
        }
    }
}
