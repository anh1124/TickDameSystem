using System;
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
}
