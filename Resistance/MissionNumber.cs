using System;
using System.Collections.Generic;
using System.Text;

namespace Resistance {
    /// <summary>
    /// 各ミッションの人数を表します。
    /// </summary>
    struct MissionNumber {
        static int[,] number = {
            {2,2,2,3,3,3,}, //第一ラウンド
            {3,3,3,4,4,4,}, //第二ラウンド
            {2,4,3,4,4,4,}, //第三ラウンド
            {3,3,4,5,5,5,}, //第四ラウンド、ただし7人以上なら2の失敗が必要
            {3,4,4,5,5,5,}, //第五ラウンド
        };

        /// <summary>
        /// ミッションに参加するべき人数を表します。
        /// </summary>
        public int NumberOfMembers { get; private set; }

        /// <summary>
        /// ミッションが失敗するために必要な失敗カードの数を表します。
        /// </summary>
        public int NumberRequiredForFailer { get; private set; }

        /// <summary>
        /// MissionNumberを生成します。
        /// </summary>
        /// <param name="playersCount">プレイヤー人数</param>
        /// <param name="round">現在のラウンド数</param>
        public MissionNumber(int playersCount, int round) {
#if DEBUG
            this.NumberOfMembers = 1;
            this.NumberRequiredForFailer = 1;
#else
            this.NumberOfMembers = number[round, playersCount - 5];
            this.NumberRequiredForFailer = (round == 4 && playersCount >= 7) ? 2 : 1;
#endif
        }
    }
}
