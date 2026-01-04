using UnityEngine;
using TickCombat.Entity;

namespace TickCombat.Effect
{
    /// <summary>
    /// Effect hồi máu theo % MaxHP mỗi interval
    /// QUAN TRỌNG: KHÔNG gọi owner.Heal() (buffer)
    /// </summary>
    public class RegenEffect : StatusEffect
    {
        readonly float percentPerTrigger;

        public RegenEffect(
            Status owner,
            int durationTicks,
            int triggerIntervalTicks,
            float percentPerTrigger)
            : base(owner, durationTicks, triggerIntervalTicks)
        {
            this.percentPerTrigger = percentPerTrigger;
        }

        protected override void OnTrigger()
        {
            float heal = owner.MaxHp * percentPerTrigger;

            // TRỰC TIẾP modify HP, bypass buffer
            float newHp = Mathf.Min(owner.Hp + heal, owner.MaxHp);
            owner.SetHp(newHp);
            owner.InvokeHealed(heal);
        }
    }
}
