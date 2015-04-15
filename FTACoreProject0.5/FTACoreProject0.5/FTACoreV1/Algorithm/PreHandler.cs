using System;
using System.Linq;
using System.Collections.Generic;
using FTACoreSL.Model;
using FTACoreSL.Constant;
using FTACoreSL.Util;

namespace FTACoreSL.Algorithm
{
    public class PreHandler
    {
        #region 对故障树进行预处理
        private static Dictionary<FTNodeBase, BasicNodeImportance> _sameBasicNodes;
        //@修改于2013-8-27；修改_sameBasicNodes的结构为Dictionary<string, BasicNodeImportance>
        //private static Dictionary<string, BasicNodeImportance> _sameBasicNodes;
        private static Stack<FTNodeGate> _voteGateStack = new Stack<FTNodeGate>();

        public static void PreHandleFaultTree(FTNodeGate fTGate)
        {
            if (_voteGateStack == null) _voteGateStack = new Stack<FTNodeGate>();
            RecursiveSeekVOTEGate(fTGate);
            HandleVOTEGateStack();
            //if (fTGate.RepeatingDescendantBasicNodes != null)
            //{
            //    _sameBasicNodes = new Dictionary<FTNodeBase, BasicNodeImportance>();
            //    //_sameBasicNodes = new Dictionary<string, BasicNodeImportance>();
            //    fTGate.RepeatingDescendantBasicNodes.ToList().ForEach(c => _sameBasicNodes.Add(c.Key, c.Value));
            //} 

            _sameBasicNodes = fTGate.RepeatingDescendantBasicNodes;
            if (_sameBasicNodes != null) RecursivePreHandleFaultTree(fTGate);
        }

        private static void RecursiveSeekVOTEGate(FTNodeGate fTGate)
        {
            if (fTGate.NodeType == FTConstants.Gate_VOTE) _voteGateStack.Push(fTGate);
            fTGate.ChildGateNodes.ToList().ForEach(cgn => RecursiveSeekVOTEGate(cgn.Value));
        }

        private static void HandleVOTEGateStack()
        {
            while (_voteGateStack.Count > 0) HandleVOTEGate(_voteGateStack.Pop());
            _voteGateStack = null;
        }

