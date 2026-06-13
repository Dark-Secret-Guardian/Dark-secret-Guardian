namespace Aesheria.Core
{
    /// <summary>
    /// 玩家行动数据类
    /// 记录一个行动对四维数值的影响（delta 值）
    /// 可用于快捷键行动、任务选项中的数值变化等
    /// </summary>
    [System.Serializable]
    public class PlayerAction
    {
        public string actionName;         // 行动名称
        public int deltaProsperity;      // 繁荣度变化量
        public int deltaEcology;          // 生态值变化量
        public int deltaAwakening;       // 觉醒值变化量
        public int deltaGuardianship;    // 守护值变化量

        /// <summary>
        /// 构造函数：创建一个行动
        /// </summary>
        /// <param name="name">行动名称</param>
        /// <param name="p">繁荣度变化</param>
        /// <param name="e">生态值变化</param>
        /// <param name="a">觉醒值变化</param>
        /// <param name="g">守护值变化</param>
        public PlayerAction(string name, int p, int e, int a, int g)
        {
            actionName = name;
            deltaProsperity = p;
            deltaEcology = e;
            deltaAwakening = a;
            deltaGuardianship = g;
        }
    }
}
