
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterActionTagRequirement {

    public ACTION_FILTER_CONDITION condition;
    public List<ATTRIBUTE> tags;

    public bool MeetsRequirement(Character character) {
        //if (condition == ACTION_FILTER_CONDITION.IS) {
        //    //the character must have ALL the tags to meet the requirement
        //    return character.HasAttributes(tags.ToArray(), true);
        //} else {
        //    //the character must have NONE of the tags to meet the requirement
        //    return !character.HasAttributes(tags.ToArray());
        //}
        return false;
    }

}
