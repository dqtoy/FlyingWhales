using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionOptionNeededObjectChecker {
    public TRAIT_REQUIREMENT categoryReq;
    public string[] requirements; //the operator for this is OR not AND, meaning if there is 1 match, return true

    public bool IsMatch(Character character) {
        if(requirements == null) {
            return true;
        } else {
            if(categoryReq == TRAIT_REQUIREMENT.RACE) {
                for (int i = 0; i < requirements.Length; i++) {
                    if(character.race.ToString().ToLower() == requirements[i].ToLower()) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
