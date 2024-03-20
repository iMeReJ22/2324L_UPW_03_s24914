using ScripInterfaces;
using UnityEngine;

namespace Enemies
{
    public class EnemyHealthSystem : MonoBehaviour, IHittable
    {
        [SerializeField] private int hp;
        public bool canGetHit = true;
        public bool IsAlive { get; set; } = true;
        public int GetHp()
        {
            return hp;
        }
        public void GetHit()
        {
            if (!canGetHit) return;
            hp--;
            if (hp <= 0) Die();
        }

        private void Die()
        {
            IsAlive = false;
            Destroy(gameObject);
        }
    }
}