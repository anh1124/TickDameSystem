using TickCombat.Combat;

namespace TickCombat.Passive
{
    public class KhoMocPhungXuan : PassiveSkill, IDamageModifier
    {
        public override int Priority => 10;

        public void Modify(DamageContext ctx)
        {
            ctx.shieldGenerated += ctx.finalDamage * 0.1f;
        }
    }
}
