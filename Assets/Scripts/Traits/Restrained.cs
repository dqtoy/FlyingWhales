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
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED, };
        daysDuration = 0;
        effects = new List<TraitEffect>();
        //_createdFeedJob = false;
    }

    #region Overrides
    public override void SetCharacterResponsibleForTrait(Character character) {
        _responsibleCharacter = character;
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
            _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_restrained");
            _sourceCharacter.RemoveTrait("Unconscious");

            _sourceCharacter.CancelAllJobsAndPlans();
        }
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        if(sourceCharacter is Character) {
            Character character = sourceCharacter as Character;
            character.CancelAllJobsTargettingThisCharacter("Feed");
            Messenger.RemoveListener(Signals.TICK_STARTED, CheckRestrainTrait);
            _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
        }
        base.OnRemoveTrait(sourceCharacter);
    }
    #endregion

    private void CheckRestrainTrait() {
        if (isPrisoner && _sourceCharacter.IsInOwnParty()) {
            if(_sourceCharacter.GetTrait("Hungry") != null) {
                CreateFeedJob();
            }else if (_sourceCharacter.GetTrait("Starving") != null) {
                MoveFeedJobToTopPriority();
            }
        }
    }

    private void CreateFeedJob() {
        if (!_sourceCharacter.HasJobTargettingThisCharacter("Feed")) {
            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = _sourceCharacter };
            GoapPlanJob job = new GoapPlanJob("Feed", goapEffect);
            job.SetCanTakeThisJobChecker(CanCharacterTakeFeedJob);
            _sourceCharacter.specificLocation.jobQueue.AddJobInQueue(job);
        }
    }
    private void MoveFeedJobToTopPriority() {
        JobQueueItem feedJob = _sourceCharacter.specificLocation.jobQueue.GetJob("Feed", _sourceCharacter);
        if (feedJob != null) {
            if (!_sourceCharacter.specificLocation.jobQueue.IsJobInTopPriority(feedJob)) {
                _sourceCharacter.specificLocation.jobQueue.MoveJobToTopPriority(feedJob);
            }
        } else {
            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = _sourceCharacter };
            GoapPlanJob job = new GoapPlanJob("Feed", goapEffect);
            job.SetCanTakeThisJobChecker(CanCharacterTakeFeedJob);
            _sourceCharacter.specificLocation.jobQueue.AddJobInQueue(job, true);
        }
    }
    private bool CanCharacterTakeFeedJob(Character character) {
        if (_sourceCharacter.specificLocation.IsResident(character)) {
            if(character.faction.id != FactionManager.Instance.neutralFaction.id) {
                return character.role.roleType == CHARACTER_ROLE.SOLDIER || character.role.roleType == CHARACTER_ROLE.CIVILIAN;
            } else {
                return true;
            }
        }
        return false;
    }
    private void CreateJudgementJob(Character targetCharacter) {
        if (!_sourceCharacter.HasJobTargettingThisCharacter("Judgement")) {
            GoapPlanJob job = new GoapPlanJob("Judgement", INTERACTION_TYPE.JUDGE_CHARACTER, targetCharacter);
            job.SetCanTakeThisJobChecker(CanDoJudgementJob);
            _sourceCharacter.gridTileLocation.structure.location.jobQueue.AddJobInQueue(job);
        }
    }
    private bool CanDoJudgementJob(Character character) {
        return character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.LEADER;
    }
    public void SetIsPrisoner(bool state) {
        isPrisoner = state;
        if(isPrisoner && _sourceCharacter.IsInOwnParty()) {
            CreateJudgementJob(_sourceCharacter);
        }
    }
}
