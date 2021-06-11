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
    #region 奸商
    public class VoidTraderModel
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public string startString { get; set; }
        public DateTime expiry { get; set; }
        public string active { get; set; }
        public string character { get; set; }
        /// <summary>
        /// 地点
        /// </summary>
        public string location { get; set; }
        public string psId { get; set; }
        /// <summary>
        /// 剩余时间
        /// </summary>
        public string endString { get; set; }
    }
    #endregion
    #region 希图斯昼夜
    public class CetusCycleModel
    {
        public string id { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime expiry { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime activation { get; set; }
        /// <summary>
        /// 是否白天 true=是
        /// </summary>
        public bool isDay { get; set; }
        /// <summary>
        /// 当前状态 day、night
        /// </summary>
        public string state { get; set; }
        /// <summary>
        /// 剩余时间
        /// </summary>
        public string timeLeft { get; set; }
        public bool isCetus { get; set; }
        /// <summary>
        /// 转换剩余时间
        /// </summary>
        public string shortString { get; set; }
    }
    #endregion
    #region 入侵
    public class InvasionsModel
    {
        public string id { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime activation { get; set; }
        public string startString { get; set; }
        /// <summary>
        /// 节点
        /// </summary>
        public string node { get; set; }
        public string nodeKey { get; set; }
        public string desc { get; set; }
        /// <summary>
        /// 进攻方奖励
        /// </summary>
        public Reward attackerReward { get; set; }
        public Faction attacker { get; set; }
        public string attackingFaction { get; set; }
        /// <summary>
        /// 防守方奖励
        /// </summary>
        public Reward defenderReward { get; set; }
        public Faction defender { get; set; }
        public string defendingFaction { get; set; }

        public bool vsInfestation { get; set; }
        public int count { get; set; }
        public int requiredRuns { get; set; }
        /// <summary>
        /// 进度
        /// </summary>
        public double completion { get; set; }
        public bool completed { get; set; }
        /// <summary>
        /// 剩余时间
        /// </summary>
        public string eta { get; set; }
        public List<string> rewardTypes { get; set; }
    }
    public class Reward
    {
        public List<string> items { get; set; }
        public List<CountedItem> countedItems { get; set; }
        public int credits { get; set; }
        public string asString { get; set; }
        public string itemString { get; set; }
        public string thumbnail { get; set; }
        public int color { get; set; }
    }
    public class Faction
    {
        public Reward reward { get; set; }
        public string faction { get; set; }
        public string factionKey { get; set; }
    }
    public class CountedItem
    {
        /// <summary>
        /// 数量
        /// </summary>
        public int count { get; set; }
        /// <summary>
        /// 奖励类型
        /// </summary>
        public string type { get; set; }
        public string key { get; set; }
    }
    #endregion
    #region 午夜电波
    public class NightwaveModel
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public string startString { get; set; }
        public DateTime expiry { get; set; }
        public bool active { get; set; }
        public int season { get; set; }
        public string tag { get; set; }
        public int phase { get; set; }
        //public object params{ get; set; }
        //public List<NightwaveChalleng> possibleChallenges { get; set; }
        public List<NightwaveChalleng> activeChallenges { get; set; }
        public List<string> rewardTypes { get; set; }
    }
    public class NightwaveChalleng
    {
        public string id { get; set; }
        public DateTime activation { get; set; }
        public string startString { get; set; }
        public DateTime expiry { get; set; }
        public bool active { get; set; }
        public bool isDaily { get; set; }
        public bool isElite { get; set; }
        public string desc { get; set; }
        public string title { get; set; }
        public int reputation { get; set; }
    }
    #endregion
}
