using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterHolderAbility : InstantAbilityBaseScript
{
    public List<GameObject> productionList;
    public Dictionary<string, int> containedFighters;
    public List<GameObject> deployedFighters;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // For FighterHolderAbility target size should be 2
    // target[0] = int where 0 = deploy contained fighter, 1 = call all deployed fighters back, 2 = produce
    // target[1] = game object to deploy/produce
    public override bool UseAbility(List<object> target)
    {
        return base.UseAbility(target);
    }
}
