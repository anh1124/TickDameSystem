Status hiện tại vi phạm Single Responsibility Principlecsharp❌ Status đang làm 4 việc:
1. Data Model (hp, defense, shield)
2. Passive Management (AddPassive, RebuildActive)
3. Combat Logic (ResolveTick - 80 lines)
4. Event Publisher (OnDamaged, OnHealed...)✅ KIẾN TRÚC ĐÚNG: TÁCH BIỆT TRÁCH NHIỆMModel-Processor PatternStatus.cs           → PURE DATA (model)
StatusProcessor.cs  → LOGIC (ResolveTick)
PassiveManager.cs   → PASSIVE (RebuildActive)📋 CẤU TRÚC MỚIEntity/
├── Status.cs                    // PURE MODEL
├── StatusProcessor.cs           // COMBAT LOGIC
└── PassiveManager.cs            // PASSIVE SYSTEM

Combat/
├── Pipeline/
│   ├── DamageResolver.cs        // Tính toán damage
│   ├── HealResolver.cs          // Tính toán heal
│   └── ShieldResolver.cs        // Xử lý shield
└── ... (giữ nguyên)💻 CODE MỚI1. Status.cs - PURE MODELcsharpusing System;
using UnityEngine;

namespace TickCombat.Entity
{
    /// <summary>
    /// PURE DATA - Không có logic tính toán
    /// </summary>
    public class Status : MonoBehaviour
    {
        // ===== STATS =====
        [SerializeField] float _maxHp = 100;
        [SerializeField] float _hp = 100;
        [SerializeField] float _defense = 10;
        [SerializeField] float _shield = 0;
        [SerializeField] float _maxShield = 200;

        bool _isAlive = true;

        // ===== PROPERTIES (READ-ONLY) =====
        public float MaxHp => _maxHp;
        public float Hp => _hp;
        public float Defense => _defense;
        public float Shield => _shield;
        public float MaxShield => _maxShield;
        public bool IsAlive => _isAlive;

        // ===== EVENTS =====
        public event Action<float> OnTotalDamageTaken;
        public event Action<float> OnHPDamaged;
        public event Action<float> OnShieldDamaged;
        public event Action<float> OnShieldGained;
        public event Action<float> OnHealed;
        public event Action OnDeath;

        // ===== INTERNAL MUTATORS (CHỈ StatusProcessor gọi) =====
        internal void SetHp(float value)
        {
            _hp = Mathf.Clamp(value, 0, _maxHp);
        }

        internal void SetShield(float value)
        {
            _shield = Mathf.Clamp(value, 0, _maxShield);
        }

        internal void SetAlive(bool value)
        {
            _isAlive = value;
        }

        // ===== EVENT INVOKERS (CHỈ StatusProcessor gọi) =====
        internal void InvokeTotalDamageTaken(float dmg) => OnTotalDamageTaken?.Invoke(dmg);
        internal void InvokeHPDamaged(float dmg) => OnHPDamaged?.Invoke(dmg);
        internal void InvokeShieldDamaged(float dmg) => OnShieldDamaged?.Invoke(dmg);
        internal void InvokeShieldGained(float shield) => OnShieldGained?.Invoke(shield);
        internal void InvokeHealed(float heal) => OnHealed?.Invoke(heal);
        internal void InvokeDeath() => OnDeath?.Invoke();
    }
}2. StatusProcessor.cs - COMBAT LOGICcsharpusing System;
using UnityEngine;
using TickCombat.Combat;
using TickCombat.Tick;

namespace TickCombat.Entity
{
    /// <summary>
    /// Xử lý combat logic cho Status
    /// </summary>
    [RequireComponent(typeof(Status))]
    public class StatusProcessor : MonoBehaviour
    {
        Status _status;
        PassiveManager _passiveManager;
        readonly DamageBuffer _buffer = new();

        void Awake()
        {
            _status = GetComponent<Status>();
            _passiveManager = GetComponent<PassiveManager>();
        }

        void OnEnable()
        {
            TickManager.OnTick += ResolveTick;
        }

        void OnDisable()
        {
            TickManager.OnTick -= ResolveTick;
        }

        // ===== PUBLIC API =====
        public void TakeDamage(Status source, float value, DamageType type)
        {
            if (!_status.IsAlive) return;

            _buffer.AddDamage(new DamageInstance
            {
                source = source,
                value = value,
                type = type,
                tickId = TickManager.CurrentTick
            });
        }

