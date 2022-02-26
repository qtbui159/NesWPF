using NesLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NesLib
{
    public interface INes
    {
        /// <summary>
        /// 开机
        /// </summary>
        void PowerUp(Action<int[][]> paintCallback);

        /// <summary>
        /// 插卡
        /// </summary>
        /// <param name="nesFile">卡文件</param>
        Task InsertCartidgeAsync(string nesFile);

        /// <summary>
        /// 获取20个调色板数据，rgba
        /// </summary>
        /// <returns></returns>
        int[] GetPalette();

        /// <summary>
        /// 按键控制
        /// </summary>
        /// <param name="jb"></param>
        /// <param name="pressDown"></param>
        void P1JoystickKey(JoystickButton jb, bool pressDown);
        void P2JoystickKey(JoystickButton jb, bool pressDown);
    }
}
