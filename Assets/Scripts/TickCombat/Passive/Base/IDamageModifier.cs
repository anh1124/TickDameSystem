using TickCombat.Combat;

namespace TickCombat.Passive
{
    public interface IDamageModifier
    {
        void Modify(DamageContext ctx);
    }
}
