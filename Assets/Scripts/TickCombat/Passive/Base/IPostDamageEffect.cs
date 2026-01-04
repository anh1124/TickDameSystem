using TickCombat.Entity;

namespace TickCombat.Passive
{
    /// <summary>
    /// Passive trigger SAU khi damage/shield đã resolve
    /// KHÔNG dùng Priority, chạy sau IDamageModifier
    /// </summary>
    public interface IPostDamageEffect
    {
        void OnPostDamage(Status target, float shieldBefore, float shieldAfter);
    }
}