        public void Heal(Status source, float value)
        {
            if (!_status.IsAlive) return;

            _buffer.AddHeal(new HealInstance
            {
                source = source,
                value = value,
                tickId = TickManager.CurrentTick
            });
        }

        public void Attack(Status target, float damage, DamageType type)
        {
            if (!_status.IsAlive || target == null || !target.IsAlive)
                return;

            var targetProcessor = target.GetComponent<StatusProcessor>();
            if (targetProcessor == null) return;

            targetProcessor.TakeDamage(_status, damage, type);
            _passiveManager?.TriggerOnHit(_status, target);
        }

        // ===== TICK RESOLVE =====
        void ResolveTick(int tickId)
        {
            if (!_status.IsAlive) return;

            _buffer.Swap();

            var damages = _buffer.ReadDamages;
            var heals = _buffer.ReadHeals;

            if (damages.Count == 0 && heals.Count == 0)
            {
                _buffer.ClearRead();
                return;
            }

            DamageContext ctx = new DamageContext
            {
                target = _status,
                snapshotHP = _status.Hp
            };

            // RAW DAMAGE
            foreach (var d in damages)
            {
                if (d.tickId != tickId) continue;

                switch (d.type)
                {
                    case DamageType.PercentCurrentHP:
                        ctx.rawDamage += d.value * ctx.snapshotHP;
                        break;
                    default:
                        ctx.rawDamage += d.value;
                        break;
                }
            }

            // RAW HEAL
            foreach (var h in heals)
                if (h.tickId == tickId)
                    ctx.rawHeal += h.value;

            ctx.finalDamage = ctx.rawDamage;
            ctx.finalHeal = ctx.rawHeal;

            // PASSIVE MODIFY
            _passiveManager?.ModifyDamage(ctx);

            // DEFENSE (HARDCODE)
            float mitigated = Mathf.Max(0, ctx.finalDamage - _status.Defense);

            // SHIELD GAIN
            if (ctx.shieldGenerated > 0)
            {
                float newShield = Mathf.Min(_status.Shield + ctx.shieldGenerated, _status.MaxShield);
                _status.SetShield(newShield);
                _status.InvokeShieldGained(ctx.shieldGenerated);
            }

            // SHIELD ABSORB
            float absorbed = Mathf.Min(_status.Shield, mitigated);
            _status.SetShield(_status.Shield - absorbed);
            mitigated -= absorbed;

            if (absorbed > 0)
                _status.InvokeShieldDamaged(absorbed);

            // APPLY HP DAMAGE
            if (mitigated > 0)
            {
                _status.SetHp(_status.Hp - mitigated);
                _status.InvokeTotalDamageTaken(ctx.finalDamage);
                _status.InvokeHPDamaged(mitigated);
            }

            // APPLY HEAL
            if (ctx.finalHeal > 0)
            {
                float before = _status.Hp;
                _status.SetHp(_status.Hp + ctx.finalHeal);
                float healed = _status.Hp - before;
                _status.InvokeHealed(healed);
            }

            // DEATH CHECK
            if (_status.Hp <= 0 && _status.IsAlive)
            {
                bool prevented = _passiveManager?.TryPreventDeath(_status) ?? false;

                if (!prevented)
                {
                    _status.SetHp(0);
                    _status.SetAlive(false);
                    _status.InvokeDeath();
                }
                else
                {
                    _status.SetHp(1);
                }
            }

            _buffer.ClearRead();
        }
    }
}3. PassiveManager.cs - PASSIVE SYSTEMcsharpusing System;
using System.Collections.Generic;
using UnityEngine;
using TickCombat.Combat;
using TickCombat.Passive;

namespace TickCombat.Entity
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
    }
}🔍 SO SÁNHTiêu chíOld (Monolithic)New (Separated)Status.cs230 lines, 4 trách nhiệm60 lines, PURE DATATestability❌ Khó test logic✅ Mock Status dễ dàngReusability❌ Không tách được✅ Dùng Status ở AI, UIMaintainability❌ Sửa 1 chỗ ảnh hưởng nhiều✅ Tách biệt rõ ràngPerformance⚠️ Tương đương✅ Component cache tốt hơn