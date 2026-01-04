using TickCombat.Entity;
using TickCombat.Effect;

namespace TickCombat.Passive
{
    /// <summary>
    /// Thực Cốt Xuân: Khi shield giảm → Regen 1% MaxHP/s trong 3s
    /// KHÔNG dùng Priority vì implement IPostDamageEffect
    /// </summary>
    public class ThucCotXuanPassive : PassiveSkill, IPostDamageEffect
    {
        // KHÔNG cần Priority vì không dùng IDamageModifier

        public void OnPostDamage(Status target, float shieldBefore, float shieldAfter)
        {
            // Trigger khi shield GIẢM (không quan tâm có bị phá hết hay không)
            if (shieldBefore > shieldAfter)
            {
                // Thêm RegenEffect:
                // - Duration: 3s = 60 ticks (3s / 0.05s)
                // - Interval: 1s = 20 ticks (1s / 0.05s)
                // - Heal: 1% MaxHP mỗi trigger
                target.AddEffect(new RegenEffect(
                    target,
                    durationTicks: 60,
                    triggerIntervalTicks: 20,
                    percentPerTrigger: 0.01f
                ));
            }
        }
    }
}
