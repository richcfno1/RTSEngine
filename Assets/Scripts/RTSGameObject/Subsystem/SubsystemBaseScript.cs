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
        public List<SpecialAbilityBaseScript.SpecialAbilityType> supportedSepcialAbility;

        // Set when instantiate
        public UnitBaseScript Host { get; set; }
        public bool Active { get; private set; }

        protected List<object> subsystemTarget = new List<object>();

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
            base.CreateDamage(damage / Host.DefencePower, attackPowerReduce, defencePowerReduce, powerPowerReduce, from);
        }
    }
}