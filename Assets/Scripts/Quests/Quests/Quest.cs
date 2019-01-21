/*
 Base class for all quests. There should only be one instance of this per questline that all the characters will access.
 Make sure to code the questline in a way that it doesn't depend on values that are different for each character.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 NOTES:
 Flow for setting quest done is SetQuestAsDone() => OnQuestDone() => OnQuestTurnedIn()
     */

public class Quest {

    public string name { get { return GetQuestName(); } }
    public string questDescription { get { return GetQuestDescription(); } }
    public int id { get; private set; }
    public QUEST_TYPE questType { get; private set; }
    public IQuestGiver questGiver { get; private set; }
    public Character owner { get; private set; }
    public bool isQuestDone { get; private set; }

    public Quest(QUEST_TYPE questType, IQuestGiver questGiver) {
        this.id = Utilities.SetID(this);
        this.questType = questType;
        this.questGiver = questGiver;
        this.isQuestDone = false;
        SubscribeToListeners();
        //QuestManager.Instance.AddAvailableQuest(this);
    }

    #region virtuals
    public virtual Quest Clone() {
        return null;
    }
    public virtual QuestAction GetQuestAction(Character character) {
        return null;
    }
    protected virtual string GetQuestName() {
        return questType.ToString();
    }
    protected virtual string GetQuestDescription() {
        return "This is the default quest description";
    }
    public virtual void OnAcceptQuest(Character accepter) {
        owner = accepter;
    }
    /*
     This is called to set the quest as done.
     Done meaning the objective of the quest has been met, but the character
     has not yet turned in the quest (got back to the quest giver and collected the rewards)
         */
    public virtual void SetQuestAsDone() {
        if (isQuestDone) {
            return; //quest is already done
        }
        Debug.Log(this.owner.party.name + " set quest as done.");
        isQuestDone = true;
        OnQuestDone();
    }
    /*
     This is called after the quest has been set as done (SetQuestAsDone()).
         */
    protected virtual void OnQuestDone() {}
    /*
     This is called at the TurnInQuestAction
         */
    public virtual void OnQuestTurnedIn() {
        owner = null;
        Messenger.Broadcast(Signals.QUEST_TURNED_IN, this);
        RemoveListeners();
    }
    /*
     This is used for when something unusual happens that renders
     this quest invalid (i.e. quest giver died/quest giver landmark is destroyed)
         */
    protected virtual void CancelQuest() {
        //owner.SetQuest(null);
        RemoveListeners();
    }
    /*
     Can this quest be taken by the character
         */
    public virtual bool CanBeTakenBy(Character character) {
        return true;
    }
    #endregion

    #region Listeners
    private void SubscribeToListeners() {
        if (questGiver.questGiverType == QUEST_GIVER_TYPE.CHARACTER) {
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        }
        //TODO: Add Listener for when a landmark dies
    }
    private void RemoveListeners() {
        if (questGiver.questGiverType == QUEST_GIVER_TYPE.CHARACTER) {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        }
    }
    /*
     In the case that this quest was given by a character and
     that character died.
         */
    private void OnCharacterDied(Character characterThatDied) {
        if (questGiver.questGiverType == QUEST_GIVER_TYPE.CHARACTER 
            && questGiver.id == characterThatDied.id) {
            CancelQuest();
        }
    }
    #endregion

    protected void SetCommonData(Quest quest) {
        //quest.questType = this.questType;
        //quest.questGiver = this.questGiver;
    }

    //public List<Character> GetAcceptedCharacters() {
    //    List<Character> characters = new List<Character>();
    //    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
    //        Character currChar = CharacterManager.Instance.allCharacters[i];
    //        if (!currChar.isDead && currChar.HasQuest(this)) {
    //            characters.Add(currChar);
    //        }
    //    }
    //    return characters;
    //}

}
