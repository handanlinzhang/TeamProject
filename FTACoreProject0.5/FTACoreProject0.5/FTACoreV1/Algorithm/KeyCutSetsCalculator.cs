using System.Linq;
using System.Collections.Generic;
using FTACoreSL.Model;
using FTACoreSL.Constant;

namespace FTACoreSL.Algorithm
{
    public class KeyCutSetsCalculator
    {
        public static void CalculateKeyCutSets(FTNodeGate fTGate)
        {
            RecursiveTrackCutSets(fTGate);
            if (fTGate.Value > 0) CalculateCutSetsImportance(fTGate);
        }

        #region �����м��¼������¼��
        /// <summary>
        /// �������нڵ�ĸ
        /// </summary>
        /// <param name="fTGate">����Ĺ������Žڵ�</param>
        public static void RecursiveTrackCutSets(FTNodeGate fTGate)
        {
            var cgnvs = fTGate.ChildGateNodes.Values.ToList();
            if (cgnvs.Count > 0) cgnvs.ForEach(cgn => RecursiveTrackCutSets(cgn));
            if (fTGate.NodeType == FTConstants.Gate_AND) TrackANDGateCutsets(fTGate);
            if (fTGate.NodeType == FTConstants.Gate_OR) TrackORGateCutsets(fTGate);
        }

        private static void TrackANDGateCutsets(FTNodeGate fTGate)
        {   //�������ظ��¼��Ľڵ�鵽һ�������м���
            var chds = fTGate.ChildNodes.Values;

            //����ӽڵ�ĸ����˳���65000��������Ϊ��������ʱ��ȥ�߽׸�������
            var subCutsetCount = (from chd in chds select chd.Cutsets.Count).Aggregate((x, y) => x * y);
            if (subCutsetCount > GlobalConst.BigCutsetUpbound) { TrackANDGateKeyCutsets(fTGate); return; }

            //�������������Ϊʲô������hasrepeatnodes�������ж�
            if (fTGate.RepeatingDescendantBasicNodes != null)
            {
                var dependentChilds = chds.Where(dChd => dChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() > 0).ToList();
                dependentChilds.OrderBy(dChd => dChd.Cutsets.Count).ToList().ForEach(dChd => fTGate.Cutsets = UnionCutSetANDLogic(fTGate.Cutsets, dChd.Cutsets));
            }

            var independChilds = (fTGate.RepeatingDescendantBasicNodes != null) ?
                chds.Where(iChd => iChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() == 0) :
                chds;
            independChilds.ToList().ForEach(iChd => fTGate.Cutsets = UnionCutSetANDLogicDirect(fTGate.Cutsets, iChd.Cutsets));
        }

        private static void TrackORGateCutsets(FTNodeGate fTGate)
        {
            var chds = fTGate.ChildNodes.Values;

            //����ӽڵ�ĸ����ӳ���65000��������Ϊ��������ʱ��ȥ�߽׸�������
            var subCutsetCount = (from chd in chds select chd.Cutsets.Count).Sum();
            if (subCutsetCount > GlobalConst.BigCutsetUpbound) { TrackORGateKeyCutsets(fTGate); return; }

            if (fTGate.RepeatingDescendantBasicNodes != null)
            {
                var dependentChilds = chds.Where(dChd => dChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() > 0).ToList();
                dependentChilds.OrderBy(dChd => dChd.Cutsets.Count).ToList().ForEach(cgn => fTGate.Cutsets = UnionCutSetORLogic(fTGate.Cutsets, cgn.Cutsets));
            }
            var independChilds = (fTGate.RepeatingDescendantBasicNodes != null) ?
                chds.Where(iChd => iChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() == 0) :
                chds;
            independChilds.ToList().ForEach(iChd => fTGate.Cutsets = UnionCutSetORLogicDirect(fTGate.Cutsets, iChd.Cutsets));
        }

        #region ������ʱѡ��ͽ׸���м���
        private static void TrackANDGateKeyCutsets(FTNodeGate fTGate)
        {   //�������ظ��¼��Ľڵ�鵽һ�������м���
            var chds = fTGate.ChildNodes.Values;
            var subCutsetCount = (from chd in chds select chd.Cutsets.Count).Aggregate((x, y) => x * y);
            if (subCutsetCount > 65000) { }
            if (fTGate.RepeatingDescendantBasicNodes != null)
            {
                var dependentChilds = chds.Where(dChd => dChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() > 0).ToList();
                dependentChilds.OrderBy(dChd => dChd.Cutsets.Count).ToList().ForEach(dChd => fTGate.Cutsets = UnionCutSetANDLogic(fTGate.Cutsets, dChd.Cutsets));
            }

            var independChilds = (fTGate.RepeatingDescendantBasicNodes != null) ?
                chds.Where(iChd => iChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() == 0) :
                chds;
            independChilds.ToList().ForEach(iChd => fTGate.Cutsets = UnionCutSetANDLogicDirect(fTGate.Cutsets, iChd.Cutsets));
        }
        private static void TrackORGateKeyCutsets(FTNodeGate fTGate)
        {
            var chds = fTGate.ChildNodes.Values;
            if (fTGate.RepeatingDescendantBasicNodes != null)
            {
                var dependentChilds = chds.Where(dChd => dChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() > 0).ToList();
                dependentChilds.OrderBy(dChd => dChd.Cutsets.Count).ToList().ForEach(cgn => fTGate.Cutsets = UnionCutSetORLogic(fTGate.Cutsets, cgn.Cutsets));
            }
            var independChilds = (fTGate.RepeatingDescendantBasicNodes != null) ?
                chds.Where(iChd => iChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() == 0) :
                chds;
            independChilds.ToList().ForEach(iChd => fTGate.Cutsets = UnionCutSetORLogicDirect(fTGate.Cutsets, iChd.Cutsets));
        }
        #endregion

