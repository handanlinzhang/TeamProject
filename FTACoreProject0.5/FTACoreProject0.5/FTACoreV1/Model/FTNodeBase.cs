using System.Collections.Generic;
using System.Linq;
using System;
using FTACoreSL.Constant;
using FTACoreSL.Util;

namespace FTACoreSL.Model
{
    public class FTNodeBase
    {
        #region 修改代码记录块
        //@修改时间：2013-10-24
        //@修改内容：添加属性IsMidEvent，标记节点是否为中间事件节点
        #endregion

        //事件或门的唯一标记
        private string _key;
        public string Key
        {
            get { _key = _key ?? KeyCreator.NewKey(); return _key; }
            set { _key = value; }
        }

        //public bool IsS { get; set; }//是否补集
        public string Name { get; set; }
        public double? Value{  get;   set; }
        //结构重要度需要用到的，对于底事件来说，其值为0.5
        private double? _structValue;
        public double? StructValue
        {
            get
            {
                if (this.IsBasic) _structValue = 0.5;
                return _structValue;
            }
            set { _structValue = value; }
        }
        public string Note { get; set; }
        public string NodeType { get; set; }
        public string ChNodeType { get; set; }

        public bool HasChildren { get { return ChildNodes != null; } }
        //将参与计算的未探明事件纳入基本事件中
        public bool IsBasic { get { return NodeType == FTConstants.BasicEventName || (NodeType == FTConstants.UndevEventName && IsNeedCalculate); } }
        public bool IsNotGate { get { return NodeType == FTConstants.Gate_NOT; } }
        public bool IsCLone { get; set; }//是否克隆节点
        public bool IsHierarchy { get; set; } //是否分层

        public bool IsNeedCalculate { get; set; }//节点是否参与计算

        internal Dictionary<string, FTNodeBase> _childNodes = new Dictionary<string, FTNodeBase>();
        /// <summary>
        /// 返回所有子节点
        /// </summary>
        public Dictionary<string, FTNodeBase> ChildNodes { get { return _childNodes; } }

        private int _bIndex;
        /// <summary>
        /// 基本事件的排序属性；非基本事件时值为-1。
        /// </summary>
        public int BIndex
        {
            get { _bIndex = IsBasic || (NodeType == FTConstants.UndevEventName && IsNeedCalculate) ? _bIndex : -1; return _bIndex; }
            set { _bIndex = IsBasic || (NodeType == FTConstants.UndevEventName && IsNeedCalculate) ? value : -1; }
        }

        #region 新算法添加的属性

        #region
        /// <summary>
        /// //是否包含了重复事件 刘林林 0808
        /// </summary>
        public bool HasRepeatBasicNodes { get; set; }
        #endregion

        //@修改内容：添加一个字典，当节点的割集数目过多时，用来存储割集阶数和割集数量，而不具体存储其割集内容
        public Dictionary<int, int> CutSetOrderAndNumber
        {
            get { return _cutSetOrderAndNumber; }
            set { _cutSetOrderAndNumber = value; }
        }

        private Dictionary<int, int> _cutSetOrderAndNumber = new Dictionary<int, int>();

        Dictionary<FTNodeBase, BasicNodeImportance> _repeatingDescBasicNodes;
        /// <summary>
        /// 节点下所包含的重复的底事件节点
        /// </summary>
        public Dictionary<FTNodeBase, BasicNodeImportance> RepeatingDescendantBasicNodes
        {
            get
            {
                if (_repeatingDescBasicNodes != null) return _repeatingDescBasicNodes;
                if (IsBasic) return null;
                var repeatDescendants = DescendantsBasicNodes.Where(sd => sd.Value.Count > 1).ToList();
                _repeatingDescBasicNodes = repeatDescendants.Count() < 1 ? null : repeatDescendants.ToDictionary(d => d.Key, dv => dv.Value);
                //if (_repeatingDescBasicNodes)
                return _repeatingDescBasicNodes;
            }
        }

        List<List<NodeWithSymbol>> _highFunction;
        /// <summary>
        /// 节点的1结构函数
        /// </summary>
        public List<List<NodeWithSymbol>> HighFunction
        {
            get
            {
                _highFunction = _highFunction ?? new List<List<NodeWithSymbol>>();
                if (_highFunction.Count == 0 && (IsBasic || !HasRepeatBasicNodes))   //如果为底事件或者独立门节点
                {
                    _highFunction.Add(new List<NodeWithSymbol>() { new NodeWithSymbol() { Node = this, HOrL = true } });
                }

                return _highFunction;
            }
            set { _highFunction = value; }
        }

        List<List<NodeWithSymbol>> _lowFunction;
        /// <summary>
        /// 节点的0结构函数
        /// </summary>
        public List<List<NodeWithSymbol>> LowFunction
        {
            get
            {
                _lowFunction = _lowFunction ?? new List<List<NodeWithSymbol>>();
                if (_lowFunction.Count == 0 && (IsBasic || !HasRepeatBasicNodes))    //如果为底事件或者独立门节点
                {
                    _lowFunction.Add(new List<NodeWithSymbol>() { new NodeWithSymbol() { Node = this, HOrL = false } });
                }

                return _lowFunction;
            }
            set { _lowFunction = value; }
        }

