using MLAPI;
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

        public override bool Use()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return false;
            }
            if (Timer < coolDown)
            {
                return false;
            }
            Host.Repair(repairAmount, 0, 0, 0, Host.gameObject);
            Timer = 0;
            return true;
        }
    }
}