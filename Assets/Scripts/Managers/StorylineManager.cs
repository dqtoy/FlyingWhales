using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StorylineManager : MonoBehaviour {

    public static StorylineManager Instance = null;

    //[SerializeField]
    //private StorylineBoolDictionary storylineStore = StorylineBoolDictionary.New<StorylineBoolDictionary>();
    //private Dictionary<STORYLINE, bool> storylineDict {
    //    get { return storylineStore.dictionary; }
    //}

    public List<StorylineData> activeStorylines;

    private void Awake() {
        Instance = this;
        activeStorylines = new List<StorylineData>();
    }

	#region Utilities
    public void GenerateStoryLines() {
    //    foreach (KeyValuePair<STORYLINE, bool> kvp in storylineDict) {
    //        StorylineData createdData = null;
    //        if (kvp.Value) { //is the current storyline enabled?
    //            switch (kvp.Key) {
    //                case STORYLINE.THE_LOST_HEIR:
    //                    createdData = new TheLostHeirData();
    //                    //TriggerLostHeir();
    //                    break;
				//	case STORYLINE.INIMICAL_INCANTATIONS:
				//	createdData = new InimicalIncantationsData();
				//		break;
    //                default:
    //                    break;
    //            }
    //        }
    //        if (createdData != null) {
				//if(createdData.InitialStorylineSetup()){
				//	activeStorylines.Add(createdData);
				//}
    //        }
    //    }
        //UIManager.Instance.storylinesSummaryMenu.PopulateStorylinesTable();
    }

	public StorylineData GetStorylineData(STORYLINE storyline){
		for (int i = 0; i < activeStorylines.Count; i++) {
			if(activeStorylines[i].storyline == storyline){
				return activeStorylines [i];
			}
		}
		return null;
	}

	public void AddStoryline(StorylineData storylineData){
		if(storylineData.InitialStorylineSetup()){
			activeStorylines.Add(storylineData);
			//UIManager.Instance.storylinesSummaryMenu.PopulateStorylinesTable();
		}
	}
	#endregion

	#region Lost Heir
    //private void TriggerLostHeir() {
    //    string log = "Lost Heir Trigger Logs: ";
    //    List<ECS.Character> allChieftains = new List<ECS.Character>();
    //    for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) {
    //        Tribe currTribe = FactionManager.Instance.allTribes[i];
    //        allChieftains.Add(currTribe.leader);
    //    }
    //    ECS.Character chosenChieftain = allChieftains[Random.Range(0, allChieftains.Count)]; //Randomly select one of the Chieftains
    //    chosenChieftain.AssignTag(CHARACTER_TAG.TERMINALLY_ILL);//add a Terminally-Ill tag to him
    //    log += "\nChosen chieftain is " + chosenChieftain.name + " of " + chosenChieftain.faction.name;

    //    List<ECS.Character> possibleSuccessors = chosenChieftain.faction.characters.Where(x => x.id != chosenChieftain.id).ToList();
    //    ECS.Character chosenSuccessor = possibleSuccessors[Random.Range(0, possibleSuccessors.Count)]; //Randomly select one of the other characters of his Tribe
    //    Successor successorTag = chosenSuccessor.AssignTag(CHARACTER_TAG.SUCCESSOR) as Successor; //and a successor tag to him
    //    successorTag.SetCharacterToSucceed(chosenChieftain);
    //    log += "\nChosen successor is " + chosenSuccessor.name + " of " + chosenSuccessor.faction.name;

    //    //Also add either a tyrannical or warmonger tag to the successor
    //    if (Random.Range(0, 2) == 0) {
    //        chosenSuccessor.AssignTag(CHARACTER_TAG.TYRANNICAL);
    //        log += "\nAdded Tyrannical tag to " + chosenSuccessor.name;
    //    } else {
    //        chosenSuccessor.AssignTag(CHARACTER_TAG.WARMONGER);
    //        log += "\nAdded Warmonger tag to " + chosenSuccessor.name;
    //    }

    //    //If there is at least 1 Hut landmark in the world, generate a character in one of those Huts
    //    List<BaseLandmark> huts = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.HUT);
    //    if (huts.Count > 0) {
    //        BaseLandmark chosenHut = huts[Random.Range(0, huts.Count)];
    //        ECS.Character lostHeir = chosenHut.CreateNewCharacter(chosenChieftain.raceSetting.race, CHARACTER_ROLE.HERMIT, "Swordsman");
    //        lostHeir.AssignTag(CHARACTER_TAG.THE_LOST_HEIR); //and add a lost heir tag and an heirloom necklace item to him. That character should not belong to any faction.
    //        log += "\nAssigned lost heir to " + lostHeir.name + " at " + chosenHut.location.name;

    //        //Create find lost heir quest
    //        FindLostHeir findLostHeirQuest = new FindLostHeir(chosenChieftain, chosenChieftain, chosenSuccessor, lostHeir);
    //        QuestManager.Instance.AddQuestToAvailableQuests(findLostHeirQuest);
    //        chosenChieftain.AddActionOnDeath(findLostHeirQuest.ForceCancelQuest);
    //    }

    //    Debug.Log(log);
        
    //}
	#endregion

    #region Item Triggers
    /*
     Trigger specific events when an item is interacted with
         */
    public void OnInteractWith(ECS.Item item, BaseLandmark location, ECS.Character interacter) {
        //TODO: Add storyline triggers when a character interacts with a specific item
        switch (item.itemName) {
            case "Vampire Coffin":
                AwakenAncientVampire(item, location, interacter);
                break;
            default:
                break;
        }
    }
    #endregion

    #region Ancient Vampire
    private void AwakenAncientVampire(ECS.Item item, BaseLandmark location, ECS.Character interacter) {
        ECS.Character ancientVampire = (location as DungeonLandmark).SpawnAncientVampire();
        AncientVampireAwakened(location, ancientVampire);
        ////Get the ancient vampire at the location
        //ECS.Character ancientVampire = null;
        //for (int i = 0; i < location.charactersAtLocation.Count; i++) {
        //    ECS.Character currCharacter = location.charactersAtLocation[i].mainCharacter;
        //    if (currCharacter.role.roleType == CHARACTER_ROLE.ANCIENT_VAMPIRE) {
        //        ancientVampire = currCharacter;
        //        break;
        //    }
        //}
        //if (ancientVampire == null) {
        //    throw new System.Exception("There is no ancient vampire at " + location.locationName);
        //}
        //if (ancientVampire.currentTask.taskType != TASK_TYPE.HIBERNATE) {
        //    throw new System.Exception("Vampire is not hibernating!");
        //}

        //AncientVampireAwakened(location, ancientVampire);

        ////end the hibernation of the ancient vampire
        //ancientVampire.currentTask.EndTask(TASK_STATUS.SUCCESS);
    }
    public void AncientVampireAwakened(BaseLandmark location, ECS.Character ancientVampire) {
        location.RemoveItemInLandmark("Vampire Coffin");
        Log awakenLog = new Log(GameManager.Instance.Today(), "Quests", "AncientVampire", "awaken");
        awakenLog.AddToFillers(ancientVampire, ancientVampire.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        ancientVampire.AddHistory(awakenLog);
        location.AddHistory(awakenLog);
        //(ancientVampire.role as AncientVampire).OnAwakened();
    }
    #endregion
}
