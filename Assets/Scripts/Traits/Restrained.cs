using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restrained : Trait {
    private Character _responsibleCharacter;
    private Character _sourceCharacter;
    //private bool _createdFeedJob;

    public bool isPrisoner { get; private set; }

    #region getters/setters
    public override Character responsibleCharacter {
        get { return _responsibleCharacter; }
    }
    #endregion

    public Restrained() {
        name = "Restrained";
        description = "This character is restrained!";
        thoughtText = "[Character] is imprisoned.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED, INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION };
        daysDuration = 0;
        effects = new List<TraitEffect>();
        //_createdFeedJob = false;
    }

    #region Overrides
    public override void SetCharacterResponsibleForTrait(Character character) {
        _responsibleCharacter = character;
    }
    public override bool IsResponsibleForTrait(Character character) {
        return _responsibleCharacter == character;
    }
    public override string GetToolTipText() {
        if (_responsibleCharacter == null) {
            return description;
        }
        return "This character is restrained by " + _responsibleCharacter.name;
    }
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            _sourceCharacter = sourceCharacter as Character;
            Messenger.AddListener(Signals.TICK_STARTED, CheckRestrainTrait);
            //_sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_restrained");
            _sourceCharacter.RemoveTrait("Unconscious");
            _sourceCharacter.CancelAllJobsAndPlans();
            _sourceCharacter.AddTraitNeededToBeRemoved(this);
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        if(sourceCharacter is Character) {
            Character character = sourceCharacter as Character;
            character.CancelAllJobsTargettingThisCharacter(JOB_TYPE.FEED);
            Messenger.RemoveListener(Signals.TICK_STARTED, CheckRestrainTrait);
            //_sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
            _sourceCharacter.RemoveTraitNeededToBeRemoved(this);
        }
        base.OnRemoveTrait(sourceCharacter);
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        Character targetCharacter = traitOwner as Character;
        if (targetCharacter.isDead || !characterThatWillDoJob.isAtHomeArea) {
            return false;
        }
        if (targetCharacter.GetTraitOf(TRAIT_TYPE.CRIMINAL) == null && CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter, null)) {
            if (!targetCharacter.isAtHomeArea) {
                characterThatWillDoJob.CreateSaveCharacterJob(targetCharacter, false);
                return true;
            } else {
                if (!targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name)) {
                    if (CanCharacterTakeRemoveTraitJob(characterThatWillDoJob, targetCharacter, null)) {
                        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
                        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect);
                        //job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveTraitJob);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                        return true;
                    }
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    #endregion

    private void CheckRestrainTrait() {
        if (isPrisoner && _sourceCharacter.IsInOwnParty()) {
            if(_sourceCharacter.GetNormalTrait("Hungry") != null) {
                CreateFeedJob();
            }else if (_sourceCharacter.GetNormalTrait("Starving") != null) {
                MoveFeedJobToTopPriority();
            }
        }
    }

    private void CreateFeedJob() {
        if (!_sourceCharacter.HasJobTargettingThisCharacter(JOB_TYPE.FEED)) {
            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = _sourceCharacter };
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.FEED, goapEffect);
            job.SetCanTakeThisJobChecker(CanCharacterTakeFeedJob);
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
            job.SetCanTakeThisJobChecker(CanCharacterTakeFeedJob);
            _sourceCharacter.specificLocation.jobQueue.AddJobInQueue(job);
        }
    }
    private bool CanCharacterTakeFeedJob(Character character, JobQueueItem job) {
        if (_sourceCharacter.specificLocation.IsResident(character)) {
            if(character.faction.id != FactionManager.Instance.neutralFaction.id) {
                return character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN;
            } else {
                return true;
            }
        }
        return false;
    }
    private void CreateJudgementJob() {
        if (!_sourceCharacter.HasJobTargettingThisCharacter(JOB_TYPE.JUDGEMENT)) {
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.JUDGEMENT, INTERACTION_TYPE.JUDGE_CHARACTER, _sourceCharacter);
            job.SetCanTakeThisJobChecker(CanDoJudgementJob);
            _sourceCharacter.gridTileLocation.structure.location.jobQueue.AddJobInQueue(job);
        }
    }
    private bool CanDoJudgementJob(Character character, JobQueueItem job) {
        return character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.LEADER;
    }
    public void SetIsPrisoner(bool state) {
        isPrisoner = state;
        if(isPrisoner && _sourceCharacter.IsInOwnParty()) {
            CreateJudgementJob();
        }
    }
}
