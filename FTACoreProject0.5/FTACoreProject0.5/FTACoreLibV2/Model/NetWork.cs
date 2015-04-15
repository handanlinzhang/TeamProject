using System;
using System.Xml;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Collections.Generic;

namespace FTACoreV2.Model
{
    class NetWork
    {
        public NodeBase TopNode;
        public Dictionary<string, NodeBase> BasicNodesDic = new Dictionary<string, NodeBase>();
        public Dictionary<string, NodeBase> AllNodesDic = new Dictionary<string, NodeBase>();

        public NodeBase FromXElement(XElement xElement)
        {
            LoadXElement(xElement);
            return (from nd in AllNodesDic.Values where nd.OutDegree == -1 select nd).FirstOrDefault();
        }

        private void LoadXElement(XElement xElement)
        {
            foreach (var sXEle in xElement.Elements())
            {
                var sNode = GenerateNewNode(sXEle);
                if (sXEle.HasElements)
                {
                    foreach (var ssXEle in sXEle.Elements())
                    {
                        var ssNode = GenerateNewNode(ssXEle);
                        sNode.Parents.Add(ssNode.ID, ssNode);
                        sNode.InDegree++;
                        ssNode.Children.Add(sNode.ID, sNode);
                        ssNode.OutDegree++;
                    }
                }
            }
        }

        private NodeBase GenerateNewNode(XElement xElement)
        {
            var id = xElement.Attribute("i").Value;
            NodeBase snd = AllNodesDic.ContainsKey(id) ? AllNodesDic[id] : new NodeBase() { ID = id };
            if (xElement.Attribute("t") != null) snd.NodeType = xElement.Attribute("t").Value;
            if (!AllNodesDic.ContainsKey(id)) AllNodesDic.Add(snd.ID, snd);
            if (xElement.Attribute("t").Value == "BSC")
            {
                snd.Value = double.Parse(xElement.Attribute("v").Value);
                BasicNodesDic.Add(snd.ID, snd);
            }
            return snd;
        }
    }
}
