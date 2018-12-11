using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionOptionTraitRequirement : ActionOptionNeededObjectChecker {
    public TRAIT_REQUIREMENT categoryReq;
    public string[] requirements; //the operator for this is OR not AND, meaning if there is 1 match, return true

    public override bool IsMatch(Character character) {
        if (requirements == null) {
            return true;
        } else {
            if (categoryReq == TRAIT_REQUIREMENT.RACE) {
                for (int i = 0; i < requirements.Length; i++) {
                    if (character.race.ToString().ToLower() == requirements[i].ToLower()) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
public class ActionOptionLocationRequirement : ActionOptionNeededObjectChecker {
    public Area requiredLocation;

    public override bool IsMatch(Character character) {
        if (requiredLocation == null) {
            return false;
        }
        if (character.specificLocation.tileLocation.areaOfTile != null 
            && character.specificLocation.tileLocation.areaOfTile.id == requiredLocation.id) {
            return true;
        }
        return false;
    }
}

public class ActionOptionNeededObjectChecker {
    
    public virtual bool IsMatch(Character character) {
        return true;
    }
}
