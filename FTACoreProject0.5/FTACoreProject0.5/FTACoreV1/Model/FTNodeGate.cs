using System.Linq;
using System.Collections.Generic;
using FTACoreSL.Constant;

namespace FTACoreSL.Model
{
    /// <summary>
    /// ���㷨��Ҫ�����ݽṹ ������ 20130718
    /// </summary>
    public class FTNodeGate : FTNodeBase
    {
        /// <summary>
        ///��ȡ���趨�Žڵ��Ƿ�Ϊ�Թ��������ж��δ������ɵ���ڵ�
        /// </summary>
        public bool IsVirtual { get; set; }

        int _kofN;
        /// <summary>
        /// ��ȡ�����ñ���ţ�K/N���ڵ��K��
        /// </summary>
        public int KofN
        {
            get { return NodeType == FTConstants.Gate_VOTE ? _kofN : 0; }
            set { if (NodeType == FTConstants.Gate_VOTE) _kofN = value; }
        }
    }
}