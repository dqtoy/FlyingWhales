using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Infected : Trait {
    private Character owner;
    private float pukeChance;
    private CharacterState pausedState;

    public override bool isPersistent { get { return true; } }
    public Infected() {
        name = "Infected";
        description = "This character has the zombie virus.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        daysDuration = 0;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER, };
        mutuallyExclusive = new string[] { "Robust" };
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
        owner.CancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_TRAIT, name, removedBy);
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
    //public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
    //    if (traitOwner is Character) {
    //        Character targetCharacter = traitOwner as Character;
    //        if (!targetCharacter.isDead && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name) && !targetCharacter.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
    //            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
    //            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect,
    //                new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.CRAFT_ITEM_GOAP, new object[] { SPECIAL_TOKEN.HEALING_POTION } }, });
    //            job.SetCanBeDoneInLocation(true);
    //            if (InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob(characterThatWillDoJob, targetCharacter, job)) {
    //                characterThatWillDoJob.jobQueue.AddJobInQueue(job);
    //                return true;
    //            } else {
    //                if (!IsResponsibleForTrait(characterThatWillDoJob)) {
    //                    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob);
    //                    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
    //                }
    //                return false;
    //            }
    //        }
    //    }
    //    return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    //}
    protected override void OnChangeLevel() {
        if (level == 1) {
            pukeChance = 5f;
        } else if (level == 2) {
            pukeChance = 7f;
        } else {
            pukeChance = 9f;
        }
    }
    public override bool PerTickOwnerMovement() {
        //string summary = owner.name + " is rolling for plagued chances....";
        float pukeRoll = Random.Range(0f, 100f);
        //summary += "\nPuke roll is: " + pukeRoll.ToString();
        //summary += "\nSeptic Shock roll is: " + septicRoll.ToString();
        bool hasCreatedJob = false;
        if (pukeRoll < pukeChance) {
            //summary += "\nPuke chance met. Doing puke action.";
            //do puke action
            if (owner.currentAction != null && owner.currentAction.goapType != INTERACTION_TYPE.PUKE) {
                //If current action is a roaming action like Hunting To Drink Blood, we must requeue the job after it is removed by StopCurrentAction
                JobQueueItem currentJob = null;
                JobQueue currentJobQueue = null;
                if (owner.currentAction.isRoamingAction && owner.currentAction.parentPlan != null && owner.currentAction.parentPlan.job != null) {
                    currentJob = owner.currentAction.parentPlan.job;
                    currentJobQueue = currentJob.jobQueueParent;
                }
                owner.StopCurrentAction(false);
                if (currentJob != null) {
                    currentJobQueue.AddJobInQueue(currentJob, false);
                }
                owner.marker.StopMovement();

                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
                job.SetAssignedPlan(goapPlan);
                goapPlan.ConstructAllNodes();
                goapAction.CreateStates();
                owner.jobQueue.AddJobInQueue(job, false);

                owner.SetCurrentAction(goapAction);
                //owner.currentAction.SetEndAction(ResumeLastAction);
                owner.currentAction.DoAction();
                hasCreatedJob = true;
            } else if (owner.stateComponent.currentState != null) {
                pausedState = owner.stateComponent.currentState;
                owner.stateComponent.currentState.PauseState();
                owner.marker.StopMovement();
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
                job.SetAssignedPlan(goapPlan);
                goapPlan.ConstructAllNodes();
                goapAction.CreateStates();
                owner.jobQueue.AddJobInQueue(job, false);

                owner.SetCurrentAction(goapAction);
                owner.currentAction.SetEndAction(ResumePausedState);
                owner.currentAction.DoAction();
                hasCreatedJob = true;
            } else {
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PUKE, owner, owner);

                GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
                GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE);
                job.SetAssignedPlan(goapPlan);
                goapPlan.ConstructAllNodes();
                goapAction.CreateStates();
                owner.jobQueue.AddJobInQueue(job, false);

                owner.SetCurrentAction(goapAction);
                owner.currentAction.DoAction();
                hasCreatedJob = true;
            }
            //Debug.Log(summary);
        }
        return hasCreatedJob;
    }
    #endregion

    private void ResumePausedState(string result, GoapAction action) {
        owner.GoapActionResult(result, action);
        pausedState.ResumeState();
    }

    private void PerHour() {
        int roll = Random.Range(0, 100);
        if (roll < 2 && owner.isAtHomeRegion) { //2
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
            owner.currentAction.Perform();
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
            string summary = hitCharacter.name + " was hit by " + hitBy.name + ". Rolling for infect...";
            int roll = Random.Range(0, 100);
            summary += "\nRoll is " + roll.ToString();
            if (roll < 20) { //15
                summary += "\nChance met, " + hitCharacter.name + " will turn into a zombie.";
                if (hitCharacter.AddTrait("Infected", characterResponsible: hitBy)) {
                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "contracted_zombie");
                    log.AddToFillers(hitCharacter, hitCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(hitBy, hitBy.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddLogToInvolvedObjects();
                    PlayerManager.Instance.player.ShowNotification(log);
                    //Debug.Log(GameManager.Instance.TodayLogString() + Utilities.LogReplacer(log));
                } else {
                    summary += "\n" + hitCharacter.name + " is already a zombie!";
                }
            }
            Debug.Log(GameManager.Instance.TodayLogString() + summary);
        }
    }
}
