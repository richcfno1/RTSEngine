using MLAPI;
using MLAPI.NetworkVariable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.Ability.CommonAbility;
using RTS.Ability.SpecialAbility;
using RTS.RTSGameObject.Unit;

namespace RTS.RTSGameObject.Subsystem
{
    public class SubsystemBaseScript : RTSGameObjectBaseScript
    {
        public enum SubsystemType
        {
            None,
            SpecialAbilitySupporter,
            WeaponS,
            WeaponM,
            WeaponL,
            AxisWeaponS,
            AxisWeaponM,
            AxisWeaponL,
            CarrierS,
            CarrierM,
            CarrierL,
        }
        // Set by editor
        public SubsystemType type;
        public float repairPercentRequired;
        public List<CommonAbilityBaseScript.CommonAbilityType> supportedCommonAbility;

        // Set when instantiate
        public UnitBaseScript Host
        {
            get { return GameManager.GameManagerInstance.GetUnitByIndex(networkHost.Value); }
            set { networkHost.Value = value.Index; }
        }
        public string Anchor
        {
            get { return networkAnchor.Value; }
            set { networkAnchor.Value = value; }
        }
        public bool Active
        {
            get { return networkActive.Value; }
            set { networkActive.Value = value; }
        }

        protected List<object> subsystemTarget = new List<object>();

        private NetworkVariable<int> networkHost = new NetworkVariable<int>(-1);
        private NetworkVariable<string> networkAnchor = new NetworkVariable<string>("");
        private NetworkVariable<bool> networkActive = new NetworkVariable<bool>(false);

        protected override void OnCreatedAction()
        {
            base.OnCreatedAction();
            Active = true;
        }

        protected override void OnDestroyedAction()
        {
            Active = false;
        }

        protected virtual void OnSubsystemRepairedAction()
        {
            Active = true;
        }

        public virtual void SetTarget(List<object> target)
        {
            subsystemTarget = target;
        }

        public override void CreateDamage(float damage, float attackPowerReduce, float defencePowerReduce, float powerPowerReduce, GameObject from)
        {
            Host.CreateDamage(HP == 0 ? damage : 0, attackPowerReduce, defencePowerReduce, powerPowerReduce, from);
            base.CreateDamage(damage / Host.DefencePowerRatio, attackPowerReduce, defencePowerReduce, powerPowerReduce, from);
        }
        
        public override void Repair(float amount, float attackPowerRecover, float defencePowerRecover, float movePowerRecover, GameObject from)
        {
            Host.Repair(HP == maxHP ? amount : 0, attackPowerRecover, defencePowerRecover, movePowerRecover, from);
            base.Repair(amount, attackPowerRecover, defencePowerRecover, movePowerRecover, from);
        }
    }
}