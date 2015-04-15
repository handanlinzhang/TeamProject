using System;
using System.Linq;
using System.Collections.Generic;
using FTACoreSL.Model;

namespace FTACoreSL.Algorithm
{
    public class CalculatorEntrance
    {
        public static void GoCalculate(FTNodeGate fTGate)
        {
            FaultTreeSimplifier.GoSimplify(fTGate);
            PreHandler.PreHandleFaultTree(fTGate);
            ProbabilityCalculator.CalculateProbability(fTGate);
            CutSetsCalculator.CalculateCutSets(fTGate);
            ImportanceCalculator.CalculateImportance(fTGate);
        }

        public static void SimplifyFaultTree(FTNodeGate fTGate)
        {
            FaultTreeSimplifier.GoSimplify(fTGate);
        }

        public static void PreHandleFaultTree(FTNodeGate fTGate)
        {
            PreHandler.PreHandleFaultTree(fTGate);
        }
        public static void CalculateProbability(FTNodeGate fTGate)
        {
            ProbabilityCalculator.CalculateProbability(fTGate);
        }
        public static void CalculateCutSets(FTNodeGate fTGate)
        {
            CutSetsCalculator.CalculateCutSets(fTGate);
        }
        public static void CalculateImportance(FTNodeGate fTGate)
        {
            ImportanceCalculator.CalculateImportance(fTGate);
        }
    }
}