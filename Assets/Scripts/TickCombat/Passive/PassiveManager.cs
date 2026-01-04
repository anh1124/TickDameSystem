using System;
using System.Collections.Generic;
using UnityEngine;
using TickCombat.Combat;
using TickCombat.Entity;
using TickCombat.Passive;

namespace TickCombat.Passive
{
    /// <summary>
    /// Quản lý passive skills
    /// </summary>
    [RequireComponent(typeof(Status))]
    public class PassiveManager : MonoBehaviour
    {
        readonly List<PassiveSkill> _passives = new();
        readonly List<PassiveSkill> _activePassives = new();
        Status _owner;

        void Awake()
        {
            _owner = GetComponent<Status>();
        }

        public void AddPassive(PassiveSkill p)
        {
            _passives.Add(p);
            p.OnAdded(_owner);
            RebuildActive();
        }

        public void RemovePassive(PassiveSkill p)
        {
            _passives.Remove(p);
            try { p.OnRemoved(_owner); }
            catch (Exception e) { Debug.LogError(e); }
            RebuildActive();
        }

        void RebuildActive()
        {
            _activePassives.Clear();
            foreach (var p in _passives)
                if (p.Enabled)
                    _activePassives.Add(p);

            _activePassives.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        }

        // ===== INTERNAL API (CHỈ StatusProcessor gọi) =====
        internal void ModifyDamage(DamageContext ctx)
        {
            foreach (var p in _activePassives)
                if (p is IDamageModifier m)
                    m.Modify(ctx);
        }

        internal void TriggerOnHit(Status attacker, Status target)
        {
            foreach (var p in _activePassives)
                if (p is IOnHitEffect onHit)
                    onHit.OnHit(attacker, target);
        }

        internal bool TryPreventDeath(Status target)
        {
            foreach (var p in _activePassives)
                if (p is IDeathPrevention dp && dp.PreventDeath(target))
                    return true;
            return false;
        }

        // ===== POST-DAMAGE HOOK ===== (MỚI)
        internal void TriggerPostDamage(Status target, float shieldBefore, float shieldAfter)
        {
            foreach (var p in _activePassives)
            {
                if (p is IPostDamageEffect post)
                    post.OnPostDamage(target, shieldBefore, shieldAfter);
            }
        }
    }
}
