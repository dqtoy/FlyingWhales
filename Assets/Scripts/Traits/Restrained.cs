using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Restrained : Trait {
        private Character _sourceCharacter;
        //private bool _createdFeedJob;

        public bool isPrisoner { get; private set; }
        public bool isCriminal { get; private set; }
        public bool isLeader { get; private set; }

        public override bool isRemovedOnSwitchAlterEgo {
            get { return true; }
        }

        public Restrained() {
            name = "Restrained";
            description = "This character is restrained!";
            thoughtText = "[Character] is imprisoned.";
            type = TRAIT_TYPE.DISABLER;
            effect = TRAIT_EFFECT.NEGATIVE;
            associatedInteraction = INTERACTION_TYPE.NONE;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED, INTERACTION_TYPE.RELEASE_CHARACTER };
            daysDuration = 0;
            //effects = new List<TraitEffect>();
            //_createdFeedJob = false;
        }

        #region Overrides
        public override string GetToolTipText() {
            if (responsibleCharacter == null) {
                return description;
            }
            return "This character is restrained by " + responsibleCharacter.name;
        }
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                _sourceCharacter = sourceCharacter as Character;
                isCriminal = _sourceCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL);
                isLeader = _sourceCharacter.role.roleType == CHARACTER_ROLE.LEADER;
                Messenger.AddListener(Signals.TICK_STARTED, CheckRestrainTrait);
                Messenger.AddListener(Signals.HOUR_STARTED, CheckRestrainTraitPerHour);
                //_sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_restrained");
                //_sourceCharacter.RemoveTrait("Unconscious", removedBy: responsibleCharacter);
                //_sourceCharacter.CancelAllJobsAndPlans();
                _sourceCharacter.AddTraitNeededToBeRemoved(this);

                //Once a faction leader is restrained set new faction leader
                if (isLeader) {
                    _sourceCharacter.faction.SetNewLeader();
                }
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (sourceCharacter is Character) {
                Character character = sourceCharacter as Character;
                character.CancelAllJobsTargettingThisCharacter(JOB_TYPE.FEED);
                character.CancelAllJobsTargettingThisCharacter(JOB_TYPE.JUDGEMENT);
                Messenger.RemoveListener(Signals.TICK_STARTED, CheckRestrainTrait);
                Messenger.RemoveListener(Signals.HOUR_STARTED, CheckRestrainTraitPerHour);
                //_sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
                _sourceCharacter.RemoveTraitNeededToBeRemoved(this);

                //If restrained trait is removed from this character this means that the character is set free from imprisonment, either he/she was saved from abduction or freed from criminal charges
                //When this happens, check if he/she was the leader of the faction, if true, he/she can only go back to being the ruler if he/she was not imprisoned because he/she was a criminal
                //But if he/she was a criminal, he/she cannot go back to being the ruler
                if (isLeader && !isCriminal) {
                    _sourceCharacter.faction.SetLeader(character);

                    Log logNotif = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "return_faction_leader");
                    logNotif.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    logNotif.AddToFillers(this, name, LOG_IDENTIFIER.FACTION_1);
                    _sourceCharacter.AddHistory(logNotif);
                    PlayerManager.Instance.player.ShowNotification(logNotif);
                }
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if (targetCharacter.isDead) {
                    return false;
                }
                if (!targetCharacter.isAtHomeRegion && !targetCharacter.isPartOfHomeFaction) {
                    if (InteractionManager.Instance.CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter, null) && !targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                        if (!IsResponsibleForTrait(characterThatWillDoJob)) {
                            characterThatWillDoJob.CreateSaveCharacterJob(targetCharacter, false);
                            return true;
                        }
                    }
                } else {
                    if (!targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                        SerialKiller serialKiller = characterThatWillDoJob.traitContainer.GetNormalTrait("Serial Killer") as SerialKiller;
                        if (serialKiller != null) {
                            serialKiller.SerialKillerSawButWillNotAssist(targetCharacter, this);
                            return false;
                        }
                        GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name);
                        if (currentJob == null) {
                            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
                            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect,
                                new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.TOOL } }, });
                            //job.SetCanBeDoneInLocation(true);
                            if (InteractionManager.Instance.CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter, job)) {
                                characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                                return true;
                            }
                            //else {
                            //    if (!IsResponsibleForTrait(characterThatWillDoJob)) {
                            //        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRemoveTraitJob);
                            //        characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                            //    }
                            //    return false;
                            //}
                        } else {
                            if (currentJob.jobQueueParent.isAreaOrQuestJobQueue && InteractionManager.Instance.CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                                bool canBeTransfered = false;
                                if (currentJob.assignedCharacter != null && currentJob.assignedCharacter.currentAction != null
                                    && currentJob.assignedCharacter.currentAction.parentPlan != null && currentJob.assignedCharacter.currentAction.parentPlan.job == currentJob) {
                                    if (currentJob.assignedCharacter != characterThatWillDoJob) {
                                        canBeTransfered = !currentJob.assignedCharacter.marker.inVisionPOIs.Contains(currentJob.assignedCharacter.currentAction.poiTarget);
                                    }
                                } else {
                                    canBeTransfered = true;
                                }
                                if (canBeTransfered && characterThatWillDoJob.CanCurrentJobBeOverriddenByJob(currentJob)) {
                                    currentJob.jobQueueParent.CancelJob(currentJob, shouldDoAfterEffect: false, forceRemove: true);
                                    characterThatWillDoJob.jobQueue.AddJobInQueue(currentJob, false);
                                    characterThatWillDoJob.jobQueue.AssignCharacterToJobAndCancelCurrentAction(currentJob, characterThatWillDoJob);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        #endregion

        private void CheckRestrainTrait() {
            if (isPrisoner && _sourceCharacter.IsInOwnParty()) {
                if (_sourceCharacter.isStarving) {
                    MoveFeedJobToTopPriority();
                } else if (_sourceCharacter.isHungry) {
                    CreateFeedJob();
                }
            }
        }
        private void CheckRestrainTraitPerHour() {
            if (!isPrisoner && _sourceCharacter.IsInOwnParty()) { //applies even if character is being Carried: so just remove the _sourceCharacter.IsInOwnParty(), right now it cannot happen while character is being carried
                if (_sourceCharacter.currentAction == null && _sourceCharacter.stateComponent.currentState == null && _sourceCharacter.stateComponent.stateToDo == null
                    && UnityEngine.Random.Range(0, 100) < 75 && !_sourceCharacter.jobQueue.HasJob(JOB_TYPE.SCREAM)
                    && _sourceCharacter.traitContainer.GetNormalTrait("Unconscious", "Resting") == null) {
                    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.SCREAM, INTERACTION_TYPE.SCREAM_FOR_HELP, _sourceCharacter);
                    _sourceCharacter.jobQueue.AddJobInQueue(job);
                }
            }
        }

        private void CreateFeedJob() {
            if (!_sourceCharacter.HasJobTargettingThis(JOB_TYPE.FEED)) {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = _sourceCharacter };
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.FEED, goapEffect);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRestrainedFeedJob);
                _sourceCharacter.specificLocation.jobQueue.AddJobInQueue(job);
            }
        }
        private void MoveFeedJobToTopPriority() {
            JobQueueItem feedJob = _sourceCharacter.specificLocation.jobQueue.GetJob(JOB_TYPE.FEED, _sourceCharacter);
            if (feedJob != null) {
                if (!_sourceCharacter.specificLocation.jobQueue.IsJobInTopPriority(feedJob)) {
                    _sourceCharacter.specificLocation.jobQueue.MoveJobToTopPriority(feedJob);
                }
            } else {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = _sourceCharacter };
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.FEED, goapEffect);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRestrainedFeedJob);
                _sourceCharacter.specificLocation.jobQueue.AddJobInQueue(job);
            }
        }
        private void CreateJudgementJob() {
            if (!_sourceCharacter.HasJobTargettingThis(JOB_TYPE.JUDGEMENT)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.JUDGEMENT, INTERACTION_TYPE.JUDGE_CHARACTER, _sourceCharacter);
                job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanDoJudgementJob);
                _sourceCharacter.gridTileLocation.structure.location.jobQueue.AddJobInQueue(job);
            }
        }
        public void SetIsPrisoner(bool state) {
            isPrisoner = state;
            if (isPrisoner && _sourceCharacter.IsInOwnParty()) {
                CreateJudgementJob();
            }
        }
    }

}
