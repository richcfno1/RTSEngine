using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject.Unit;

namespace RTS.RTSGameObject.Subsystem
{
    public class HyperSpaceJumpDeviceScript : SpecialAbilitySubsystemBaseScript
    {
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

        public override bool Use(Vector3 target)
        {
            if (timer < coolDown)
            {
                return false;
            }
            //Host.GetComponent<Rigidbody>().MovePosition(target);
            Host.transform.position = target;
            timer = 0;
            return true;
        }
    }
}