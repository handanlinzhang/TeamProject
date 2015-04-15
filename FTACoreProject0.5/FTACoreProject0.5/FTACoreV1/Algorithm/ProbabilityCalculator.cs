using System;
using System.Linq;
using FTACoreSL.Model;
using FTACoreSL.Constant;
using System.Collections.Generic;

namespace FTACoreSL.Algorithm
{
    public class ProbabilityCalculator
    {
        /// <summary>
        /// ������м��¼������¼���������,ͬʱΪ�˼���ṹ��Ҫ�Ƚ�����structValue�ļ���
        /// </summary>
        /// <param name="fTGate">����Ĺ������Žڵ�</param>
        public static void CalculateProbability(FTNodeGate fTGate)
        {
            CalculateProbabilityDirect(fTGate);
            if (fTGate.HasRepeatBasicNodes)
            {
                RecursiveTrackFunction(fTGate);
                CalculateProbabilityByFunction(fTGate);
            }
        }

        #region ֱ�ӷ�������м��¼������¼��������ʣ��������ظ��¼�ʱʹ��
        public static void CalculateProbabilityDirect(FTNodeGate fTGate)
        {
            if (!fTGate.HasRepeatBasicNodes)
            {                
                RecursiveCalculateProbabilityDirect(fTGate); return;
            }
            fTGate.ChildGateNodes.Values.ToList().ForEach(cgnv => CalculateProbabilityDirect(cgnv));
        }        

        private static void RecursiveCalculateProbabilityDirect(FTNodeGate fTGate)
        {
            var cgnvs = fTGate.ChildGateNodes.Values.ToList();
            if (cgnvs.Count > 0) cgnvs.ForEach(cgn => RecursiveCalculateProbabilityDirect(cgn));

            var childNodesValues = (from v in fTGate.ChildNodes.Values select v.Value).ToList();
            var childNodesStructValues = (from v in fTGate.ChildNodes.Values select v.StructValue).ToList();
            if (fTGate.NodeType == FTConstants.Gate_AND)
            {
                fTGate.Value = CalculateANDGateProbability(childNodesValues);
                fTGate.StructValue = CalculateANDGateProbability(childNodesStructValues);
            }
            if (fTGate.NodeType == FTConstants.Gate_OR)
            {
                fTGate.Value = CalculateORGateProbability(childNodesValues);
                fTGate.StructValue = CalculateORGateProbability(childNodesStructValues);
            }
        }

        private static double? CalculateANDGateProbability(List<double?> valueList)
        {
            return valueList.Aggregate((x, y) => x * y);
        }
        /// <summary>
        /// ��CalculateORGateProbability����һ��
        /// </summary>
        /// <param name="valueList"></param>
        /// <returns></returns>
        private static double? CalculateORGateProbabilityWithInverse(List<double?> valueList)
        {
            double? productSum = 1;
            valueList.ForEach(probability => productSum *= probability);
            return 1 - productSum;  
        }

        private static double? CalculateORGateProbability(List<double?> valueList)
        {
            double? funcSum = 0; double? funcItem = 1; double? funcItemi = 1;
            valueList.ForEach(value =>
            {
                funcItem = value * funcItemi; funcItemi *= 1 - value; funcSum += funcItem;
            });
            return funcSum;
        }
        #endregion

        #region ����Ƕ����ڵ�Ľṹ����(�����ڵ���ֱ�ӷ�����)
        /// <summary>
        /// ����Ƕ����ڵ�Ľṹ�����������,�÷�����Ҫ��ֱ�ӷ���������
        /// </summary>
        /// <param name="fTGate">����Ĺ������Žڵ�</param>
        public static void RecursiveTrackFunction(FTNodeGate fTGate)
        {
           // var cgnvs = fTGate.ChildGateNodes.Values.Where(cgn => cgn.Value == null).ToList();
            var cgnvs = fTGate.ChildGateNodes.Values.Where(cgn => cgn.HasRepeatBasicNodes).ToList();
            if (cgnvs.Count > 0) cgnvs.ForEach(cgn => RecursiveTrackFunction(cgn));
            if (fTGate.NodeType == FTConstants.Gate_AND) TrackANDGateFunction(fTGate);
            if (fTGate.NodeType == FTConstants.Gate_OR) TrackORGateFunction(fTGate);
        }

