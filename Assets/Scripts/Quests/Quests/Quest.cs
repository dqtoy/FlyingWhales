/*
 Base class for all quests. There should only be one instance of this per questline that all the characters will access.
 Make sure to code the questline in a way that it doesn't depend on values that are different for each character.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest {

    public int id { get; private set; }
    public QUEST_TYPE questType { get; private set; }

    public Quest(QUEST_TYPE questType) {
        this.id = Utilities.SetID(this);
        this.questType = questType;
        QuestManager.Instance.AddAvailableQuest(this);
    }

    public virtual CharacterAction GetQuestAction(ECS.Character character, CharacterQuestData data, ref IObject targetObject) {
        return null;
    }

}
