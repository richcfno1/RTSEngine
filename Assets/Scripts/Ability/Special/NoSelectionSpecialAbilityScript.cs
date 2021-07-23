using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RTS.RTSGameObject.Subsystem;

namespace RTS.Ability.SpecialAbility
{
    public class NoSelectionSpecialAbilityScript : SpecialAbilityBaseScript
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public virtual void UseAbility()
        {
            supportedBy.Use(null);
        }
    }
}