        private static void TrackANDGateFunction(FTNodeGate fTGate)
        {
            var cgns = fTGate.ChildNodes.Values;
            cgns.Where(chn => chn.HasRepeatBasicNodes).OrderBy(chn => chn.ChildNodes.Count).ToList().ForEach(chn =>
            {
                fTGate.LowFunction.AddRange(UnionFunctionLogic(fTGate.HighFunction, chn.LowFunction));
                fTGate.HighFunction = UnionFunctionLogic(fTGate.HighFunction, chn.HighFunction);
            });
            cgns.Where(chn => !chn.HasRepeatBasicNodes).ToList().ForEach(chn =>
            {
                fTGate.LowFunction.AddRange(UnionFunctionLogic(fTGate.HighFunction, chn.LowFunction));
                fTGate.HighFunction = UnionFunctionLogic(fTGate.HighFunction, chn.HighFunction);
            });
        }

        private static void TrackORGateFunction(FTNodeGate fTGate)
        {
            var chns = fTGate.ChildNodes.Values;
            chns.Where(chn => chn.HasRepeatBasicNodes).OrderBy(chn => chn.ChildNodes.Count).ToList().ForEach(chn =>
            {
                fTGate.HighFunction.AddRange(UnionFunctionLogic(fTGate.LowFunction, chn.HighFunction));
                fTGate.LowFunction = UnionFunctionLogic(fTGate.LowFunction, chn.LowFunction);
            });
            chns.Where(chn => !chn.HasRepeatBasicNodes).ToList().ForEach(chn =>
            {
                fTGate.HighFunction.AddRange(UnionFunctionLogic(fTGate.LowFunction, chn.HighFunction));
                fTGate.LowFunction = UnionFunctionLogic(fTGate.LowFunction, chn.LowFunction);
            });
        }

        private static List<List<NodeWithSymbol>> UnionFunctionLogic(List<List<NodeWithSymbol>> list1, List<List<NodeWithSymbol>> list2)
        {
            if (list1.Count == 0) return list2;
            List<List<NodeWithSymbol>> tempList = new List<List<NodeWithSymbol>>();            
            list1.ForEach(l1 => list2.ForEach(l2 => { if (!IsCollided(l1, l2)) tempList.Add(l1.Union(l2).ToList()); }));           
            return tempList;
        }

        /// <summary>
        /// �ж�Ҫ��ϵ�����·���Ƿ���ͬһ�ڵ���ͬһ�ڵ���0,1��֧�϶�����
        /// </summary>
        /// <param name="nodeList1">·��</param>
        /// <param name="nodeList2">·��</param>
        /// <returns></returns>
        private static bool IsCollided(List<NodeWithSymbol> nodeList1, List<NodeWithSymbol> nodeList2)
        {
            var nL1T = from nl1 in nodeList1.Where(n => n.HOrL == true) select nl1.Node.Key;
            var nL2F = from nl2 in nodeList2.Where(n => n.HOrL == false) select nl2.Node.Key;
            var nL1F = from nl1 in nodeList1.Where(n => n.HOrL == false) select nl1.Node.Key;
            var nL2T = from nl2 in nodeList2.Where(n => n.HOrL == true) select nl2.Node.Key;
            return nL1T.Intersect(nL2F).Count() + nL1F.Intersect(nL2T).Count() > 0;
        }
        #endregion

        #region �����м��¼��붥�¼���������(�ṹ������)
        public static void CalculateProbabilityByFunction(FTNodeGate fTGate)
        {
            if (fTGate.Value == null)
                fTGate.Value = 0;
            if (fTGate.StructValue == null)
                fTGate.StructValue = 0;
            fTGate.HighFunction.ForEach(fh =>
            {
                fTGate.Value += (from f in fh select f.HOrL ? f.Node.Value : 1 - f.Node.Value).Aggregate((x, y) => x * y);
                fTGate.StructValue += (from f in fh select f.Node.StructValue).Aggregate((x, y) => x * y);
            });
        }
        #endregion
    }
}
