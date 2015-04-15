using System.Linq;
using FTACoreSL.Model;
using FTACoreSL.Constant;
using System.Collections.Generic;
using System;

namespace FTACoreSL.Algorithm
{
    //@修改时间：2013-10-15
    //@修改人：李志峰
    //@修改原因：底事件发生概率为1时，分式的分母为0，应修改计算概率重要度的实现公式
    //@修改内容：修改CalculateORGateImportance函数中的计算概率重要度的内容
    public class ImportanceCalculator
    {
        public static void CalculateImportance(FTNodeGate fTGate)
        {
            CalculateImportanceDirect(fTGate);
            if (fTGate.HasRepeatBasicNodes)
                CalculateImportanceByFunction(fTGate);


            foreach (KeyValuePair<FTNodeBase, BasicNodeImportance> kvnode in fTGate.DescendantsBasicNodes)
            {
                //kvnode.Value.BM = kvnode.Value.RiskAchievementWorth - kvnode.Value.RiskReductionWorth;
                kvnode.Value.RiskReductionWorth = fTGate.Value / kvnode.Value.RiskReductionWorth;
                kvnode.Value.RiskAchievementWorth = kvnode.Value.RiskAchievementWorth / fTGate.Value;
                kvnode.Value.FV = 1 - (1 / kvnode.Value.RiskReductionWorth);
                kvnode.Value.BM = fTGate.Value * (kvnode.Value.RiskAchievementWorth - 1 / kvnode.Value.RiskReductionWorth);
                //kvnode.Value.BM = kvnode.Value.RiskAchievementWorth - kvnode.Value.RiskReductionWorth;
            }
        }

        #region 直接法计算重要度(不包含有重复事件的树或独立子树)
        public static void CalculateImportanceDirect(FTNodeGate fTGate)
        {
            if (!fTGate.HasRepeatBasicNodes)
            {
                RecursiveCalculateImportanceDirect(fTGate); return;
            }
            fTGate.ChildGateNodes.Values.ToList().ForEach(cgnv => CalculateImportanceDirect(cgnv));
        }

        public static void RecursiveCalculateImportanceDirect(FTNodeGate fTGate)
        {
            var cgnvs = fTGate.ChildGateNodes.Values.ToList();
            if (cgnvs.Count > 0) cgnvs.ForEach(cgn => RecursiveCalculateImportanceDirect(cgn));
            if (fTGate.NodeType == FTConstants.Gate_AND) CalculateANDGateImportance(fTGate);
            if (fTGate.NodeType == FTConstants.Gate_OR) CalculateORGateImportance(fTGate);
        }

        private static void CalculateANDGateImportance(FTNodeGate fTGate)
        {

            double? tempHStr = (from cbnv in fTGate.ChildNodes.Values select cbnv.StructValue).Aggregate((x, y) => x * y);
            fTGate.DescendantsBasicNodes.ToList().ForEach(dbn =>
            {
                //层次得到重要度计算的辅助变量
                var nodeWithSameBasic = (from cgn in fTGate.ChildNodes.Values
                                         where cgn.DescendantsBasicNodes.Keys.Contains(dbn.Key) || cgn == dbn.Key
                                         select cgn).FirstOrDefault();
                //会出现select的序列为空的情况，
                double? tempHPrb;
                List<double?> valueSequence = (from cgn in fTGate.ChildNodes.Values
                                               where !cgn.DescendantsBasicNodes.Keys.Contains(dbn.Key) && cgn != dbn.Key
                                               select cgn.Value).ToList();
                if (valueSequence.Count != 0)
                {
                    tempHPrb = (from item in valueSequence select item).Aggregate((x, y) => x * y);
                }
                else
                {
                    tempHPrb = 1;
                }
                //根据概率重要度的定义，按复合偏导展开
                dbn.Value.PrbImportance = tempHPrb * nodeWithSameBasic.DescendantsBasicNodes[dbn.Key].PrbImportance;
                dbn.Value.StrImportance = tempHStr * nodeWithSameBasic.DescendantsBasicNodes[dbn.Key].StrImportance / nodeWithSameBasic.StructValue;
                dbn.Value.CriImportance = dbn.Value.PrbImportance * dbn.Key.Value / fTGate.Value;

                //dbn.Value.RiskReductionWorth = nodeWithSameBasic.DescendantsBasicNodes[dbn.Key].RiskReductionWorth *
                //                                   (from cbnv in fTGate.ChildNodes.Values where cbnv.Key!=nodeWithSameBasic.Key select cbnv.Value).Aggregate((x,y)=>x*y);
                dbn.Value.RiskReductionWorth = tempHPrb * (nodeWithSameBasic.DescendantsBasicNodes[dbn.Key].RiskReductionWorth - nodeWithSameBasic.Value) + fTGate.Value;
                //dbn.Value.RiskAchievementWorth = nodeWithSameBasic.DescendantsBasicNodes[dbn.Key].RiskAchievementWorth *
                //                                   (from cbnv in fTGate.ChildNodes.Values where cbnv.Key != nodeWithSameBasic.Key select cbnv.Value).Aggregate((x, y) => x * y);
                dbn.Value.RiskAchievementWorth = tempHPrb * (nodeWithSameBasic.DescendantsBasicNodes[dbn.Key].RiskAchievementWorth - nodeWithSameBasic.Value) + fTGate.Value;
            });
        }

