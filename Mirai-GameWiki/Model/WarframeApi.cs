using System;
using System.Collections.Generic;
using System.Text;

namespace Mirai_GameWiki.Model
{
    #region 突击
    public class SortieModel
    {
        public string id { get; set; }
        public string activation { get; set; }
        public string startString { get; set; }
        public string expiry { get; set; }
        public bool active { get; set; }
        public string rewardPool { get; set; }
        public List<Sortie_Variants> variants { get; set; }
        public string boss { get; set; }
        public string faction { get; set; }
        public bool expired { get; set; }
        public string eta { get; set; }
    }
    public class Sortie_Variants
    {
        public string boss { get; set; }
        /// <summary>
        /// 星球(是不是和node搞反了)
        /// </summary>
        public string planet { get; set; }
        /// <summary>
        /// 星球节点
        /// </summary>
        public string node { get; set; }
        /// <summary>
        /// 任务类型
        /// </summary>
        public string missionType { get; set; }
        /// <summary>
        /// 任务特性
        /// </summary>
        public string modifier { get; set; }
        /// <summary>
        /// 特性描述
        /// </summary>
        public string modifierDescription { get; set; }
    }
    #endregion
}
