using MLAPI;
using UnityEngine;
using RTS.Game.RTSGameObject.Unit;

namespace RTS.Game.RTSGameObject.Subsystem
{
    public class WeaponDisruptDeviceScript : SpecialAbilitySubsystemBaseScript
    {
        public float attackPowerReduceAmount;

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

        public override bool Use(GameObject target)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return false;
            }
            if (Timer < coolDown)
            {
                return false;
            }
            target.GetComponent<UnitBaseScript>().CreateDamage(0, attackPowerReduceAmount, 0, 0, Host.gameObject);
            Timer = 0;
            return true;
        }
    }
}