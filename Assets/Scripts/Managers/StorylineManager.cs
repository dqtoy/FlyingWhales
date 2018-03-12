using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StorylineManager : MonoBehaviour {

    public static StorylineManager Instance = null;

    [SerializeField]
    private StorylineBoolDictionary storylineStore = StorylineBoolDictionary.New<StorylineBoolDictionary>();
    private Dictionary<STORYLINE, bool> storylineDict {
        get { return storylineStore.dictionary; }
    }

    private void Awake() {
        Instance = this;
    }

    public void GenerateStoryLines() {
        foreach (KeyValuePair<STORYLINE, bool> kvp in storylineDict) {
            if (kvp.Value) { //is the current storyline enabled?
                switch (kvp.Key) {
                    case STORYLINE.LOST_HEIR:
                        TriggerLostHeir();
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void TriggerLostHeir() {
        string log = "Lost Heir Trigger Logs: ";
        List<ECS.Character> allChieftains = new List<ECS.Character>();
        for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) {
            Tribe currTribe = FactionManager.Instance.allTribes[i];
            allChieftains.Add(currTribe.leader);
        }
        ECS.Character chosenChieftain = allChieftains[Random.Range(0, allChieftains.Count)]; //Randomly select one of the Chieftains
        chosenChieftain.AssignTag(CHARACTER_TAG.TERMINALLY_ILL);//add a Terminally-Ill tag to him
        log += "\nChosen chieftain is " + chosenChieftain.name + " of " + chosenChieftain.faction.name;

        List<ECS.Character> possibleSuccessors = chosenChieftain.faction.characters.Where(x => x.id != chosenChieftain.id).ToList();
        ECS.Character chosenSuccessor = possibleSuccessors[Random.Range(0, possibleSuccessors.Count)]; //Randomly select one of the other characters of his Tribe
        Successor successorTag = chosenSuccessor.AssignTag(CHARACTER_TAG.SUCCESSOR) as Successor; //and a successor tag to him
        successorTag.SetCharacterToSucceed(chosenChieftain);
        log += "\nChosen successor is " + chosenSuccessor.name + " of " + chosenSuccessor.faction.name;

        //Also add either a tyrannical or warmonger tag to the successor
        if (Random.Range(0, 2) == 0) {
            chosenSuccessor.AssignTag(CHARACTER_TAG.TYRANNICAL);
            log += "\nAdded Tyrannical tag to " + chosenSuccessor.name;
        } else {
            chosenSuccessor.AssignTag(CHARACTER_TAG.WARMONGER);
            log += "\nAdded Warmonger tag to " + chosenSuccessor.name;
        }

        //If there is at least 1 Hut landmark in the world, generate a character in one of those Huts
        List<BaseLandmark> huts = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.HUT);
        if (huts.Count > 0) {
            BaseLandmark chosenHut = huts[Random.Range(0, huts.Count)];
            ECS.Character lostHeir = chosenHut.CreateNewCharacter(chosenChieftain.raceSetting.race, CHARACTER_ROLE.NONE, "Swordsman");
            lostHeir.AssignTag(CHARACTER_TAG.LOST_HEIR); //and add a lost heir tag and an heirloom necklace item to him. That character should not belong to any faction.
            log += "\nAssigned lost heir to " + lostHeir.name + " at " + chosenHut.location.name;

            //Create find lost heir quest
            FindLostHeir findLostHeirQuest = new FindLostHeir(chosenChieftain, chosenChieftain, chosenSuccessor, lostHeir);
            QuestManager.Instance.AddQuestToAvailableQuests(findLostHeirQuest);
        }

        Debug.Log(log);
        
    }
}
