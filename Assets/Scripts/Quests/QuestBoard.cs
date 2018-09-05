using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestBoard {

	public BaseLandmark owner { get; private set; }

    public QuestBoard(BaseLandmark owner) {
        this.owner = owner;
    }

    public Quest GenerateQuestForCharacter(ECS.Character character) {
        if (character.role.roleType != CHARACTER_ROLE.HERO) {
            throw new System.Exception("Non hero character " + character.name + " is trying to get a quest!");
        }

        return null;
    }
}
