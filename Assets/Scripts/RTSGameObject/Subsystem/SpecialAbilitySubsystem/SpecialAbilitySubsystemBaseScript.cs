using MLAPI;
using MLAPI.NetworkVariable;
using UnityEngine;

namespace RTS.RTSGameObject.Subsystem
{
    public class SpecialAbilitySubsystemBaseScript : SubsystemBaseScript
    {
        public float coolDown;

        protected float Timer
        {
            get { return networkCoolDown.Value; }
            set { networkCoolDown.Value = value; }
        }

        private NetworkVariable<float> networkCoolDown = new NetworkVariable<float>(0);

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
            return Timer / coolDown;
        }
    }
}

