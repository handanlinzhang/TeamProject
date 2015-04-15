using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTACoreLibV2.Algorithm
{
    /// <summary>
    /// 简化类：简化逻辑冗余，吸收子树，简化其他...
    /// </summary>
    public class FTSimplifier
    {
        public static void GoSimplify(FTNodeGate fng)
        {
            RemoveLogicRedundance(fng);
            AbsorbSubTree(fng);
        }

        /// <summary>
        /// 简化逻辑冗余
        /// </summary>
        /// <param name="fng"></param>
        private static void RemoveLogicRedundance(FTNodeGate fng)
        {
            if (fng == null || fng.ChildGateNodes.Count == 0) return;

            //找与父节点同类型的子节点
            var childSameGates = fng.ChildGateNodes.Where(cgn => cgn.Value.NodeType == fng.NodeType);
            if (childSameGates.Count() > 0)
            {    //将相同类型的子节点挂到父节点中
                childSameGates.ToList().ForEach(csg =>
                {
                    fng.ChildNodes.Remove(csg.Key);
                    csg.Value.ChildNodes.ToList().ForEach(csgcnd =>
                    {
                        if (!fng.ChildNodes.ContainsKey(csgcnd.Key))
                            fng.ChildNodes.Add(csgcnd.Key, csgcnd.Value);
                    });
                    csg.Value.ChildNodes.Clear();
                });
                RemoveLogicRedundance(fng);
            }
            else
            {
                //不同类型的子门节点接着往下递归
                var childNotSameGates = fng.ChildGateNodes.Where(cgn => cgn.Value.NodeType != fng.NodeType);
                if (childNotSameGates.Count() > 0)
                    childNotSameGates.ToList().ForEach(cnsg => RemoveLogicRedundance(cnsg.Value));
            }
        }
        /// <summary>
        /// 吸收子树
        /// </summary>
        /// <param name="fng">待吸收的故障树结构</param>
        private static void AbsorbSubTree(FTNodeGate fng)
        {
            if (fng == null || fng.ChildGateNodes.Count == 0) return;
            //找到能够被吸收的子门节点，然后移除掉
            var absorbedChildGates = fng.ChildGateNodes.Values.ToList().Where(cnv => cnv.ChildNodes.Keys.Intersect(fng.ChildNodes.Keys).Count() > 0);
            //@修改时间：2014/5/6
            //@修改人：李志峰
            //因为ChildGateNodes的值依赖于ChildNodes,因此吸收的门节点应直接从ChildNodes中去除
            if (absorbedChildGates.Count() > 0)
                //absorbedChildGates.ToList().ForEach(abcg => test = fng.ChildGateNodes.Remove(abcg.Key));
                absorbedChildGates.ToList().ForEach(abcg => fng.ChildNodes.Remove(abcg.Key));

            //如果还有子门节点，接着往下递归
            if (fng.ChildGateNodes.Count > 0) fng.ChildGateNodes.Values.ToList().ForEach(cgv => AbsorbSubTree(cgv));
        }
    }
}