        //�������ϵ����߼������ظ��¼��Ĵ���
        private static List<CutSet> UnionCutSetANDLogic(List<CutSet> aList, List<CutSet> bList)
        {
            if (aList == null || aList.Count == 0) return bList;
            var combineList = new List<CutSet>();
            aList.ForEach(aLi =>
            {
                //�ҳ�blist��������ali���������ҳ��������Ӧ��ali��blist�еĸȫ�����ÿ����ˣ�����Ҳ����������Ŷ�blist���е���foreach����
                var sbLi = bList.Where(bLi => aLi.Cutset.Intersect(bLi.Cutset).Count() == bLi.Cutset.Count);
                if (sbLi.Count() > 0) //{ if (!combineList.Contains(aLi)) combineList.Add(aLi); }
                {
                    if (!combineList.Contains(aLi))
                    {
                        combineList.RemoveAll(cs => cs.Cutset.Intersect(aLi.Cutset).Count() == aLi.Cutset.Count);
                        combineList.Add(aLi);
                    }
                }
                if (sbLi.Count() == 0)
                {
                    bList.ForEach(bLi =>
                    {
                        if (bLi.Cutset.Intersect(aLi.Cutset).Count() == aLi.Cutset.Count)
                        {
                            combineList.RemoveAll(cbl => cbl.Cutset.Intersect(bLi.Cutset).Count() == bLi.Cutset.Count);
                            if (!combineList.Contains(aLi)) combineList.Add(bLi);
                        }
                        else
                        {
                            var unionList = aLi.Cutset.Union(bLi.Cutset).ToList();
                            if (!(from cl in combineList select cl.Cutset).Contains(unionList))
                            {
                                var csInSet = combineList.Where(cs => cs.Cutset.Intersect(unionList).Count() == cs.Cutset.Count);
                                if (csInSet.Count() == 0) { combineList.Add(new CutSet() { Cutset = unionList }); }
                            }
                        }
                    });
                }
            });
            aList.Clear(); bList.Clear();
            return combineList;
        }
        //�������ϵ����߼������ظ��¼��Ĵ���
        private static List<CutSet> UnionCutSetANDLogicDirect(List<CutSet> aList, List<CutSet> bList)
        {
            if (aList == null || aList.Count == 0) return bList;
            var combineList = new List<CutSet>();
            aList.ForEach(aLi => combineList.AddRange(from bLi in bList select new CutSet() { Cutset = bLi.Cutset.Union(aLi.Cutset).ToList() }));
            aList.Clear(); bList.Clear();
            return combineList;
        }

        //�������ϵĻ��߼������ظ��¼��Ĵ���
        private static List<CutSet> UnionCutSetORLogic(List<CutSet> aList, List<CutSet> bList)
        {
            if (aList == null || aList.Count == 0) return bList;
            var combineList = new List<CutSet>();
            aList.ForEach(aLi =>
            {
                bList.RemoveAll(bLi => bLi.Cutset.Intersect(aLi.Cutset).Count() == aLi.Cutset.Count);
                var xCount = bList.Where(bli => bli.Cutset.Intersect(aLi.Cutset).Count() == bli.Cutset.Count);
                if (xCount.Count() > 0) return;
                if (!combineList.Contains(aLi)) combineList.Add(aLi);
            });
            combineList.AddRange(bList);
            aList.Clear(); bList.Clear();
            return combineList;
        }

        //�������ϵĻ��߼������ظ��¼��Ĵ���
        private static List<CutSet> UnionCutSetORLogicDirect(List<CutSet> aList, List<CutSet> bList)
        {
            return aList.Union(bList).ToList();
        }
        #endregion

        #region ������Ҫ�Ⱥ���ظ��Ҫ��
        public static void CalculateCutSetsImportance(FTNodeGate fTGate)
        {
            fTGate.Cutsets.ForEach(cs => cs.CutsetImportance = (from c in cs.Cutset select c.Value).Aggregate((x, y) => x * y) / fTGate.Value);
            fTGate.DescendantsBasicNodes.ToList().ForEach(dbn =>
                    dbn.Value.RelCSImportance = (from c in fTGate.Cutsets.Where(cs => cs.Cutset.Contains(dbn.Key)).ToList()
                                                 select c.CutsetImportance).Sum());
        }
        #endregion
    }
}
