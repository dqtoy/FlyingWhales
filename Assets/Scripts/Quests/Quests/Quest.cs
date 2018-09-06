/*
 Base class for all quests. There should only be one instance of this per questline that all the characters will access.
 Make sure to code the questline in a way that it doesn't depend on values that are different for each character.
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest {

    public string name { get { return GetQuestName(); } }
    public string questDescription { get { return GetQuestDescription(); } }
    public int id { get; private set; }
    public QUEST_TYPE questType { get; private set; }

    public Quest(QUEST_TYPE questType) {
        this.id = Utilities.SetID(this);
        this.questType = questType;
        //QuestManager.Instance.AddAvailableQuest(this);
    }

    public virtual QuestAction GetQuestAction(ECS.Character character, CharacterQuestData data) {
        return null;
    }
    protected virtual string GetQuestName() {
        return questType.ToString();
    }
    protected virtual string GetQuestDescription() {
        return "This is the default quest description";
    }
    //public List<ECS.Character> GetAcceptedCharacters() {
    //    List<ECS.Character> characters = new List<ECS.Character>();
    //    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
    //        ECS.Character currChar = CharacterManager.Instance.allCharacters[i];
    //        if (!currChar.isDead && currChar.HasQuest(this)) {
    //            characters.Add(currChar);
    //        }
    //    }
    //    return characters;
    //}

}
