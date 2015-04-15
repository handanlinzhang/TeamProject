using System.Linq;
using System.Collections.Generic;

namespace FTACoreSL.Model
{
    public class BasicNodeImportance
    {
        public int Count;
        public double? PrbImportance;
        public double? StrImportance;
        public double? CriImportance;
        public double? RelCSImportance;
        /// <summary>
        ///  RiskReductionWorth的值为该基本事件不发生时，其上门事件发生的概率
        /// </summary>
        public double? RiskReductionWorth;
        /// <summary>
        ///RiskAchievementWorth的值为：该基本事件发生时，其上门事件发生的概率
        /// </summary>
        public double? RiskAchievementWorth;
        public double? FV;
        public double? BM;
    }
}
