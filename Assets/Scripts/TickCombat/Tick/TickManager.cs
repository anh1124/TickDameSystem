using System;
using UnityEngine;

namespace TickCombat.Tick
{
    public class TickManager : MonoBehaviour
    {
        public static event Action<int> OnTick;
        public static int CurrentTick { get; private set; }

        public float tickInterval = 0.05f;
        float timer;

        void Update()
        {
            timer += Time.deltaTime;
            while (timer >= tickInterval)
            {
                timer -= tickInterval;
                CurrentTick++;
                OnTick?.Invoke(CurrentTick);
            }
        }
    }
}