        private static void CalculateORGateImportance(FTNodeGate fTGate)
        {
            //var list = (from cbnv in fTGate.ChildNodes.Values select 1 - cbnv.StructValue).ToList();

            double? tempLStr = (from cbnv in fTGate.ChildNodes.Values select 1 - cbnv.StructValue).ToList().Aggregate((x, y) => x * y);

            fTGate.DescendantsBasicNodes.ToList().ForEach(dbn =>
            {
                var nodeWithSameBasic = (from cgn in fTGate.ChildNodes.Values
                                         where cgn.DescendantsBasicNodes.Keys.Contains(dbn.Key) || cgn == dbn.Key
                                         select cgn).FirstOrDefault();
                //逆求结构函数 1- Q = (1-x1)(1-x2)(1-x3) ,由此式可得偏导
                double? tempLPrb;
                List<double?> valueSequence = (from cgn in fTGate.ChildNodes.Values where !cgn.DescendantsBasicNodes.Keys.Contains(dbn.Key) && cgn != dbn.Key select 1 - cgn.Value).ToList();

                tempLPrb = valueSequence.Count != 0 ? valueSequence.Aggregate((x, y) => x * y) : 1;

                dbn.Value.PrbImportance = tempLPrb * nodeWithSameBasic.DescendantsBasicNodes[dbn.Key].PrbImportance;
                dbn.Value.StrImportance = tempLStr * nodeWithSameBasic.DescendantsBasicNodes[dbn.Key].StrImportance / (1 - nodeWithSameBasic.StructValue);
                dbn.Value.CriImportance = dbn.Value.PrbImportance * dbn.Key.Value / fTGate.Value;

                //1、多元函数求偏导
                //2、fTGate = 1-(1-x1)(1-y)(1-z)-----( * )
                //   fTGate1 = 1- (1-x*)(1-y)(1-z)----(**)
                //由上述(*)、(**)求得fTGate1

                dbn.Value.RiskReductionWorth = tempLPrb * (nodeWithSameBasic.DescendantsBasicNodes[dbn.Key].RiskReductionWorth - nodeWithSameBasic.Value) + fTGate.Value;
                dbn.Value.RiskAchievementWorth = tempLPrb * (nodeWithSameBasic.DescendantsBasicNodes[dbn.Key].RiskAchievementWorth - nodeWithSameBasic.Value) + fTGate.Value;
            });
        }
        #endregion

