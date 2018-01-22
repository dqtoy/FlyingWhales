using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Adventurer : CharacterRole {

	public Adventurer(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.ADVENTURER;
        _allowedQuestTypes = new List<QUEST_TYPE>() {
            QUEST_TYPE.JOIN_PARTY
        };
    }
}
