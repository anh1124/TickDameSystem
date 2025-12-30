using TickCombat.Entity;

namespace TickCombat.Passive
{
    public abstract class PassiveSkill
    {
        public virtual int Priority => 0;
        public virtual bool Enabled => true;

        public virtual void OnAdded(Status owner) { }
        public virtual void OnRemoved(Status owner) { }
    }
}
