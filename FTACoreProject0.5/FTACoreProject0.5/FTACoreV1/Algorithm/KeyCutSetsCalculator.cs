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

        #region 计算中间事件及顶事件割集
        /// <summary>
        /// 计算所有节点的割集
        /// </summary>
        /// <param name="fTGate">传入的故障树门节点</param>
        public static void RecursiveTrackCutSets(FTNodeGate fTGate)
        {
            var cgnvs = fTGate.ChildGateNodes.Values.ToList();
            if (cgnvs.Count > 0) cgnvs.ForEach(cgn => RecursiveTrackCutSets(cgn));
            if (fTGate.NodeType == FTConstants.Gate_AND) TrackANDGateCutsets(fTGate);
            if (fTGate.NodeType == FTConstants.Gate_OR) TrackORGateCutsets(fTGate);
        }

        private static void TrackANDGateCutsets(FTNodeGate fTGate)
        {   //将带有重复事件的节点归到一起来进行计算
            var chds = fTGate.ChildNodes.Values;

            //如果子节点的割集数相乘超过65000个，则视为超大割集，此时舍去高阶割集不予计算
            var subCutsetCount = (from chd in chds select chd.Cutsets.Count).Aggregate((x, y) => x * y);
            if (subCutsetCount > GlobalConst.BigCutsetUpbound) { TrackANDGateKeyCutsets(fTGate); return; }

            //？？？这个东西为什么不是用hasrepeatnodes来进行判断
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

            //如果子节点的割集数相加超过65000个，则视为超大割集，此时舍去高阶割集不予计算
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

        #region 处理超大割集时选择低阶割集进行计算
        private static void TrackANDGateKeyCutsets(FTNodeGate fTGate)
        {   //将带有重复事件的节点归到一起来进行计算
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

        //两个集合的与逻辑：带重复事件的处理
        private static List<CutSet> UnionCutSetANDLogic(List<CutSet> aList, List<CutSet> bList)
        {
            if (aList == null || aList.Count == 0) return bList;
            var combineList = new List<CutSet>();
            aList.ForEach(aLi =>
            {
                //找出blist中能吸收ali的项，如果能找出来，则对应于ali，blist中的割集全部不用考虑了；如果找不出来，接着对blist进行单个foreach考虑
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
        //两个集合的与逻辑：无重复事件的处理
        private static List<CutSet> UnionCutSetANDLogicDirect(List<CutSet> aList, List<CutSet> bList)
        {
            if (aList == null || aList.Count == 0) return bList;
            var combineList = new List<CutSet>();
            aList.ForEach(aLi => combineList.AddRange(from bLi in bList select new CutSet() { Cutset = bLi.Cutset.Union(aLi.Cutset).ToList() }));
            aList.Clear(); bList.Clear();
            return combineList;
        }

        //两个集合的或逻辑：带重复事件的处理
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

        //两个集合的或逻辑：无重复事件的处理
        private static List<CutSet> UnionCutSetORLogicDirect(List<CutSet> aList, List<CutSet> bList)
        {
            return aList.Union(bList).ToList();
        }
        #endregion

        #region 计算割集重要度和相关割集重要度
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
