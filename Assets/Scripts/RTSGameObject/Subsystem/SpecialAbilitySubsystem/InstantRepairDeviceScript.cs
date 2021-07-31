using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTS.RTSGameObject.Subsystem
{
    public class InstantRepairDeviceScript : SpecialAbilitySubsystemBaseScript
    {
        public float repairAmount;

        // Start is called before the first frame update
        void Start()
        {
            OnCreatedAction();
            timer = 0;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (HP <= 0)
            {
                OnDestroyedAction();
            }
            if (!Active && HP / maxHP > repairPercentRequired)
            {
                OnSubsystemRepairedAction();
            }
            if (timer < coolDown)
            {
                timer += Time.fixedDeltaTime;
            }
        }

        public override bool Use()
        {
            if (timer < coolDown)
            {
                return false;
            }
            Host.Repair(repairAmount, 0, 0, 0, Host.gameObject);
            timer = 0;
            return true;
        }
    }
}