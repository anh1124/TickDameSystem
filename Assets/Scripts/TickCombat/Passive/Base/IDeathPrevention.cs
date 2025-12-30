using TickCombat.Entity;

namespace TickCombat.Passive
{
    public interface IDeathPrevention
    {
        bool PreventDeath(Status target);
    }
}
