using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using FTACoreV2.Model;

namespace FTACoreLibV2.Algorithm
{
    public class CalculatorEntrance
    {
        public static void GoCalculate(NetWork fTNet)
        {
            FaultTreeSimplifier.GoSimplify(fTGate);
            FTStandandizer.PreHandleFaultTree(fTGate);
            ProbabilityCalculator.CalculateProbability(fTGate);
            CutSetsCalculator.CalculateCutSets(fTGate);
            ImportanceCalculator.CalculateImportance(fTGate);
        }

        public static void SimplifyFaultTree(NetWork fTNet)
        {
            //FaultTreeSimplifier.GoSimplify(fTGate);
        }

        public static void PreHandleFaultTree(NetWork fTNet)
        {
            //PreHandler.PreHandleFaultTree(fTGate);
        }

        public static void ModularizeFaultTree(NetWork fTNet)
        { 
            
        }

        public static void CalculateProbability(NetWork fTNet)
        {
            //ProbabilityCalculator.CalculateProbability(fTGate);
        }
        public static void CalculateCutSets(NetWork fTNet)
        {
            //CutSetsCalculator.CalculateCutSets(fTGate);
        }
        public static void CalculateImportance(NetWork fTNet)
        {
            //ImportanceCalculator.CalculateImportance(fTGate);
        }
    }
}
