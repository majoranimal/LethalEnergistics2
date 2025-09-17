namespace LethalEnergistics2.Classes
{
    internal class SystemStats
    {
        public int quantity;
        public float weight;
        public int value;
        public bool clientOnly;

        public SystemStats(int quantity = 0, float weight = 0, int value = 0, bool clientOnly = true)
        {
            this.quantity = quantity;
            this.weight = weight;
            this.value = value;
            this.clientOnly = clientOnly;
        }
    }
}
