using System;
using System.Collections.Generic;
using UnityEngine;
using TickCombat.Effect;

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

        // ===== EFFECT SYSTEM ===== (MỚI)
        readonly List<StatusEffect> activeEffects = new();

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

        // ===== EFFECT API ===== (MỚI)
        public void AddEffect(StatusEffect effect)
        {
            activeEffects.Add(effect);
        }

        /// <summary>
        /// Tick tất cả effects, remove expired
        /// Được gọi từ StatusProcessor.ResolveTick() TRƯỚC xử lý damage
        /// </summary>
        internal void TickEffects(int currentTick)
        {
            // Tick tất cả
            foreach (var effect in activeEffects)
            {
                effect.Tick(currentTick);
            }

            // Remove expired (backward loop để tránh index shift)
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i].IsExpired(currentTick))
                    activeEffects.RemoveAt(i);
            }
        }

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
}
