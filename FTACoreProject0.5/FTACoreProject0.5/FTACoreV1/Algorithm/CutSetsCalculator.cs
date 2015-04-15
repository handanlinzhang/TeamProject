using System.Linq;
using System.Collections.Generic;
using FTACoreSL.Model;
using FTACoreSL.Constant;

namespace FTACoreSL.Algorithm
{
    #region �޸ļ�¼��
    //�޸�ʱ�䣺2013-10-18
    //@�޸����ݣ�ע�͵�aList.Clear()�� bList.Clear()
    //@�޸�ԭ�򣺶��ڴ��ڿ�¡�������Ĺ������������ȷ���Դ������Դ�����Ľڵ��ڵڶ��㣩��Ȼ�����һ������Դ���õĽڵ�ʱ���ὫԴ�����ĸ��գ�Ӱ��������

    //@�޸�ʱ�䣺2013-10-24
    //@�޸�ԭ�򣺶��ڴ����������õĹ�����������ո������ĸ
    //@�޸����ݣ������м��¼�Ϊ���û�Դ�����丱������
    #endregion
    public class CutSetsCalculator
    {        
        public static void CalculateCutSets(FTNodeGate fTGate)
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

            //����ӽڵ�ĸ����˳���BigCutsetUpbound��������Ϊ��������ʱ��ȥ�߽׸�������
            var subCutsetCount = (from chd in chds select chd.Cutsets.Count).Aggregate((x, y) => x * y);
            if (subCutsetCount > GlobalConst.BigCutsetUpbound) { TrackANDGateKeyCutsets(fTGate); return; }

            if (fTGate.RepeatingDescendantBasicNodes != null)
            {
                var dependentChilds = chds.Where(dChd => dChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() > 0).ToList();
                dependentChilds.OrderBy(dChd => dChd.Cutsets.Count).ToList().ForEach(dChd =>
                {
                    if (CalcAndGateCutSetNumber(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(dChd)) < GlobalConst.CutSetElementLimit)
                        fTGate.Cutsets = UnionCutSetANDLogic(fTGate.Cutsets, dChd.IsCLone ? getDeepCopy(dChd.Cutsets) : dChd.Cutsets);
                    else
                    {
                        fTGate.CutSetOrderAndNumber = EstimateAndGateCutSet(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(dChd));
                        fTGate.Cutsets.Clear();
                    }
                });
            }

            var independChilds = (fTGate.RepeatingDescendantBasicNodes != null) ?
                chds.Where(iChd => iChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() == 0) :
                chds;
            
