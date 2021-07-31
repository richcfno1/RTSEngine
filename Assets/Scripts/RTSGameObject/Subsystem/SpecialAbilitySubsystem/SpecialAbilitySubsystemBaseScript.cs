using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.RTSGameObject.Subsystem
{
    public class SpecialAbilitySubsystemBaseScript : SubsystemBaseScript
    {
        public float coolDown;

        protected float timer;

        public virtual bool Use()
        {
            return false;
        }

        public virtual bool Use(GameObject target)
        {
            return false;
        }
        public virtual bool Use(Vector3 target)
        {
            return false;
        }

        // 1 = ready
        public virtual float GetCoolDownPercent()
        {
            return timer / coolDown;
        }
    }
}

