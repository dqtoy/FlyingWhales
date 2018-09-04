using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Gregarious : CharacterTag {
    public Gregarious(Character character) : base(character, CHARACTER_TAG.GREGARIOUS) {
        _grantedActionTypes.Add(ACTION_TYPE.PARTY);
    }
}
