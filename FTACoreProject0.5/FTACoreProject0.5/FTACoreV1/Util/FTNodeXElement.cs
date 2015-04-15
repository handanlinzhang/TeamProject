using System.Collections.Generic;
using System.Xml.Linq;
using FTACoreSL.Constant;
using FTACoreSL.Model;

namespace FTACoreSL.Util
{
    public class FTNodeXElement
    {
        public static XElement ToXElement(FTNodeBase node)
        {
            return RecursionOutXElement(node);
        }

        static Dictionary<string, FTNodeBase> NodesDic = new Dictionary<string, FTNodeBase>();

        private static XElement RecursionOutXElement(FTNodeBase node)
        {
            XElement outXElement = null;
            if (node.IsHierarchy)
            {
                outXElement = new XElement(
                   node.NodeType,
                   new XAttribute(FTConstants.KeyAttributeName, node.Key),
                   new XAttribute(FTConstants.NameAttributeName, node.Name),
                   new XAttribute(FTConstants.NoteAttributeName, node.Note),
                   new XAttribute(FTConstants.ValueAttributeName, node.Value),
                   new XAttribute(FTConstants.IsHierarchy, node.IsHierarchy)
                   );
            }
            else
            {
                outXElement = new XElement(
                  node.NodeType,
                  new XAttribute(FTConstants.KeyAttributeName, node.Key),
                  new XAttribute(FTConstants.NameAttributeName, node.Name),
                  new XAttribute(FTConstants.NoteAttributeName, node.Note),
                  new XAttribute(FTConstants.ValueAttributeName, node.Value)
                  );
            }
            var basicNode = node;
            if (basicNode != null)
            {
                outXElement.Add(new XAttribute(
                    FTConstants.ProbabilityAttributeName,
                    basicNode.Value
                    ));
            }

            if (node.ChildNodes != null)
                foreach (var cnd in node.ChildNodes)
                    outXElement.Add(RecursionOutXElement(cnd.Value as FTNodeBase));

            return outXElement;
        }

        public static XElement RecursionOutXElement1(FTNodeBase node)
        {
            XElement outXElement = new XElement(node.NodeType, new XAttribute(FTConstants.KeyAttributeName, node.Key), new XAttribute("BIndex", node.BIndex));

            if (node.ChildNodes != null)
                foreach (var cnd in node.ChildNodes)
                    outXElement.Add(RecursionOutXElement1(cnd.Value as FTNodeBase));

            return outXElement;
        }

        public static FTNodeBase FromXElement(XElement xElement)
        {
            var loadFTNode = RecursionLoadXElement(xElement, null);
            return loadFTNode;
        }

        private static FTNodeBase RecursionLoadXElement(XElement xElement, FTNodeBase fatherNode)
        {
            FTNodeBase loadNode;
            if (xElement.Name.LocalName != "BASIC")
            {
                loadNode = new FTNodeGate()
                {
                    Key = xElement.FirstAttribute.Value,
                    NodeType = xElement.Name.LocalName,
                    KofN = (xElement.Name.LocalName == FTConstants.Gate_VOTE) ? (int.Parse(xElement.Attribute("KofN").Value)) : 0
                };
                if (fatherNode != null) fatherNode.ChildNodes.Add(loadNode.Key, loadNode);
            }
            else
            {
                if (!NodesDic.ContainsKey(xElement.FirstAttribute.Value))
                {
                    loadNode = new FTNodeBase()
                    {
                        Key = xElement.FirstAttribute.Value,
                        NodeType = xElement.Name.LocalName,
                        BIndex = int.Parse(xElement.FirstAttribute.Value),
                        Value = double.Parse(xElement.LastAttribute.Value) > 1 ? 0.2 : double.Parse(xElement.LastAttribute.Value)
                    };
                    fatherNode.ChildNodes.Add(loadNode.Key, loadNode);
                    NodesDic.Add(loadNode.Key, loadNode);
                }
                else
                {
                    loadNode = NodesDic[xElement.FirstAttribute.Value];
                    fatherNode.ChildNodes.Add(loadNode.Key, loadNode);
                }
            }

            if (!loadNode.IsBasic)
            {
                if (xElement.Elements() != null)
                    foreach (var subElement in xElement.Elements())
                        RecursionLoadXElement(subElement, loadNode);
            }

            return loadNode;
        }

        public static FTNodeBase FromXElementFromRAWXML(XElement xElement)
        {
            var loadFTNode = RecursionLoadRAWXElement(xElement, null);
            return loadFTNode;
        }

        private static FTNodeBase RecursionLoadRAWXElement(XElement xElement, FTNodeBase fatherNode)
        {
            FTNodeBase loadNode;

            var isExistNode = NodesDic.ContainsKey(xElement.FirstAttribute.Value);
            if (xElement.Name.LocalName != FTConstants.BasicEventName)
            {
                loadNode = isExistNode ? NodesDic[xElement.FirstAttribute.Value] :
                                         new FTNodeGate()
                                                {
                                                    Key = xElement.FirstAttribute.Value,
                                                    NodeType = xElement.Name.LocalName,
                                                    KofN = (xElement.Name.LocalName == FTConstants.Gate_VOTE) ? (int.Parse(xElement.Attribute(FTConstants.KofAttributeName).Value)) : 0
                                                };
            }
            else
            {
                loadNode = isExistNode ? NodesDic[xElement.FirstAttribute.Value] :
                                        new FTNodeBase()
                                            {
                                                Key = xElement.FirstAttribute.Value,
                                                NodeType = xElement.Name.LocalName,
                                                BIndex = int.Parse(xElement.FirstAttribute.Value),
                                                Value = double.Parse(xElement.LastAttribute.Value) > 1 ? 0.2 : double.Parse(xElement.LastAttribute.Value)
                                            };
            }
            if (!isExistNode) NodesDic.Add(loadNode.Key, loadNode);
            fatherNode.ChildNodes.Add(loadNode.Key, loadNode);

            if (!loadNode.IsBasic)
            {
                if (xElement.Elements() != null)
                    foreach (var subElement in xElement.Elements())
                        RecursionLoadXElement(subElement, loadNode);
            }

            return loadNode;
        }
    }
}
