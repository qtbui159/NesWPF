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
        void PowerUp();

        /// <summary>
        /// 插卡
        /// </summary>
        /// <param name="nesFile">卡文件</param>
        Task InsertCartidgeAsync(string nesFile);

        /// <summary>
        /// 获取背景色,rgba
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        int GetBackgroundColor(int x, int y);
    }
}
