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
        ///  RiskReductionWorth��ֵΪ�û����¼�������ʱ���������¼������ĸ���
        /// </summary>
        public double? RiskReductionWorth;
        /// <summary>
        ///RiskAchievementWorth��ֵΪ���û����¼�����ʱ���������¼������ĸ���
        /// </summary>
        public double? RiskAchievementWorth;
        public double? FV;
        public double? BM;
    }
}
