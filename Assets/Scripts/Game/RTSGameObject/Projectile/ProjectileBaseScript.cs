using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.Game.RTSGameObject;

namespace RTS.Game.RTSGameObject.Projectile
{
    // RTSProjectileBaseScript would not report the RTSGO index to GameManager
    public class ProjectileBaseScript : RTSGameObjectBaseScript
    {
        [Header("Damage")]
        [Tooltip("Damage amount.")]
        public float damage;
        [Tooltip("Attack power reduced amount.")]
        public float attackPowerReduce;
        [Tooltip("Denfece power reduced amount.")]
        public float defencePowerReduce;
        [Tooltip("Move power reduced amount.")]
        public float movePowerReduce;

        [HideInInspector]
        public GameObject target;
        [HideInInspector]
        public GameObject createdBy;

        protected Rigidbody thisBody;

        void OnDestroy()
        {
            Debug.Log("Projectile OnDestroy Placeholder");
        }

        protected override void OnCreatedAction()
        {
            HP = maxHP;
        }

        protected override void OnDestroyedAction()
        {
            if (onDestroyedEffect != null)
            {
                Instantiate(onDestroyedEffect, transform.position, new Quaternion());
            }
        }
    }
}

