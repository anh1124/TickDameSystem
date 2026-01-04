using System;
using UnityEngine;
using TickCombat.Combat;
using TickCombat.Tick;

namespace TickCombat.Entity
{
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

        // ===== TICK RESOLVE ===== (CẬP NHẬT)
        void ResolveTick(int tickId)
        {
            if (!_status.IsAlive) return;

            // ===== 1. EFFECT TICK TRƯỚC ===== (MỚI)
            _status.TickEffects(tickId);

            // ===== 2. BUFFER SWAP =====
            _buffer.Swap();

            var damages = _buffer.ReadDamages;
            var heals = _buffer.ReadHeals;

            if (damages.Count == 0 && heals.Count == 0)
            {
                _buffer.ClearRead();
                return;
            }

            // ===== 3. CONTEXT INIT =====
            DamageContext ctx = new DamageContext
            {
                target = _status,
                snapshotHP = _status.Hp
            };

            // ===== 4. RAW DAMAGE =====
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

            // ===== 5. RAW HEAL =====
            foreach (var h in heals)
                if (h.tickId == tickId)
                    ctx.rawHeal += h.value;

            ctx.finalDamage = ctx.rawDamage;
            ctx.finalHeal = ctx.rawHeal;

            // ===== 6. PASSIVE MODIFY (Priority) =====
            _passiveManager?.ModifyDamage(ctx);

            // ===== 7. DEFENSE (HARDCODE) =====
            float mitigated = Mathf.Max(0, ctx.finalDamage - _status.Defense);

            // ===== 8. SHIELD GAIN =====
            if (ctx.shieldGenerated > 0)
            {
                float newShield = Mathf.Min(_status.Shield + ctx.shieldGenerated, _status.MaxShield);
                _status.SetShield(newShield);
                _status.InvokeShieldGained(ctx.shieldGenerated);
            }

            // ===== 9. TRACK SHIELD BEFORE ===== (MỚI)
            ctx.shieldBefore = _status.Shield;

            // ===== 10. SHIELD ABSORB =====
            float absorbed = Mathf.Min(_status.Shield, mitigated);
            _status.SetShield(_status.Shield - absorbed);
            mitigated -= absorbed;

            if (absorbed > 0)
                _status.InvokeShieldDamaged(absorbed);

            // ===== 11. TRACK SHIELD AFTER ===== (MỚI)
            ctx.shieldAfter = _status.Shield;

            // ===== 12. POST-DAMAGE TRIGGER ===== (MỚI)
            if (ctx.shieldBefore > ctx.shieldAfter)
            {
                _passiveManager?.TriggerPostDamage(_status, ctx.shieldBefore, ctx.shieldAfter);
            }

            // ===== 13. APPLY HP DAMAGE =====
            if (mitigated > 0)
            {
                _status.SetHp(_status.Hp - mitigated);
                _status.InvokeTotalDamageTaken(ctx.finalDamage);
                _status.InvokeHPDamaged(mitigated);
            }

            // ===== 14. APPLY HEAL =====
            if (ctx.finalHeal > 0)
            {
                float before = _status.Hp;
                _status.SetHp(_status.Hp + ctx.finalHeal);
                float healed = _status.Hp - before;
                _status.InvokeHealed(healed);
            }

            // ===== 15. DEATH CHECK =====
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
}
