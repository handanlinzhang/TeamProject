using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace FTACoreV2.Model
{
    class NodeBase
    {
        public string ID;
        public double Value = 0;
        public int OutDegree = 0;
        public int InDegree = 0;
        //public int OutDegrees = 0;
        //public int InDegrees = 0;
        public bool IsVirtual;
        //public bool IsIndependent = true;
        public bool IsModule;
        public string NodeType = string.Empty;
        public Dictionary<string, NodeBase> Parents = new Dictionary<string, NodeBase>();
        public Dictionary<string, NodeBase> Children = new Dictionary<string, NodeBase>();
        public Dictionary<string, NodeBase> RepeatNodes = new Dictionary<string, NodeBase>();
        Dictionary<NodeBase, int> _repeatNodesWithInDegree;
        public Dictionary<NodeBase, int> RepeatNodeWithInDegree
        {
            get { return _repeatNodesWithInDegree ?? new Dictionary<NodeBase, int>(); }
        }
    }
}
