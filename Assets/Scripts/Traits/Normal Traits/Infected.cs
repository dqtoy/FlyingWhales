using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Infected : Trait {

        private Character owner;
        private float pukeChance;

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
            owner.ForceCancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_TRAIT, name, removedBy);
        }
        public override void OnDeath(Character character) {
            base.OnDeath(character);
            if (character.characterClass.className == "Zombie") {
                //if the character that died is a zombie, remove this trait
                Messenger.RemoveListener<Character, Character>(Signals.CHARACTER_WAS_HIT, OnCharacterHit);
                owner.traitContainer.RemoveTrait(owner, this);
            } else {
                Messenger.RemoveListener(Signals.HOUR_STARTED, PerHour);
                SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(30)), StartReanimationCheck, this);
            }
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if (!targetCharacter.isDead && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name) && !targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL)
                    && !IsResponsibleForTrait(characterThatWillDoJob)) {
                    if (InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob(characterThatWillDoJob, targetCharacter)) {
                        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, target = GOAP_EFFECT_TARGET.TARGET };
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect, targetCharacter, characterThatWillDoJob);
                        job.AddOtherData(INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION });
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    } else {
                        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, target = GOAP_EFFECT_TARGET.TARGET };
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect, targetCharacter, characterThatWillDoJob.specificLocation);
                        job.AddOtherData(INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION });
                        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob);
                        characterThatWillDoJob.specificLocation.AddToAvailableJobs(job);
                    }
                    return true;
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
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
            float pukeRoll = Random.Range(0f, 100f);
            bool hasCreatedJob = false;
            if (pukeRoll < pukeChance) {
                //do puke action
                if (owner.characterClass.className == "Zombie" || (owner.currentActionNode != null && owner.currentActionNode.action.goapType == INTERACTION_TYPE.PUKE)) {
                    //If current action is a roaming action like Hunting To Drink Blood, we must requeue the job after it is removed by StopCurrentAction
                    return hasCreatedJob;
                }
                ActualGoapNode node = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.PUKE], owner, owner, null, 0);
                GoapPlan goapPlan = new GoapPlan(new List<JobNode>() { new SingleJobNode(node) }, owner);
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE, owner, owner);
                goapPlan.SetDoNotRecalculate(true);
                job.SetCannotBePushedBack(true);
                job.SetAssignedPlan(goapPlan);
                owner.jobQueue.AddJobInQueue(job);
                //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE, owner, owner);
                //owner.jobQueue.AddJobInQueue(job);
                hasCreatedJob = true;
            }
            return hasCreatedJob;
        }
        #endregion

        private void PerHour() {
            int roll = Random.Range(0, 100);
            if (roll < 2 && owner.isAtHomeRegion) { //2
                owner.marker.StopMovement();
                if (owner.currentActionNode.action != null && owner.currentActionNode.action.goapType != INTERACTION_TYPE.ZOMBIE_DEATH) {
                    owner.StopCurrentActionNode(false);
                } else if (owner.stateComponent.currentState != null) {
                    owner.stateComponent.ExitCurrentState();
                }

                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.ZOMBIE_DEATH, owner, owner);
                owner.jobQueue.AddJobInQueue(job);
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
                    if (hitCharacter.traitContainer.AddTrait(hitCharacter, "Infected", characterResponsible: hitBy)) {
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
}