            independChilds.ToList().ForEach(iChd =>
            {
                if (CalcAndGateCutSetNumber(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(iChd)) < GlobalConst.CutSetElementLimit)
                    fTGate.Cutsets = UnionCutSetANDLogicDirect(fTGate.Cutsets, iChd.IsCLone ? getDeepCopy(iChd.Cutsets) : iChd.Cutsets);
                else
                {
                    fTGate.CutSetOrderAndNumber = EstimateAndGateCutSet(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(iChd));
                    fTGate.Cutsets.Clear();
                }
            });
        }      
        private static void TrackORGateCutsets(FTNodeGate fTGate)
        {
            var chds = fTGate.ChildNodes.Values;

            //����ӽڵ�ĸ����ӳ���BigCutsetUpbound��������Ϊ��������ʱ��ȥ�߽׸�������
            var subCutsetCount = (from chd in chds select chd.Cutsets.Count).Sum();
            if (subCutsetCount > GlobalConst.BigCutsetUpbound) { TrackORGateKeyCutsets(fTGate); return; }

            if (fTGate.RepeatingDescendantBasicNodes != null)
            {
                var dependentChilds = chds.Where(dChd => dChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() > 0).ToList();
                dependentChilds.OrderBy(dChd => dChd.Cutsets.Count).ToList().ForEach(cgn => 
                {
                    if (CalcORGateCutSetNumber(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(cgn)) < GlobalConst.CutSetElementLimit)
                        fTGate.Cutsets = UnionCutSetORLogic(fTGate.Cutsets, cgn.IsCLone ? getDeepCopy(cgn.Cutsets) : cgn.Cutsets);
                    else
                    {
                        fTGate.CutSetOrderAndNumber = EstimateOrGateCutSet(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(cgn));
                        fTGate.Cutsets.Clear();
                     }
                });
            }
            var independChilds = (fTGate.RepeatingDescendantBasicNodes != null) ?
                chds.Where(iChd => iChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() == 0) :
                chds;
          
            independChilds.ToList().ForEach(iChd =>
            {
                if (CalcORGateCutSetNumber(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(iChd)) < GlobalConst.CutSetElementLimit)
                    fTGate.Cutsets = UnionCutSetORLogicDirect(fTGate.Cutsets, iChd.IsCLone ? getDeepCopy(iChd.Cutsets) : iChd.Cutsets);
                else
                {
                    fTGate.CutSetOrderAndNumber = EstimateOrGateCutSet(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(iChd));
                    fTGate.Cutsets.Clear();
                }
            });
        }

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
                                var csOutlSet = combineList.Where(cs => cs.Cutset.Intersect(unionList).Count() == unionList.Count);
                                if (csInSet.Count() == 0 && csOutlSet.Count() == 0)
                                {
                                    combineList.Add(new CutSet() { Cutset = unionList });
                                }
                                else if (csInSet.Count() == 0 && csOutlSet.Count() != 0)
                                {
                                    combineList.RemoveAll(cs => cs.Cutset.Intersect(unionList).Count() == unionList.Count);
                                    combineList.Add(new CutSet() { Cutset = unionList });
                                }
                            }
                        }
                    });
                }
            });
            aList.Clear(); bList.Clear();
            return combineList;
        }
        //�������ϵ����߼������ظ��¼��Ĵ���
        //bListΪ��ʱ�����ʲô�����
        private static List<CutSet> UnionCutSetANDLogicDirect(List<CutSet> aList, List<CutSet> bList)
        {            
            if (aList == null || aList.Count == 0) return bList;
            var combineList = new List<CutSet>();
           
            aList.ForEach(aLi =>
                {   
                   combineList.AddRange(from bLi in bList select new CutSet() { Cutset = bLi.Cutset.Union(aLi.Cutset).ToList()});
                });
                
            aList.Clear();
            bList.Clear();
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
            aList.Clear();
            bList.Clear();
            return combineList;
        }
        //�������ϵĻ��߼������ظ��¼��Ĵ���
        private static List<CutSet> UnionCutSetORLogicDirect(List<CutSet> aList, List<CutSet> bList)
        {
            return aList.Union(bList).ToList();
        }
        #endregion

        #region ������ʱ�������Ľ�������Ӧ�����ĸ����
        private static void TrackANDGateKeyCutsets(FTNodeGate fTGate)
        {
            var chds = fTGate.ChildNodes.Values;
            chds.ToList().ForEach(chd => fTGate.CutsetsRank = CalculateANDGateCutsetsRank(fTGate.CutsetsRank, chd.CutsetsRank));
            return;

            if (fTGate.RepeatingDescendantBasicNodes != null)
            {
                var dependentChilds = chds.Where(dChd => dChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() > 0).ToList();
                dependentChilds.OrderBy(dChd => dChd.Cutsets.Count).ToList().ForEach(dChd =>   
                {
                    if (CalcAndGateCutSetNumber(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(dChd)) < GlobalConst.CutSetElementLimit)
                        fTGate.Cutsets = UnionKeyCutSetANDLogic(fTGate.Cutsets, dChd.IsCLone ? getDeepCopy(dChd.Cutsets) : dChd.Cutsets);
                    else
                    {
                        fTGate.CutSetOrderAndNumber = EstimateAndGateCutSet(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(dChd));
                        fTGate.Cutsets.Clear();
                    }
                });   
            }

            var independChilds = (fTGate.RepeatingDescendantBasicNodes != null) ?
                chds.Where(iChd => iChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() == 0) :
                chds;
          
            independChilds.ToList().ForEach(iChd => 
            {
                if (CalcAndGateCutSetNumber(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(iChd)) < GlobalConst.CutSetElementLimit)
                    fTGate.Cutsets = UnionKeyCutSetANDLogicDirect(fTGate.Cutsets, iChd.IsCLone ? getDeepCopy(iChd.Cutsets) : iChd.Cutsets);
                else
                {
                    fTGate.CutSetOrderAndNumber = EstimateAndGateCutSet(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(iChd));
                    fTGate.Cutsets.Clear();
                }
            });
        }

        private static void TrackORGateKeyCutsets(FTNodeGate fTGate)
        {
            var chds = fTGate.ChildNodes.Values;
            chds.ToList().ForEach(chd => fTGate.CutsetsRank = CalculateORGateCutsetsRank(fTGate.CutsetsRank, chd.CutsetsRank));
            return;

            if (fTGate.RepeatingDescendantBasicNodes != null)
            {
                var dependentChilds = chds.Where(dChd => dChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() > 0).ToList();
                dependentChilds.OrderBy(dChd => dChd.Cutsets.Count).ToList().ForEach(cgn =>
                    {
                        //int temp = CalcORGateCutSetNumber(fTGate, (FTNodeGate)cgn);
                        if (CalcORGateCutSetNumber(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(cgn)) < GlobalConst.CutSetElementLimit)
                            fTGate.Cutsets = UnionKeyCutSetORLogic(fTGate.Cutsets, cgn.IsCLone ? getDeepCopy(cgn.Cutsets) : cgn.Cutsets);
                        else
                        {
                            fTGate.CutSetOrderAndNumber = EstimateOrGateCutSet(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(cgn));
                            fTGate.Cutsets.Clear();
                        }
                    });

            }
            var independChilds = (fTGate.RepeatingDescendantBasicNodes != null) ?
                chds.Where(iChd => iChd.DescendantsBasicNodes.Keys.Intersect(fTGate.RepeatingDescendantBasicNodes.Keys).Count() == 0) :
                chds;

            independChilds.ToList().ForEach(iChd =>
                {
                    //int temp = CalcORGateCutSetNumber(fTGate, (FTNodeGate)iChd);
                    if (CalcORGateCutSetNumber(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(iChd)) < GlobalConst.CutSetElementLimit)
                        fTGate.Cutsets = UnionKeyCutSetORLogicDirect(fTGate.Cutsets, iChd.IsCLone ? getDeepCopy(iChd.Cutsets) : iChd.Cutsets);
                    else
                    {
                        fTGate.CutSetOrderAndNumber = EstimateOrGateCutSet(GetCutOrderAndNumber(fTGate), GetCutOrderAndNumber(iChd));
                        fTGate.Cutsets.Clear();
                    }
                });
        }

        private static Dictionary<int, ulong> CalculateANDGateCutsetsRank(Dictionary<int, ulong> aCount, Dictionary<int, ulong> bCount)
        {
            if (aCount == null) return bCount;
            if (bCount.Count == 0) return aCount;
            Dictionary<int, ulong> resultCount = new Dictionary<int, ulong>();
            aCount.Keys.ToList().ForEach(ak =>
                bCount.Keys.ToList().ForEach(bk =>
                    {
                        if (resultCount.ContainsKey(ak + bk)) resultCount[ak + bk] += aCount[ak] * bCount[bk];
                        else resultCount.Add(ak + bk, aCount[ak] * bCount[bk]);
                    })
                );
            return resultCount;
        }

        private static Dictionary<int, ulong> CalculateORGateCutsetsRank(Dictionary<int, ulong> aCount, Dictionary<int, ulong> bCount)
        {
            if (aCount == null) return bCount;
            if (bCount.Count == 0) return aCount;
            bCount.Keys.ToList().ForEach(bk =>
            {
                if (aCount.ContainsKey(bk)) aCount[bk] += bCount[bk];
                else aCount.Add(bk, bCount[bk]);
            });
            return aCount;
        }

        //�������ϵ����߼������ظ��¼��Ĵ���
        private static List<CutSet> UnionKeyCutSetANDLogic(List<CutSet> aList, List<CutSet> bList)
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
            aList.Clear();
            bList.Clear();
            return combineList;
        }
        //�������ϵ����߼������ظ��¼��Ĵ���
        private static List<CutSet> UnionKeyCutSetANDLogicDirect(List<CutSet> aList, List<CutSet> bList)
        {
            if (aList == null || aList.Count == 0) return bList;
            var combineList = new List<CutSet>();
            aList.ForEach(aLi => combineList.AddRange(from bLi in bList select new CutSet() { Cutset = bLi.Cutset.Union(aLi.Cutset).ToList() }));
            aList.Clear();
            bList.Clear();
            return combineList;
        }

        //�������ϵĻ��߼������ظ��¼��Ĵ���
        private static List<CutSet> UnionKeyCutSetORLogic(List<CutSet> aList, List<CutSet> bList)
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
            aList.Clear();
            bList.Clear();
            return combineList;
        }
        //�������ϵĻ��߼������ظ��¼��Ĵ���
        private static List<CutSet> UnionKeyCutSetORLogicDirect(List<CutSet> aList, List<CutSet> bList)
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

        private static List<CutSet> getDeepCopy(List<CutSet> cList)
        {
            List<CutSet> cutSetCopy = new List<CutSet>();
            cutSetCopy.AddRange(cList);
            return cutSetCopy;
        }

        private static Dictionary<int, int> GetCutOrderAndNumber(FTNodeBase ft)
        {
            if (ft.Cutsets == null) return ft.CutSetOrderAndNumber;
            Dictionary<int, int> temp = new Dictionary<int, int>();
            ft.Cutsets.ForEach(c =>
            {
                if (temp.ContainsKey(c.Cutset.Count))
                    temp[c.Cutset.Count]++;
                else
                    temp.Add(c.Cutset.Count, 1);
            });
            return temp;
        }

        private static Dictionary<int, int> EstimateOrGateCutSet(Dictionary<int, int> aDic, Dictionary<int, int> bDic)
        {
            if (aDic == null || aDic.Count == 0) return bDic;

            foreach (KeyValuePair<int, int> kv in bDic)
            {
                if (aDic.ContainsKey(kv.Key))
                    aDic[kv.Key] += kv.Value;
                else
                    aDic.Add(kv.Key, kv.Value);
            }
            return aDic;
        }
        private static Dictionary<int, int> EstimateAndGateCutSet(Dictionary<int, int> aDic, Dictionary<int, int> bDic)
        {
            if (aDic == null || aDic.Count == 0) return bDic;
            Dictionary<int, int> combineDic = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int> dic in aDic)
            {
                foreach (KeyValuePair<int, int> kv in bDic)
                {
                    if (combineDic.ContainsKey(kv.Key + dic.Key))
                        combineDic[kv.Key + dic.Key] += kv.Value * dic.Value;
                    else
                        combineDic.Add(kv.Key + dic.Key, kv.Value * dic.Value);
                }
            }
            return combineDic;
        }
        private static int CalcORGateCutSetNumber(Dictionary<int, int> aDic, Dictionary<int, int> bDic)
        {
            if (aDic == null || aDic.Count == 0)
                return (from s in bDic select s.Key * s.Value).Sum();
            else
                return (from f in aDic select f.Key * f.Value).Sum() + (from s in bDic select s.Key * s.Value).Sum();
        }
        private static int CalcAndGateCutSetNumber(Dictionary<int, int> aDic, Dictionary<int, int> bDic)
        {
            if (aDic == null || aDic.Count == 0) return (from t in bDic select t.Key * t.Value).Sum();
            int sum = 0;

            foreach (KeyValuePair<int, int> k in aDic)
            {
                foreach (KeyValuePair<int, int> v in bDic)
                {
                    sum += (k.Key + v.Key) * (k.Value * v.Value);
                }
            }
            return sum;
        }
    }
}