        private static void HandleVOTEGate(FTNodeGate fTGate)
        {
            var combinationList = PermutationAndCombination<FTNodeBase>.GetCombination(fTGate.ChildNodes.Values.ToArray(), fTGate.KofN);
            fTGate.NodeType = FTConstants.Gate_OR;
            fTGate.ChNodeType = FTConstants.Gate_OR_CN;
            fTGate.KofN = 0;
            fTGate.ChildNodes.Clear();
            combinationList.ForEach(cmb =>
                {
                    var newANDGate = new FTNodeGate() { NodeType = FTConstants.Gate_AND,ChNodeType = FTConstants.Gate_AND_CN };
                    cmb.ToList().ForEach(c => newANDGate.ChildNodes.Add(c.Key, c));
                    fTGate.ChildNodes.Add(newANDGate.Key, newANDGate);
                });
        }
        private static int CompareFaultNode(FTNodeBase a, FTNodeBase b)
        {
            if (a.BIndex != b.BIndex)
                return 0;
            if (a.ChildBasicNodes != b.ChildBasicNodes)
                return 1;
            if (a.ChildGateNodes != b.ChildGateNodes)
                return 2;
            if (a.ChildNodes != b.ChildNodes)
                return 3;
            if (a.Cutsets != b.Cutsets)
                return 4;
            if (a.CutsetsRank != b.CutsetsRank)
                return 5;
            if (a.DescendantsBasicNodes != b.DescendantsBasicNodes)
                return 6;
            if (a.HasChildren != b.HasChildren)
                return 7;
            if (a.HasRepeatBasicNodes != b.HasRepeatBasicNodes)
                return 8;
            if (a.HighFunction != b.HighFunction)
                return 9;
            if (a.IsBasic != b.IsBasic)
                return 10;
            if (a.IsHierarchy != b.IsHierarchy)
                return 11;
            if (a.IsNeedCalculate != b.IsNeedCalculate)
                return 12;
            if (a.IsNotGate != b.IsNotGate)
                return 13;
            if (a.Key != b.Key)
                return 15;
            if (a.KeyCutsets != b.KeyCutsets)
                return 16;
            if (a.LowFunction != b.LowFunction)
                return 18;
            if (a.Name != b.Name)
                return 19;
            if (a.NodeType != b.NodeType)
                return 20;
            if (a.Note != b.Note)
                return 21;
            if (a.RepeatingDescendantBasicNodes != b.RepeatingDescendantBasicNodes)
                return 22;
            if (a.StructValue != b.StructValue)
                return 23;
            if (a.Value != b.Value)
                return 24;

            return 100;

        }
        private static void RecursivePreHandleFaultTree(FTNodeGate fTGate)
        {
            if (fTGate.DescendantsBasicNodes.Count == 0) return;
            //如果当前门节点的子孙节点中不包含重复的节点，则返回跳出递归
            //int temp = 1000;
            //FTNodeBase temp1 = _sameBasicNodes.Keys.FirstOrDefault(); 
            //fTGate.DescendantsBasicNodes.Keys.ToList().ForEach(k => temp = CompareFaultNode(k, temp1));
            int count = fTGate.DescendantsBasicNodes.Keys.Where(k => _sameBasicNodes.ContainsKey(k)).Count();
            if (count == 0) return;
            fTGate.HasRepeatBasicNodes = true;

            //获取该门孩子节点中包含的重复底事件,如果个数大于1，则创建一个虚门节点
            var cbnvs = fTGate.ChildBasicNodes.Values.Where(cbn => _sameBasicNodes.ContainsKey(cbn)).ToList();
            if (cbnvs.Count > 1) cbnvs.ForEach(cbnv => cbnv.HasRepeatBasicNodes = true);
            if (cbnvs.Count > 1 && cbnvs.Count < fTGate.ChildNodes.Count) GenerateVirtualGate(fTGate, cbnvs, true);

            //获取该门孩子节点中包含的非重复底事件，如果个数大于1，则创建一个虚门节点
            cbnvs = fTGate.ChildBasicNodes.Values.Where(cn => !_sameBasicNodes.ContainsKey(cn)).ToList();
            if (cbnvs.Count > 1 && cbnvs.Count < fTGate.ChildNodes.Count) GenerateVirtualGate(fTGate, cbnvs, false);

            //获取该门孩子节点中的门事件
            var cgnvs = fTGate.ChildGateNodes.Values.ToList();
            if (cgnvs.Count > 0) cgnvs.ForEach(cgnv => RecursivePreHandleFaultTree(cgnv));
        }

        private static void GenerateVirtualGate(FTNodeGate fTGate, List<FTNodeBase> childNodes, bool hasRepeatBasicNodes)
        {
            if (childNodes.Count == 0 || fTGate.IsVirtual) return;
            if (childNodes.Count == 1) { childNodes.FirstOrDefault().HasRepeatBasicNodes = hasRepeatBasicNodes; return; }
            FTNodeGate newGate = new FTNodeGate()
            {
                //FatherNode = fTGate,
                NodeType = fTGate.NodeType,
                IsVirtual = true,
                HasRepeatBasicNodes = hasRepeatBasicNodes
            };
            fTGate.ChildNodes.Add(KeyCreator.NewKey(), newGate);

            childNodes.ForEach(cbnv =>
            {
                fTGate.ChildNodes.Remove(cbnv.Key);
                fTGate.ChildBasicNodes.Remove(cbnv.Key);
                newGate.ChildNodes.Add(cbnv.Key, cbnv);
            });
        }
        #endregion
    }
}