        List<CutSet> _cutsets;
        /// <summary>
        /// 割集
        /// </summary>
        public List<CutSet> Cutsets
        {
            get
            {
                _cutsets = _cutsets ?? new List<CutSet>();
                if (_cutsets.Count == 0 && IsBasic) _cutsets.Add(new CutSet() { Cutset = new List<FTNodeBase>() { this } });
                return _cutsets;
            }
            set { _cutsets = value; }
        }

        List<CutSet> _keyCutsets;
        /// <summary>
        /// 关键割集
        /// </summary>
        public List<CutSet> KeyCutsets
        {
            get { return _keyCutsets; }
            set { _keyCutsets = value; }
        }

        Dictionary<int, ulong> _cutsetsRank;
        /// <summary>
        /// 割集阶数及对应数量
        /// </summary>
        public Dictionary<int, ulong> CutsetsRank
        {
            get
            {
                if (_cutsetsRank != null && _cutsetsRank.Count > 0) return _cutsetsRank;
                if (this.Cutsets.Count > 0)
                {
                    _cutsetsRank = new Dictionary<int, ulong>();
                    var ranks = (from cs in this.Cutsets select cs.Cutset.Count).Distinct().ToList();
                    ranks.ForEach(rank => _cutsetsRank.Add(rank, (ulong)this.Cutsets.Where(cs => cs.Cutset.Count == rank).Count()));
                }
                return _cutsetsRank;
            }
            set { _cutsetsRank = value; }
        }

        Dictionary<FTNodeBase, BasicNodeImportance> _descendantsBasicNodes;
        /// <summary>
        /// 获取门节点下的所有底事件节点的重要度
        /// </summary>
        public Dictionary<FTNodeBase, BasicNodeImportance> DescendantsBasicNodes
        {
            get
            {
                if (_descendantsBasicNodes == null) _descendantsBasicNodes = new Dictionary<FTNodeBase, BasicNodeImportance>();
                if (_descendantsBasicNodes.Count == 0)
                {
                    if (this.IsBasic)
                        _descendantsBasicNodes.Add(this, new BasicNodeImportance()
                        {
                            Count = 1,
                            PrbImportance = 1,
                            StrImportance = 1,
                            RiskReductionWorth = 0,
                            RiskAchievementWorth = 1
                        });
                    else GetAllDescendantsBasicNodes(this);
                }
                return _descendantsBasicNodes;
            }
        }

        /// 递归获取所有门事件下的所有子孙底事件。
        private void GetAllDescendantsBasicNodes(FTNodeBase fTGate)
        {
            if (fTGate.ChildBasicNodes.Count > 0)
            {
                fTGate.ChildBasicNodes.Values.ToList().ForEach(cbn =>
                {
                    FTNodeBase repeatNode = _descendantsBasicNodes.Keys.Where(c => c.Key == cbn.Key).FirstOrDefault();
                    if (repeatNode == null) _descendantsBasicNodes.Add(cbn, new BasicNodeImportance()
                    {
                        Count = 1,
                        PrbImportance = 0,
                        StrImportance = 0,
                        RiskReductionWorth = 0,
                        RiskAchievementWorth = 0
                    });
                    else _descendantsBasicNodes[repeatNode].Count++;
                });
            }
            if (fTGate.ChildGateNodes.Count > 0) fTGate.ChildGateNodes.Values.ToList().ForEach(cgn => GetAllDescendantsBasicNodes(cgn));
        }

        Dictionary<string, FTNodeGate> _childGateNodes;
        /// <summary>
        /// 返回子节点中的门节点 
        /// </summary>
        /// 
        public Dictionary<string, FTNodeGate> ChildGateNodes
        {
            get
            {
                if (_childGateNodes == null) _childGateNodes = new Dictionary<string, FTNodeGate>();
                //@修改原因：不参与计算的未探明事件不能是虚拟门节点
                //@修改内容：c.Value.NodeType != FTConstants.UndevEventName 作为一个与条件
                //-------------------------------
                //@修改原因：子节点中的门节点状况发生改变，子门节点也应相应发生改变
                //@修改内容：增加了ChildNodes与ChildGateNodes的同步操作
                var r = _childNodes.Where(c => !c.Value.IsBasic && c.Value.NodeType != FTConstants.UndevEventName).ToList();
                //if (r.Count == 0) 
                _childGateNodes.Clear();
                r.ForEach(ri => { if (!_childGateNodes.ContainsKey(ri.Key))_childGateNodes.Add(ri.Key, ri.Value as FTNodeGate); });
                return _childGateNodes;
            }
        }

        Dictionary<string, FTNodeBase> _childBasicNodes;
        /// <summary>
        /// 返回子节点中的基本事件节点
        /// </summary>
        public Dictionary<string, FTNodeBase> ChildBasicNodes
        {
            get
            {
                if (_childBasicNodes == null) _childBasicNodes = new Dictionary<string, FTNodeBase>();
                var r = _childNodes.Where(c => c.Value.IsBasic).ToList();
                if (r.Count == 0) _childBasicNodes.Clear();
                r.ForEach(ri => { if (!_childBasicNodes.ContainsKey(ri.Key)) _childBasicNodes.Add(ri.Key, ri.Value); });
                return _childBasicNodes;
            }
        }
        #endregion
    }


}