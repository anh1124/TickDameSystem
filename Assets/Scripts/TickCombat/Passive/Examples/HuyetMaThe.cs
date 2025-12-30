using UnityEngine;
using TickCombat.Combat;

namespace TickCombat.Passive
{
    public class HuyetMaThe : PassiveSkill, IDamageModifier
    {
        public override int Priority => 0;

        public void Modify(DamageContext ctx)
        {
            ctx.finalDamage = Mathf.Max(0, ctx.finalDamage - ctx.finalHeal);
            ctx.finalHeal = 0;
        }
    }
}
