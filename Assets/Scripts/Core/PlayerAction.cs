namespace Aesheria.Core
{
    [System.Serializable]
    public class PlayerAction
    {
        public string actionName;
        public int deltaProsperity;
        public int deltaEcology;
        public int deltaAwakening;
        public int deltaGuardianship;

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
