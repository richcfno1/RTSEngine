using MLAPI;
using UnityEngine;

namespace RTS.Game.RTSGameObject.Subsystem
{
    public class HyperSpaceJumpDeviceScript : SpecialAbilitySubsystemBaseScript
    {
        // Start is called before the first frame update
        void Start()
        {
            OnCreatedAction();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }
            if (HP <= 0)
            {
                OnDestroyedAction();
            }
            if (!Active && HP / maxHP > repairPercentRequired)
            {
                OnSubsystemRepairedAction();
            }
            if (Timer < coolDown)
            {
                Timer += Time.fixedDeltaTime;
            }
        }

        public override bool Use(Vector3 target)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return false;
            }
            if (Timer < coolDown)
            {
                return false;
            }
            //Host.GetComponent<Rigidbody>().MovePosition(target);
            Host.transform.position = target;
            Timer = 0;
            return true;
        }
    }
}