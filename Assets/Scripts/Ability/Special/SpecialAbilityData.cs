using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RTS.Ability.SpecialAbility
{
    [CreateAssetMenu(fileName = "SpecialAbilityData", menuName = "RTSEngine/Create SpecialAbilityData", order = 2)]
    public class SpecialAbilityData : ScriptableObject
    {
        public string abilityID;
        public string abilityScriptTypeName;
        public Sprite icon;
    }
}