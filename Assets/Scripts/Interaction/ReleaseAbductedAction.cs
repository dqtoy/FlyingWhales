using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseAbductedAction : Interaction {

    private Character _targetCharacter;

    private const string Release_Success = "Release Success";
    private const string Release_Fail = "Release Fail";
    private const string Release_Critical_Fail = "Release Critical Fail";
    private const string Target_Missing = "Target Missing";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    private LocationStructure _targetStructure;

    public ReleaseAbductedAction(Area interactable) 
        : base(interactable, INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION, 0) {
        _name = "Release Abducted Action";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState normalReleaseSuccess = new InteractionState(Release_Success, this);
        InteractionState normalReleaseFail = new InteractionState(Release_Fail, this);
        InteractionState normalReleaseCriticalFail = new InteractionState(Release_Critical_Fail, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        normalReleaseSuccess.SetEffect(() => NormalReleaseSuccessRewardEffect(normalReleaseSuccess));
        normalReleaseFail.SetEffect(() => NormalReleaseFailRewardEffect(normalReleaseFail));
        normalReleaseCriticalFail.SetEffect(() => NormalReleaseCriticalFailRewardEffect(normalReleaseCriticalFail));
        targetMissing.SetEffect(() => TargetMissingRewardEffect(targetMissing));

        _states.Add(startState.name, startState);

        _states.Add(normalReleaseSuccess.name, normalReleaseSuccess);
        _states.Add(normalReleaseFail.name, normalReleaseFail);
        _states.Add(normalReleaseCriticalFail.name, normalReleaseCriticalFail);
        _states.Add(targetMissing.name, targetMissing);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        /*
         Once the actual action is triggered, the character will check if the target to be saved is still in the location and 
         if its original home still has available resident capacity.
         */
        if (_targetCharacter == null 
            || _targetCharacter.specificLocation.id != interactable.id
            || _targetCharacter.GetTrait("Abducted") == null
            || (_targetCharacter.GetTrait("Abducted") as Abducted).originalHome.IsResidentsFull()
            || targetCharacter.isDead) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character targetCharacter) {
        this._targetCharacter = targetCharacter;
        AddToDebugLog("Set " + targetCharacter.name + " as target");
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        string nextState = string.Empty;
        if (targetCharacter.currentStructure == _targetStructure) {
            if (_targetStructure.charactersHere.Count == 2) { //target and saver only
                nextState = Release_Success;
            } else {
                nextState = Release_Success;
            }
        } else {
            nextState = Target_Missing;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void StartEffect(InteractionState state) {
        _targetStructure = _characterInvolved.GetCharacterRelationshipData(targetCharacter).knownStructure;
        _characterInvolved.MoveToAnotherStructure(_targetStructure);
    }
    private void NormalReleaseSuccessRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Remove Abducted trait from Character 2. Change Character 2 Home to its original one. Override his next tick to return home.
        _targetCharacter.ReleaseFromAbduction();

    }
    private void NormalReleaseFailRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalReleaseCriticalFailRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Character Name 1 dies.
        _characterInvolved.Death();
    }
    private void TargetMissingRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
