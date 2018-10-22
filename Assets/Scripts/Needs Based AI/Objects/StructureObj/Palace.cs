using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Palace : StructureObj {

    //private BuildStructureQuest activeBuildStructureQuest = null;

    public Palace() : base() {
        _specificObjectType = LANDMARK_TYPE.PALACE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Palace clone = new Palace();
        SetCommonData(clone);
        clone.Initialize();
        return clone;
    }
    #endregion

    public void Initialize() {
        //SchedulingManager.Instance.AddEntry(new GameDate(1, 1, 80, 2), () => StartOfMonth()); //so that only cloned palaces schedule monthly
        //SchedulingManager.Instance.AddEntry(new GameDate(1, 1, 80, 48), () => EndOfMonth()); //so that only cloned palaces schedule monthly
    }

    private void StartOfMonth() {
        UpdateAdvertisedChangeClassAction();
        ScheduleStartOfMonthActions();
        //CheckForBuildStructureQuest();
    }
    private void EndOfMonth() {
        ScheduleEndOfMonthActions();
        //CheckBuildStructureElligibility();
    }
    private void ScheduleStartOfMonthActions() {
        GameDate gameDate = GameManager.Instance.Today();
        gameDate.SetHours(2);
        gameDate.AddDays(1);
        SchedulingManager.Instance.AddEntry(gameDate, () => StartOfMonth());
    }
    private void ScheduleEndOfMonthActions() {
        GameDate gameDate = GameManager.Instance.Today();
        gameDate.SetHours(48);
        gameDate.AddDays(1);
        SchedulingManager.Instance.AddEntry(gameDate, () => EndOfMonth());
    }
    private void UpdateAdvertisedChangeClassAction() {
        ChangeClassAction changeClassAction = currentState.GetAction(ACTION_TYPE.CHANGE_CLASS) as ChangeClassAction;
        if (changeClassAction != null) {
            string highestPriorityMissingRole = string.Empty;
            for (int i = 0; i < objectLocation.tileLocation.areaOfTile.orderClasses.Count; i++) {
                if (objectLocation.tileLocation.areaOfTile.missingClasses.Contains(objectLocation.tileLocation.areaOfTile.orderClasses[i])) {
                    highestPriorityMissingRole = objectLocation.tileLocation.areaOfTile.orderClasses[i];
                    break;
                }
            }
            if (!string.IsNullOrEmpty(string.Empty)) {
                changeClassAction.SetAdvertisedClass(highestPriorityMissingRole);
            }
        }
    }

    //#region Build Structure Quest
    //private void CheckForBuildStructureQuest() {
    //    //At the start of each month, if the Settlement has no active Build Structure Quest
    //    if (activeBuildStructureQuest == null) {
    //        if (Random.Range(0, 100) < 100) { //there is a 20% chance that a Build Structure Quest will be created
    //            StructurePriority prio = this.objectLocation.tileLocation.areaOfTile.GetNextStructurePriority();
    //            List<HexTile> choices = this.objectLocation.tileLocation.areaOfTile.GetAdjacentBuildableTiles();
    //            if (prio != null && choices.Count > 0) {
    //                HexTile targetTile = choices[Random.Range(0, choices.Count)];
    //                List<RESOURCE> neededResources = new List<RESOURCE>(prio.setting.buildResourceCost.Select(x => x.resource));
    //                List<ECS.Character> elligibleCharacters = GetElligibleCharactersForBuildQuest(neededResources);
    //                if (elligibleCharacters.Count > 0) {
    //                    //create build structure quest
    //                    BuildStructureQuest buildQuest = new BuildStructureQuest(prio.setting, targetTile);
    //                    activeBuildStructureQuest = buildQuest;
    //                    string log = "Palace at " + this.objectLocation.landmarkName + " has created a quest to build a " + prio.setting.landmarkType.ToString() + " at " + targetTile.ToString() + ". Quest was assigned to the ff: ";
    //                    Messenger.AddListener<Quest>(Signals.QUEST_DONE, OnBuildDone);
    //                    //the quest will be given to all civilian Special Citizens of the settlement whose role can perform Harvest on required Resources.
    //                    for (int i = 0; i < elligibleCharacters.Count; i++) {
    //                        ECS.Character currChar = elligibleCharacters[i];
    //                        QuestManager.Instance.TakeQuest(buildQuest, currChar);
    //                        log += "\n" + currChar.name;
    //                    }
    //                    Debug.Log(log);
    //                }

    //            }
    //        }
    //    }
    //}
    //private List<ECS.Character> GetElligibleCharactersForBuildQuest(List<RESOURCE> neededResources) {
    //    List<ECS.Character> characters = new List<ECS.Character>();
    //    for (int i = 0; i < this.objectLocation.tileLocation.areaOfTile.owner.characters.Count; i++) {
    //        ECS.Character currChar = this.objectLocation.tileLocation.areaOfTile.owner.characters[i];
    //        if (currChar.IsSpecialCivilian() && currChar.CanObtainResource(neededResources)) {
    //            characters.Add(currChar);
    //        }
    //    }
    //    return characters;
    //}
    //private void CheckBuildStructureElligibility() {
    //    if (activeBuildStructureQuest != null) {
    //        if (activeBuildStructureQuest.GetAcceptedCharacters().Count <= 0) {
    //            //if there are no more characters on the Quest, assign the Quest to characters that can obtain the resource types it requires
    //            List<RESOURCE> neededResources = new List<RESOURCE>(activeBuildStructureQuest.setting.buildResourceCost.Select(x => x.resource));
    //            List<ECS.Character> elligibleCharacters = GetElligibleCharactersForBuildQuest(neededResources);
    //            if (elligibleCharacters.Count > 0) {
    //                //the quest will be given to all civilian Special Citizens of the settlement whose role can perform Harvest on required Resources.
    //                for (int i = 0; i < elligibleCharacters.Count; i++) {
    //                    ECS.Character currChar = elligibleCharacters[i];
    //                    QuestManager.Instance.TakeQuest(activeBuildStructureQuest, currChar);
    //                }
    //            } else {
    //                //If none available, remove this Quest.
    //                QuestManager.Instance.OnQuestDone(activeBuildStructureQuest);
    //            }
    //        }
    //    }
    //}
    //#endregion


    //private void OnBuildDone(Quest doneQuest) {
    //    if (activeBuildStructureQuest != null && doneQuest.id == activeBuildStructureQuest.id) {
    //        Messenger.RemoveListener<Quest>(Signals.QUEST_DONE, OnBuildDone);
    //        if (activeBuildStructureQuest.lackingResources.Count != 0) { //There are still lacking resources, means that he building was not completed, destroy the landmark
    //            LandmarkManager.Instance.DestroyLandmarkOnTile(activeBuildStructureQuest.targetTile);
    //        }
    //        activeBuildStructureQuest = null; //means that there is no active build quest, since the current one has been completed
    //    }
    //}
}
