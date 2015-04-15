using System;
using System.Linq;
using System.Text;
using FTACoreV2.Model;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FTACoreLibV2.Algorithm
{
    /// <summary>
    /// 模块化故障树，线性时间算法
    /// </summary>
    class FTModularizer
    {
        public Dictionary<string, NodeBase> modules = new Dictionary<string, NodeBase>();

        Dictionary<string, NodeBase> nodesHasUncloseRepeat;
        NodeBase topNode;
        public void GoModularize(NetWork fTNet)
        {
            //SetModule(fTNet);
        }

        private void SetDependency(NetWork fTNet)
        {
            Dictionary<string, NodeBase> repeatNodes = fTNet.AllNodesDic.Where(nd => nd.Value.OutDegree > 1).ToDictionary(x => x.Key, x => x.Value);
            repeatNodes.Values.ToList().ForEach(rnd => RecursiveSetDependency(rnd));
        }

        private void RecursiveSetDependency(NodeBase node)
        {
            if (node.OutDegree == 0) { topNode = node; topNode.IsModule = true; modules.Add(topNode.ID, topNode); return; }
            if (node.OutDegree > 1 && IsRepeatsClose(node)) { node.IsModule = true; modules.Add(node.ID, node); }
            
            node.Children.Values.ToList().ForEach(cnd =>
            {
                if (cnd.RepeatNodeWithInDegree.ContainsKey(node)) cnd.RepeatNodeWithInDegree[node]++;
                else { nodesHasUncloseRepeat.Add(cnd.ID, cnd); cnd.RepeatNodeWithInDegree.Add(node, 1); }

                if (!IsRepeatsClose(cnd)) RecursiveSetDependency(cnd);
                else 
                {
                    cnd.IsModule = true; 
                    modules.Add(cnd.ID, cnd);
                }
            });
        }

        private void SetModule(NetWork fTNet)
        {
            foreach (var nd in nodesHasUncloseRepeat.Values) SetModule(nd);
        }

        void SetModule(NodeBase node)
        {
            if (IsRepeatsClose(node))
            {
                node.IsModule = true;
                modules.Add(node.ID, node);
            }

            var independentParents = node.Parents.Values.Where(p => p.RepeatNodeWithInDegree.Count == 0);
            if (independentParents.Count() > 1)
                GenerateVirtualNode(independentParents.ToList(), node);
            else if (independentParents.Count() == 1)
            {
                var singleNode = independentParents.FirstOrDefault();
                if (singleNode.InDegree == 0) return; //底事件节点滚粗
                singleNode.IsModule = true;
                modules.Add(singleNode.ID, singleNode);
            }
        }

        bool IsRepeatsClose(NodeBase node)
        {
            foreach (var rnd in node.RepeatNodeWithInDegree)
                if (rnd.Value != rnd.Key.OutDegree) return false;

            return true;
        }

        void GenerateVirtualNode(List<NodeBase> nodesList, NodeBase node)
        {
            NodeBase virtualNode = new NodeBase() { IsVirtual = true, NodeType = node.NodeType, OutDegree = 1, IsModule = true };

            virtualNode.Children.Add(node.ID, node);
            nodesList.ForEach(nd =>
            {
                virtualNode.ID += nd.ID + "_";
                virtualNode.Parents.Add(nd.ID, nd);
                node.Parents.Remove(nd.ID);
            });
            modules.Add(virtualNode.ID, virtualNode);
        }
    }
}
