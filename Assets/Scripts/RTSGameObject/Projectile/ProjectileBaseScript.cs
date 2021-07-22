using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject;

namespace RTS.RTSGameObject.Projectile
{
    // RTSProjectileBaseScript would not report the RTSGO index to GameManager
    public class ProjectileBaseScript : RTSGameObjectBaseScript
    {
        public float damage;
        public float attackPowerReduce;
        public float defencePowerReduce;
        public float movePowerReduce;

        [HideInInspector]
        public GameObject target;
        public GameObject createdBy;

        protected float timer;
        protected Rigidbody thisRigidbody;

        protected override void OnCreatedAction()
        {
            HP = maxHP;
        }

        protected override void OnDestroyedAction()
        {
            Destroy(gameObject);
            if (onDestroyedEffect != null)
            {
                Instantiate(onDestroyedEffect, transform.position, new Quaternion());
            }
        }
    }
}

