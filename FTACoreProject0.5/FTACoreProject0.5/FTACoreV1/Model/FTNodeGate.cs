using System.Linq;
using System.Collections.Generic;
using FTACoreSL.Constant;

namespace FTACoreSL.Model
{
    /// <summary>
    /// 新算法需要的数据结构 刘林林 20130718
    /// </summary>
    public class FTNodeGate : FTNodeBase
    {
        /// <summary>
        ///获取或设定门节点是否为对故障树进行二次处理生成的虚节点
        /// </summary>
        public bool IsVirtual { get; set; }

        int _kofN;
        /// <summary>
        /// 获取或设置表决门（K/N）节点的K。
        /// </summary>
        public int KofN
        {
            get { return NodeType == FTConstants.Gate_VOTE ? _kofN : 0; }
            set { if (NodeType == FTConstants.Gate_VOTE) _kofN = value; }
        }
    }
}