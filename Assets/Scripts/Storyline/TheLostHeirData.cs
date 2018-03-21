using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TheLostHeirData : StorylineData {
    public TheLostHeirData() : base(STORYLINE.LOST_HEIR) {

    }

    #region overrides
    public override void SetupStoryline() {
        base.SetupStoryline();

        List<ECS.Character> allChieftains = new List<ECS.Character>();
        for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) {
            Tribe currTribe = FactionManager.Instance.allTribes[i];
            allChieftains.Add(currTribe.leader);
        }
        ECS.Character chosenChieftain = allChieftains[Random.Range(0, allChieftains.Count)]; //Randomly select one of the Chieftains
        chosenChieftain.AssignTag(CHARACTER_TAG.TERMINALLY_ILL);//add a Terminally-Ill tag to him
        AddRelevantCharacter(chosenChieftain, new Log(GameManager.Instance.Today(), "Storylines", "TheLostHeir", "chieftain_title"));
        Log chieftainDescription = new Log(GameManager.Instance.Today(), "Storylines", "TheLostHeir", "chieftain_description");
        chieftainDescription.AddToFillers(chosenChieftain, chosenChieftain.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        AddCharacterLog(chosenChieftain, chieftainDescription);

        List<ECS.Character> possibleSuccessors = chosenChieftain.faction.characters.Where(x => x.id != chosenChieftain.id).ToList();
        ECS.Character chosenSuccessor = possibleSuccessors[Random.Range(0, possibleSuccessors.Count)]; //Randomly select one of the other characters of his Tribe
        Successor successorTag = chosenSuccessor.AssignTag(CHARACTER_TAG.SUCCESSOR) as Successor; //and a successor tag to him
        successorTag.SetCharacterToSucceed(chosenChieftain);
        CHARACTER_TAG chosenTag = CHARACTER_TAG.TYRANNICAL;
        //Also add either a tyrannical or warmonger tag to the successor
        if (Random.Range(0, 2) == 0) {
            chosenTag = CHARACTER_TAG.WARMONGER;
        }
        chosenSuccessor.AssignTag(chosenTag);

        AddRelevantCharacter(chosenSuccessor, new Log(GameManager.Instance.Today(), "Storylines", "TheLostHeir", "successor_title"));

        Log successorDescription = new Log(GameManager.Instance.Today(), "Storylines", "TheLostHeir", "successor_description_1");
        successorDescription.AddToFillers(chosenSuccessor, chosenSuccessor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        successorDescription.AddToFillers(chosenChieftain, chosenChieftain.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        AddCharacterLog(chosenSuccessor, successorDescription);

        Log successorDescription2 = new Log(GameManager.Instance.Today(), "Storylines", "TheLostHeir", "successor_description_2");
        successorDescription2.AddToFillers(chosenSuccessor, chosenSuccessor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        successorDescription2.AddToFillers(null, Utilities.NormalizeString(chosenTag.ToString()), LOG_IDENTIFIER.OTHER);
        AddCharacterLog(chosenSuccessor, successorDescription2);

        //If there is at least 1 Hut landmark in the world, generate a character in one of those Huts
        List<BaseLandmark> huts = LandmarkManager.Instance.GetLandmarksOfType(LANDMARK_TYPE.HUT);
        if (huts.Count > 0) {
            BaseLandmark chosenHut = huts[Random.Range(0, huts.Count)];
            ECS.Character lostHeir = chosenHut.CreateNewCharacter(chosenChieftain.raceSetting.race, CHARACTER_ROLE.HERMIT, "Swordsman");
            lostHeir.AssignTag(CHARACTER_TAG.LOST_HEIR); //and add a lost heir tag and an heirloom necklace item to him. That character should not belong to any faction.

            AddRelevantCharacter(lostHeir, new Log(GameManager.Instance.Today(), "Storylines", "TheLostHeir", "lost_heir_title"));

            Log lostHeirDescription = new Log(GameManager.Instance.Today(), "Storylines", "TheLostHeir", "lost_heir_description");
            lostHeirDescription.AddToFillers(lostHeir, lostHeir.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            lostHeirDescription.AddToFillers(chosenChieftain, chosenChieftain.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            AddCharacterLog(lostHeir, lostHeirDescription);

            ECS.Item heirloomNecklace = ItemManager.Instance.CreateNewItemInstance("Heirloom Necklace");
            lostHeir.PickupItem(heirloomNecklace);


            //Create find lost heir quest
            FindLostHeir findLostHeirQuest = new FindLostHeir(chosenChieftain, chosenChieftain, chosenSuccessor, lostHeir);
            QuestManager.Instance.AddQuestToAvailableQuests(findLostHeirQuest);
            chosenChieftain.AddActionOnDeath(findLostHeirQuest.ForceCancelQuest);
        }


    }
    #endregion
}
