using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie_Virus : Trait {
    private Character owner;
    public override bool isPersistent { get { return true; } }
    public Zombie_Virus() {
        name = "Zombie Virus";
        description = "This character is infected with a Zombie Virus.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        daysDuration = 0;
    }

    #region Override
    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        owner = addedTo as Character;
        Messenger.AddListener(Signals.HOUR_STARTED, PerHour);
    }
    public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        base.OnRemoveTrait(removedFrom, removedBy);
        Messenger.RemoveListener(Signals.HOUR_STARTED, PerHour);
        owner.marker.SetMarkerColor(Color.white);
    }
    public override void OnDeath(Character character) {
        base.OnDeath(character);
        if (character.characterClass.className == "Zombie") {
            //if the character that died is a zombie, remove this trait
            Messenger.RemoveListener<Character, Character>(Signals.CHARACTER_WAS_HIT, OnCharacterHit);
            owner.RemoveTrait(this);
        } else {
            Messenger.RemoveListener(Signals.HOUR_STARTED, PerHour);
            SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(30)), StartReanimationCheck, this);
        }
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is Character) {
            Character targetCharacter = traitOwner as Character;
            if (!targetCharacter.isDead && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name) && !targetCharacter.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect,
                    new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION } }, });
                if (CanCharacterTakeRemoveIllnessesJob(characterThatWillDoJob, targetCharacter, null)) {
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    return true;
                } else {
                    if (!IsResponsibleForTrait(characterThatWillDoJob)) {
                        job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveIllnessesJob);
                        characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                    }
                    return false;
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    #endregion

    private void PerHour() {
        int roll = Random.Range(0, 100);
        if (roll < 2 && owner.isAtHomeArea) { //2
            owner.marker.StopMovement();
            if (owner.currentAction != null && owner.currentAction.goapType != INTERACTION_TYPE.ZOMBIE_DEATH) {
                owner.StopCurrentAction(false);
            } else if (owner.stateComponent.currentState != null) {
                owner.stateComponent.currentState.OnExitThisState();
            }
            GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.ZOMBIE_DEATH, owner, owner);

            GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
            GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.ZOMBIE_DEATH);
            job.SetAssignedPlan(goapPlan);
            goapPlan.ConstructAllNodes();

            goapAction.CreateStates();
            owner.SetCurrentAction(goapAction);
            owner.currentAction.PerformActualAction();
        }
    }

    private void StartReanimationCheck() {
        Messenger.AddListener(Signals.TICK_ENDED, RollForReanimation);
        RollForReanimation(); //called this so, check will run immediately after the first 30 mins of being dead.
    }

    private void RollForReanimation() {
        //string summary = owner.name + " will roll for reanimation...";
        int roll = Random.Range(0, 100);
        //summary += "\nRoll is: " + roll.ToString(); 
        if (roll < 15) { //15
            Messenger.RemoveListener(Signals.TICK_ENDED, RollForReanimation);
            //reanimate
            //summary += "\n" + owner.name + " is being reanimated.";
            if (!owner.IsInOwnParty()) {
                //character is being carried, check per tick if it is dropped or buried, then reanimate
                Messenger.AddListener(Signals.TICK_ENDED, CheckIfCanReanimate);
            } else {
                Reanimate();
            }
            
        }
        //Debug.Log(summary);
    }

    private void CheckIfCanReanimate() {
        if (owner.IsInOwnParty()) {
            Reanimate();
            Messenger.RemoveListener(Signals.TICK_ENDED, CheckIfCanReanimate);
        }
    }

    private void Reanimate() {
        owner.RaiseFromDeath(faction: FactionManager.Instance.zombieFaction, race: owner.race, className: "Zombie");
        owner.marker.SetMarkerColor(Color.grey);
        Messenger.AddListener<Character, Character>(Signals.CHARACTER_WAS_HIT, OnCharacterHit);
    }

    private void OnCharacterHit(Character hitCharacter, Character hitBy) {
        if (hitBy == owner) {
            //a character was hit by the owner of this trait, check if the character that was hit becomes infected.
            int roll = Random.Range(0, 100);
            if (roll < 20) { //15
                if (hitCharacter.AddTrait("Zombie_Virus", characterResponsible: hitBy)) {
                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_zombie");
                    log.AddToFillers(hitCharacter, hitCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(hitBy, hitBy.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddLogToInvolvedObjects();
                    PlayerManager.Instance.player.ShowNotification(log);
                    //Debug.Log(GameManager.Instance.TodayLogString() + Utilities.LogReplacer(log));
                }
            }
        }
    }
}
