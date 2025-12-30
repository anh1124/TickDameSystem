using TickCombat.Entity;

namespace TickCombat.Combat
{
    public class DamageContext
    {
        public Status target;

        public float snapshotHP;

        public float rawDamage;
        public float rawHeal;

        public float finalDamage;
        public float finalHeal;

        public float shieldGenerated;
    }
}
