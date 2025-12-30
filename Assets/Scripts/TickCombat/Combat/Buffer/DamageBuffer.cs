using System.Collections.Generic;

namespace TickCombat.Combat
{
    public class DamageBuffer
    {
        readonly List<DamageInstance> damageA = new();
        readonly List<DamageInstance> damageB = new();
        readonly List<HealInstance> healA = new();
        readonly List<HealInstance> healB = new();

        List<DamageInstance> writeDamage;
        List<HealInstance> writeHeal;
        List<DamageInstance> readDamage;
        List<HealInstance> readHeal;

        readonly object locker = new();

        public DamageBuffer()
        {
            writeDamage = damageA;
            readDamage  = damageB;
            writeHeal   = healA;
            readHeal    = healB;
        }

        public void AddDamage(DamageInstance dmg)
        {
            lock (locker)
                writeDamage.Add(dmg);
        }

        public void AddHeal(HealInstance heal)
        {
            lock (locker)
                writeHeal.Add(heal);
        }

        public void Swap()
        {
            lock (locker)
            {
                (writeDamage, readDamage) = (readDamage, writeDamage);
                (writeHeal, readHeal) = (readHeal, writeHeal);
            }
        }

        public List<DamageInstance> ReadDamages => readDamage;
        public List<HealInstance> ReadHeals => readHeal;

        public void ClearRead()
        {
            readDamage.Clear();
            readHeal.Clear();
        }
    }
}
