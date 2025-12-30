using TickCombat.Entity;

namespace TickCombat.Passive
{
    public interface IOnHitEffect
    {
        void OnHit(Status attacker, Status target);
    }
}
