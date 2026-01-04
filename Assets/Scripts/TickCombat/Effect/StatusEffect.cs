using TickCombat.Entity;
using TickCombat.Tick;

namespace TickCombat.Effect
{
    /// <summary>
    /// Base class cho mọi effect theo thời gian
    /// TICK-PURE: không có float timer
    /// </summary>
    public abstract class StatusEffect
    {
        protected readonly Status owner;

        readonly int startTick;
        readonly int durationTicks;
        readonly int triggerIntervalTicks;

        /// <summary>
        /// Check effect hết hạn dựa trên tick count
        /// </summary>
        public bool IsExpired(int currentTick)
            => currentTick >= startTick + durationTicks;

        protected StatusEffect(
            Status owner,
            int durationTicks,
            int triggerIntervalTicks)
        {
            this.owner = owner;
            this.startTick = TickManager.CurrentTick;
            this.durationTicks = durationTicks;
            this.triggerIntervalTicks = triggerIntervalTicks;
        }

        /// <summary>
        /// Được gọi MỖI TICK từ Status.TickEffects()
        /// </summary>
        public void Tick(int currentTick)
        {
            int elapsed = currentTick - startTick;

            // Trigger mỗi intervalTicks
            if (elapsed > 0 && elapsed % triggerIntervalTicks == 0)
            {
                OnTrigger();
            }
        }

        /// <summary>
        /// Logic effect cụ thể (regen, poison, buff...)
        /// </summary>
        protected abstract void OnTrigger();
    }
}
