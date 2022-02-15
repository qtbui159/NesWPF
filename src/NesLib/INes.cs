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

        void Down(bool pressDown);

        void Start(bool pressDown);

        void Select(bool pressDown);

        /// <summary>
        /// 获取背景色,rgba
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        int GetBackgroundColor(int x, int y);

        /// <summary>
        /// 获取背景tile，每个tile 8*8像素,共32*30
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="ty"></param>
        /// <returns></returns>
        int[][] GetBackgroundTileColor(int tx, int ty);

        /// <summary>
        /// 获取20个调色板数据，rgba
        /// </summary>
        /// <returns></returns>
        int[] GetPalette();

        /// <summary>
        /// 获取精灵tile
        /// </summary>
        /// <returns></returns>
        int[][] GetSpriteTileColor(int count, out int x, out int y);
    }
}
