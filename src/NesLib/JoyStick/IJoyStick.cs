using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib.JoyStick
{
    interface IJoyStick
    {
        /// <summary>
        /// 左按键
        /// </summary>
        /// <param name="pressDown">是否按下</param>
        void Left(bool pressDown);

        /// <summary>
        /// 右按键
        /// </summary>
        /// <param name="pressDown">是否按下</param>
        void Right(bool pressDown);

        /// <summary>
        /// 上按键
        /// </summary>
        /// <param name="pressDown">是否按下</param>
        void Up(bool pressDown);

        /// <summary>
        /// 下按键
        /// </summary>
        /// <param name="pressDown">是否按下</param>
        void Down(bool pressDown);

        /// <summary>
        /// 选择按键
        /// </summary>
        /// <param name="pressDown">是否按下</param>
        void Select(bool pressDown);

        /// <summary>
        /// 开始按键
        /// </summary>
        /// <param name="pressDown">是否按下</param>
        void Start(bool pressDown);

        /// <summary>
        /// A键
        /// </summary>
        /// <param name="pressDown">是否按下</param>
        void A(bool pressDown);

        /// <summary>
        /// B键
        /// </summary>
        /// <param name="pressDown">是否按下</param>
        void B(bool pressDown);

        /// <summary>
        /// 清除strobe选通
        /// </summary>
        void ClearOffset();

        /// <summary>
        /// 获取手柄值
        /// </summary>
        /// <returns></returns>
        byte GetValue();
    }
}
