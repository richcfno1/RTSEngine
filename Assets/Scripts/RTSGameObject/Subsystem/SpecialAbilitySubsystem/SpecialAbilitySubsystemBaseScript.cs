using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.RTSGameObject.Subsystem
{
    public class SpecialAbilitySubsystemBaseScript : SubsystemBaseScript
    {
        public float coolDown;

        protected float timer;

        public virtual bool Use(Object target)
        {
            return true;
        }

        public virtual float GetCoolDownPercent()
        {
            return timer / coolDown;
        }
    }
}

