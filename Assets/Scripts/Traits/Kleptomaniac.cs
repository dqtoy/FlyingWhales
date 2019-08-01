﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kleptomaniac : Trait {
    public List<Character> noItemCharacters { get; private set; }
    private Character owner;

    private int _happinessDecreaseRate;
    public Kleptomaniac() {
        name = "Kleptomaniac";
        description = "This character has irresistible urge to steal.";
        thoughtText = "[Character] has irresistible urge to steal.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        noItemCharacters = new List<Character>();
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        //(sourceCharacter as Character).RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "afflicted", null, "Kleptomania");
        owner = sourceCharacter as Character;
        owner.AdjustHappinessDecreaseRate(_happinessDecreaseRate);
        base.OnAddTrait(sourceCharacter);
        owner.AddInteractionType(INTERACTION_TYPE.STEAL_CHARACTER);
        owner.AddInteractionType(INTERACTION_TYPE.STEAL);
        Messenger.AddListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter, Character removedBy) {
        base.OnRemoveTrait(sourceCharacter, removedBy);
        owner.RemoveInteractionType(INTERACTION_TYPE.STEAL_CHARACTER);
        owner.RemoveInteractionType(INTERACTION_TYPE.STEAL);
        owner.AdjustHappinessDecreaseRate(-_happinessDecreaseRate);
        Messenger.RemoveListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
    }
    public override void OnDeath(Character character) {
        base.OnDeath(character);
        Messenger.RemoveListener(Signals.DAY_STARTED, CheckForClearNoItemsList);
    }
    public override string GetTestingData() {
        string testingData = string.Empty;
        testingData += "Known character'S with no items: \n";
        for (int i = 0; i < noItemCharacters.Count; i++) {
            testingData += noItemCharacters[i].name + ", ";
        }
        return testingData;
    }
    protected override void OnChangeLevel() {
        base.OnChangeLevel();
        if (level == 1) {
            _happinessDecreaseRate = 10;
        } else if (level == 2) {
            _happinessDecreaseRate = 15;
        } else if (level == 3) {
            _happinessDecreaseRate = 20;
        }
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is SpecialToken) {
            SpecialToken token = traitOwner as SpecialToken;
            if (characterThatWillDoJob.currentAction != null && characterThatWillDoJob.currentAction.goapType == INTERACTION_TYPE.ROAMING_TO_STEAL && !characterThatWillDoJob.currentAction.isDone) {
                if ((token.characterOwner == null || token.characterOwner != characterThatWillDoJob) && characterThatWillDoJob.marker.CanDoStealthActionToTarget(traitOwner)) {
                    GoapPlanJob job = new GoapPlanJob(characterThatWillDoJob.currentAction.parentPlan.job.jobType, INTERACTION_TYPE.STEAL, traitOwner);

                    characterThatWillDoJob.currentAction.parentPlan.job.jobQueueParent.CancelJob(characterThatWillDoJob.currentAction.parentPlan.job);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    return true;
                }
            }
        } else if (traitOwner is Character) {
            //In Vampiric, the parameter traitOwner is the target character, that's why you must pass the target character in this parameter not the actual owner of the trait, the actual owner of the trait is the characterThatWillDoJob
            Character targetCharacter = traitOwner as Character;
            if (characterThatWillDoJob.currentAction != null && characterThatWillDoJob.currentAction.goapType == INTERACTION_TYPE.ROAMING_TO_STEAL && !characterThatWillDoJob.currentAction.isDone) {
                if (characterThatWillDoJob.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE && characterThatWillDoJob.marker.CanDoStealthActionToTarget(targetCharacter)) {
                    GoapPlanJob job = new GoapPlanJob(characterThatWillDoJob.currentAction.parentPlan.job.jobType, INTERACTION_TYPE.STEAL_CHARACTER, targetCharacter);

                    characterThatWillDoJob.currentAction.parentPlan.job.jobQueueParent.CancelJob(characterThatWillDoJob.currentAction.parentPlan.job);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    return true;
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    #endregion

    public void AddNoItemCharacter(Character character) {
        noItemCharacters.Add(character);
    }
    public void RemoveNoItemCharacter(Character character) {
        noItemCharacters.Remove(character);
    }

    private void ClearNoItemsList() {
        noItemCharacters.Clear();
        Debug.Log(GameManager.Instance.TodayLogString() + "Cleared " + owner.name + "'s Kleptomaniac list of character's with no items.");
    }

    private void CheckForClearNoItemsList() {
        //Store the character into the Kleptomaniac trait if it does not have any items. 
        //Exclude all characters listed in Kleptomaniac trait from Steal actions. Clear out the list at the start of every even day.
        if (Utilities.IsEven(GameManager.days)) {
            ClearNoItemsList();
        }
    }
}
