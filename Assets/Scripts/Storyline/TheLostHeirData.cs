using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TheLostHeirData : StorylineData {

    private ECS.Character chieftain, falseHeir, lostHeir;
    private ECS.Item heirloomNecklace;
    private Quest findLostHeirQuest;
	private EliminateLostHeir eliminateLostHeirQuest;

    public TheLostHeirData() : base(STORYLINE.LOST_HEIR) {
        
    }

    #region overrides
    public override void InitialStorylineSetup() {
        base.InitialStorylineSetup();

        Messenger.AddListener<ECS.Item, BaseLandmark>(Signals.ITEM_PLACED_LANDMARK, OnHeirloomPlacedInLandmark);
        Messenger.AddListener<ECS.Item, ECS.Character>(Signals.ITEM_PLACED_INVENTORY, OnHeirloomPlacedInInventory);

        List<ECS.Character> allChieftains = new List<ECS.Character>();
        for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) {
            Tribe currTribe = FactionManager.Instance.allTribes[i];
            allChieftains.Add(currTribe.leader);
        }
        chieftain = allChieftains[Random.Range(0, allChieftains.Count)]; //Randomly select one of the Chieftains
        chieftain.AssignTag(CHARACTER_TAG.TERMINALLY_ILL);//add a Terminally-Ill tag to him
        AddRelevantItem(chieftain, CreateLogForStoryline("chieftain_title"));

        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, OnChieftainDied);

        Log chieftainDescription = CreateLogForStoryline("chieftain_description");
        chieftainDescription.AddToFillers(chieftain, chieftain.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        AddRelevantItem(chieftain, chieftainDescription);

        List<ECS.Character> possibleSuccessors = chieftain.faction.characters.Where(x => x.id != chieftain.id).ToList();
        falseHeir = possibleSuccessors[Random.Range(0, possibleSuccessors.Count)]; //Randomly select one of the other characters of his Tribe
        Successor successorTag = falseHeir.AssignTag(CHARACTER_TAG.SUCCESSOR) as Successor; //and a successor tag to him
        successorTag.SetCharacterToSucceed(chieftain);
        CHARACTER_TAG chosenTag = CHARACTER_TAG.TYRANNICAL;
        //Also add either a tyrannical or warmonger tag to the successor
        if (Random.Range(0, 2) == 0) {
            chosenTag = CHARACTER_TAG.WARMONGER;
        }
        falseHeir.AssignTag(chosenTag);

        AddRelevantItem(falseHeir, CreateLogForStoryline("successor_title"));

        Log successorDescription = CreateLogForStoryline("successor_description_1");
        successorDescription.AddToFillers(falseHeir, falseHeir.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        successorDescription.AddToFillers(chieftain, chieftain.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        AddRelevantItem(falseHeir, successorDescription);

        Log successorDescription2 = CreateLogForStoryline("successor_description_2");
        successorDescription2.AddToFillers(falseHeir, falseHeir.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        successorDescription2.AddToFillers(null, Utilities.NormalizeString(chosenTag.ToString()), LOG_IDENTIFIER.OTHER);
        AddRelevantItem(falseHeir, successorDescription2);

        //If there is at least 1 Hut landmark in the world, generate a character in one of those Huts
        List<BaseLandmark> huts = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.HUT);
        if (huts.Count > 0) {
            BaseLandmark chosenHut = huts[Random.Range(0, huts.Count)];
            lostHeir = chosenHut.CreateNewCharacter(chieftain.raceSetting.race, CHARACTER_ROLE.HERMIT, "Swordsman");
            lostHeir.AssignTag(CHARACTER_TAG.LOST_HEIR); //and add a lost heir tag and an heirloom necklace item to him. That character should not belong to any faction.

            AddRelevantItem(lostHeir, CreateLogForStoryline("lost_heir_title"));

            Log lostHeirDescription = CreateLogForStoryline("lost_heir_description");
            lostHeirDescription.AddToFillers(lostHeir, lostHeir.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            lostHeirDescription.AddToFillers(chieftain, chieftain.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            AddRelevantItem(lostHeir, lostHeirDescription);

            heirloomNecklace = ItemManager.Instance.CreateNewItemInstance("Heirloom Necklace");
            AddRelevantItem(heirloomNecklace, CreateLogForStoryline("heirloom_description"));
            lostHeir.PickupItem(heirloomNecklace);

            //Create find lost heir quest
            findLostHeirQuest = new FindLostHeir(chieftain, chieftain, falseHeir, lostHeir);
            QuestManager.Instance.AddQuestToAvailableQuests(findLostHeirQuest);
            //chieftain.AddActionOnDeath(findLostHeirQuest.ForceCancelQuest);

			eliminateLostHeirQuest = new EliminateLostHeir(chieftain, chieftain, falseHeir, lostHeir);
			QuestManager.Instance.AddQuestToAvailableQuests(eliminateLostHeirQuest);
//			chosenChieftain.AddActionOnDeath(eliminateLostHeirQuest.ForceCancelQuest);


            AddRelevantQuest(findLostHeirQuest);
			AddRelevantQuest(eliminateLostHeirQuest);

			Debug.Log("LOST HEIR LOCATION: " + chosenHut.landmarkName + " - " + chosenHut.tileLocation.tileName);

            _storylineDescription = CreateLogForStoryline("description");
            _storylineDescription.AddToFillers(chieftain, chieftain.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            _storylineDescription.AddToFillers(null, Utilities.NormalizeString(chosenTag.ToString()), LOG_IDENTIFIER.OTHER);
        }        
    }
    #endregion

    #region Logs
    protected override Log CreateLogForStoryline(string key) {
        return new Log(GameManager.Instance.Today(), "Storylines", "TheLostHeir", key);
    }
    private void OnHeirloomPlacedInLandmark(ECS.Item item, BaseLandmark landmark) {
        if (item == heirloomNecklace) {
            Log changeLocation = CreateLogForStoryline("heirloom_drop");
            changeLocation.AddToFillers(landmark, landmark.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
            ReplaceItemLog(item, changeLocation, 1);
        }
    }
    private void OnHeirloomPlacedInInventory(ECS.Item item, ECS.Character character) {
        if (item == heirloomNecklace) {
            Log changeLocation = CreateLogForStoryline("heirloom_change_possession");
            changeLocation.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            ReplaceItemLog(item, changeLocation, 1);
        }
    }
    #endregion

    private void OnChieftainDied(ECS.Character characterThatDied) {
        if (chieftain.id == characterThatDied.id) {
            Messenger.RemoveListener<ECS.Item, BaseLandmark>(Signals.ITEM_PLACED_LANDMARK, OnHeirloomPlacedInLandmark);
            Messenger.RemoveListener<ECS.Item, ECS.Character>(Signals.ITEM_PLACED_INVENTORY, OnHeirloomPlacedInInventory);
            Messenger.RemoveListener<ECS.Character>(Signals.CHARACTER_DEATH, OnChieftainDied);
            if (!findLostHeirQuest.isDone) {
                findLostHeirQuest.ForceCancelQuest();
            }
			if(!eliminateLostHeirQuest.isDone){
				eliminateLostHeirQuest.ForceCancelQuest ();
			}

            Log chieftainDeath = CreateLogForStoryline("chieftain_died");
            chieftainDeath.AddToFillers(chieftain, chieftain.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            chieftainDeath.AddToFillers(chieftain.faction.leader, chieftain.faction.leader.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            AddItemLog(chieftain, chieftainDeath);

            if (chieftain.faction.leader.id == falseHeir.id) {
                //the false heir succeeded the chieftain
                Log falseHeirSucceed = CreateLogForStoryline("successor_succeeded");
                falseHeirSucceed.AddToFillers(falseHeir, falseHeir.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                falseHeirSucceed.AddToFillers(chieftain, chieftain.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                AddItemLog(falseHeir, falseHeirSucceed);
            }
        }
    }
}
