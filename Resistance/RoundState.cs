using System;
using System.Collections.Generic;
using System.Text;

namespace Resistance {
    /// <summary>
    /// 各ラウンドの状態
    /// </summary>
    enum RoundState {
        /// <summary>
        /// レジスタンス（青陣営）の勝利
        /// </summary>
        ResistanceWin,
        
        /// <summary>
        /// スパイ（赤陣営）の勝利
        /// </summary>
        SpyWin,

        /// <summary>
        /// まだ
        /// </summary>
        NotYet,
    }
}
