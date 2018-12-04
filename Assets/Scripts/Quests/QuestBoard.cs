
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestBoard : IQuestGiver {

	public BaseLandmark owner { get; private set; }

    private List<Quest> postedQuests; //These are the quests that are posted by other quest givers (i.e. characters)

    #region getters/setters
    public int id {
        get { return owner.id; }
    }
    public string name {
        get { return owner.landmarkName; }
    }
    public IObject questGiverObj {
        get { return owner.landmarkObj; }
    }
    public QUEST_GIVER_TYPE questGiverType {
        get { return QUEST_GIVER_TYPE.QUEST_BOARD; }
    }
    #endregion

    public QuestBoard(BaseLandmark owner) {
        this.owner = owner;
        postedQuests = new List<Quest>();
        Messenger.AddListener<Quest>(Signals.QUEST_TURNED_IN, OnQuestTurnedIn);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    public Quest GetQuestForCharacter(Character character) {
        if (character.role.roleType != CHARACTER_ROLE.HERO) {
            throw new System.Exception("Non hero character " + character.name + " is trying to get a quest!");
        }

        if (postedQuests.Count > 0) {
            //give the hero a posted quest instead
            for (int i = 0; i < postedQuests.Count; i++) {
                Quest currQuest = postedQuests[i];
                if (currQuest.CanBeTakenBy(character)) {
                    return currQuest.Clone();
                }
            }
        } 

        //if the code reached here, it means that the character could not get a quest from the posted quests, generate one instead
        return GenerateQuestForCharacter(character);
    }
    private Quest GenerateQuestForCharacter(Character character) {
        //get all landmarks that are part of the area that can spawn monsters
        List<HexTile> tilesInRange = owner.tileLocation.GetTilesInRange(6);
        List<MonsterSpawnerLandmark> choices = new List<MonsterSpawnerLandmark>();
        for (int i = 0; i < tilesInRange.Count; i++) {
            HexTile currTile = tilesInRange[i];
            if (currTile.landmarkOnTile != null && currTile.landmarkOnTile is MonsterSpawnerLandmark && (currTile.landmarkOnTile as MonsterSpawnerLandmark).monsterChoices != null) {
                choices.Add(currTile.landmarkOnTile as MonsterSpawnerLandmark);
            }
        }
        if (choices.Count == 0) {
            Debug.LogWarning("Could not generate fetch quest for " + character.name + " because there are no monster spawner landmarks in area!");
        } else {
            MonsterSpawnerLandmark chosenLandmark = choices[Random.Range(0, choices.Count)];
            //once landmark has been picked. Pick a monster the character must encounter
            MonsterPartyComponent chosenSet = chosenLandmark.monsterChoices.parties[Random.Range(0, chosenLandmark.monsterChoices.parties.Length)];
            TextAsset chosenMonsterAsset = chosenSet.monsters[Random.Range(0, chosenSet.monsters.Length)];
            Monster chosenMonster = MonsterManager.Instance.monstersDictionary[chosenMonsterAsset.name];
            List<string> itemChoices = chosenMonster.itemDropsLookup.Where(x => x.Value > 0f && !x.Key.Contains("Scroll")).Select(x => x.Key).ToList(); //TODO: Remove scroll special case when done testing, this is so the fetch quest will not target scrolls
            string chosenItem = itemChoices[Random.Range(0, itemChoices.Count)]; //Then pick an item that that monster can drop
            int amount = Random.Range(2, 5); //randomize the amount of items needed for now
            FetchQuest fetchQuest = new FetchQuest(this, chosenLandmark, chosenItem, amount);
            return fetchQuest;
        }
        return null;
    }
    public void PostQuest(Quest quest) {
        postedQuests.Add(quest);
    }
    public void RemoveQuest(Quest quest) {
        postedQuests.Remove(quest);
    }
    public void RemoveQuest(List<Quest> quests) {
        for (int i = 0; i < quests.Count; i++) {
            RemoveQuest(quests[i]);
        }
    }
    public List<Quest> GetQuestsPostedBy(IQuestGiver questGiver) {
        List<Quest> quests = new List<Quest>();
        for (int i = 0; i < postedQuests.Count; i++) {
            Quest currQuest = postedQuests[i];
            if (currQuest.questGiver.id == questGiver.id && currQuest.questGiver.questGiverType == questGiver.questGiverType) {
                quests.Add(currQuest);
            }
        }
        return quests;
    }

    #region Listeners
    private void OnCharacterDied(Character character) {
        //check if the character that died had any quests posted here, if yes, remove that quest. NOTE: Cancelling the quest is handled by the quest itself!
        RemoveQuest(GetQuestsPostedBy(character));
    }
    private void OnQuestTurnedIn(Quest quest) {
        //once a quest has been turned in, remove that quest from the posted quests
        if (postedQuests.Contains(quest)) {
            RemoveQuest(quest);
        }
    }
    #endregion

}