        #region 结构函数法计算重要度(带重复事件)
        public static void CalculateImportanceByFunction(FTNodeGate fTGate)
        {
            if (fTGate.HighFunction.Count == 0) return;
            fTGate.DescendantsBasicNodes.Where(dbn => dbn.Value.Count > 1).ToList().ForEach(dbn =>
            {
                dbn.Value.RiskReductionWorth = 0;
                dbn.Value.RiskAchievementWorth = 0;
                fTGate.HighFunction.ForEach(hf =>
                {
                    //结构函数的分式中应该包含有节点才继续求值
                    if (!(from f in hf select f.Node).Contains(dbn.Key)) return;
                    dbn.Value.RiskReductionWorth += (from f in hf
                                                     select f.HOrL ?
                                                     (f.Node.Key == dbn.Key.Key ? 0 : f.Node.Value) :
                                                     (f.Node.Key == dbn.Key.Key ? 1 : 1 - f.Node.Value))
                                                .Aggregate((x, y) => x * y);
                    dbn.Value.RiskAchievementWorth += (from f in hf
                                                       select f.HOrL ?
                                                       (f.Node.Key == dbn.Key.Key ? 1 : f.Node.Value) :
                                                       (f.Node.Key == dbn.Key.Key ? 0 : 1 - f.Node.Value))
                                                .Aggregate((x, y) => x * y);

                    dbn.Value.PrbImportance += (from f in hf
                                                select f.HOrL ?
                                                (f.Node.Key == dbn.Key.Key ? 1 : f.Node.Value) :
                                                (f.Node.Key == dbn.Key.Key ? -1 : 1 - f.Node.Value))
                                                .Aggregate((x, y) => x * y);
                    dbn.Value.StrImportance += (from f in hf
                                                select f.HOrL ?
                                                (f.Node.Key == dbn.Key.Key ? 1 : f.Node.StructValue) :
                                                (f.Node.Key == dbn.Key.Key ? -1 : 1 - f.Node.StructValue))
                                                .Aggregate((x, y) => x * y);

                });
                dbn.Value.CriImportance = dbn.Value.PrbImportance * dbn.Key.Value / fTGate.Value;
            });

            fTGate.DescendantsBasicNodes.Where(dbn => dbn.Value.Count == 1).ToList().ForEach(dbn =>
            {
                dbn.Value.RiskAchievementWorth = 0;
                dbn.Value.RiskReductionWorth = 0;
                fTGate.HighFunction.OrderBy(hf => hf.Count).ToList().ForEach(hf =>
                {
                    //节点不在结构函数中时，直接跳出
                    var hasSameBasicNodes = (from f in hf.Where(h => h.Node.IsBasic) select f.Node).Contains(dbn.Key);
                    var hasSameGateNodes = (from f in hf.Where(h => !h.Node.IsBasic)
                                            where f.Node.DescendantsBasicNodes.Keys.Contains(dbn.Key)
                                            select f.Node).Count() > 0;
                    if (!hasSameBasicNodes && !hasSameGateNodes) return;

                    dbn.Value.RiskAchievementWorth += (from f in hf
                                                       select f.HOrL ?
                                                       (f.Node.DescendantsBasicNodes.Keys.Contains(dbn.Key) ? f.Node.DescendantsBasicNodes[dbn.Key].RiskAchievementWorth : f.Node.Value) :
                                                       (f.Node.DescendantsBasicNodes.Keys.Contains(dbn.Key) ? 1 - f.Node.DescendantsBasicNodes[dbn.Key].RiskAchievementWorth : 1 - f.Node.Value))
                                                .Aggregate((x, y) => x * y);
                    dbn.Value.RiskReductionWorth += (from f in hf
                                                     select f.HOrL ?
                                                     (f.Node.DescendantsBasicNodes.Keys.Contains(dbn.Key) ? f.Node.DescendantsBasicNodes[dbn.Key].RiskReductionWorth : f.Node.Value) :
                                                     (f.Node.DescendantsBasicNodes.Keys.Contains(dbn.Key) ? 1 - f.Node.DescendantsBasicNodes[dbn.Key].RiskReductionWorth : 1 - f.Node.Value))
                                                .Aggregate((x, y) => x * y);
                    dbn.Value.PrbImportance += (from f in hf
                                                select f.HOrL ?
                                                (f.Node.DescendantsBasicNodes.Keys.Contains(dbn.Key) ? f.Node.DescendantsBasicNodes[dbn.Key].PrbImportance : f.Node.Value) :
                                                (f.Node.DescendantsBasicNodes.Keys.Contains(dbn.Key) ? -f.Node.DescendantsBasicNodes[dbn.Key].PrbImportance : 1 - f.Node.Value))
                                                .Aggregate((x, y) => x * y);

                    dbn.Value.StrImportance += (from f in hf
                                                select f.HOrL ?
                                                (f.Node.DescendantsBasicNodes.Keys.Contains(dbn.Key) ? f.Node.DescendantsBasicNodes[dbn.Key].StrImportance : f.Node.StructValue) :
                                                (f.Node.DescendantsBasicNodes.Keys.Contains(dbn.Key) ? -f.Node.DescendantsBasicNodes[dbn.Key].StrImportance : 1 - f.Node.StructValue))
                                                .Aggregate((x, y) => x * y);
                });
                if (dbn.Value.CriImportance == null)
                    dbn.Value.CriImportance = dbn.Value.PrbImportance * dbn.Key.Value / fTGate.Value;
                else
                    dbn.Value.CriImportance += dbn.Value.PrbImportance * dbn.Key.Value / fTGate.Value;
            });
        }
        #endregion
    }
}
