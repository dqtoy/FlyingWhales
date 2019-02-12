using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReanimateAction : Interaction {

    private const string Reanimation_Success = "Reanimation Success";
    private const string Reanimation_Fail = "Reanimation Fail";
    private const string Corpse_Missing = "Corpse Missing";

    public override Character targetCharacter {
        get {
            if (_targetCorpse != null) {
                return _targetCorpse.character;
            }
            return null;
        }
    }

    private Corpse _targetCorpse;

    public ReanimateAction(Area interactable) 
        : base(interactable, INTERACTION_TYPE.REANIMATE_ACTION, 0) {
        _name = "Reanimate";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState reanimationSuccess = new InteractionState(Reanimation_Success, this);
        InteractionState reanimationFail = new InteractionState(Reanimation_Fail, this);
        InteractionState corpseMissing = new InteractionState(Corpse_Missing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState));
        reanimationSuccess.SetEffect(() => ReanimationSuccessRewardEffect(reanimationSuccess));
        reanimationFail.SetEffect(() => ReanimationFailRewardEffect(reanimationFail));
        corpseMissing.SetEffect(() => CorpseMissingRewardEffect(corpseMissing));

        _states.Add(startState.name, startState);

        _states.Add(reanimationSuccess.name, reanimationSuccess);
        _states.Add(reanimationFail.name, reanimationFail);
        _states.Add(corpseMissing.name, corpseMissing);

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
        if (character.homeArea.IsResidentsFull()) {
            return false;
        }
        Corpse corpse = GetTargetCorpse(character);
        if (corpse == null) {
            return false;
        }
        _targetCorpse = corpse;
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        string nextState = string.Empty;
        if (_targetCorpse.location.HasCorpseOf(targetCharacter)) {
            WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
            resultWeights.AddElement(Reanimation_Success, 50);
            resultWeights.AddElement(Reanimation_Fail, 20);
            nextState = resultWeights.PickRandomElementGivenWeights();
        } else {
            nextState = Corpse_Missing;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetCorpse.location);
    }
    private void ReanimationSuccessRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        TransferCharacter(targetCharacter, _characterInvolved.faction);
    }
    private void ReanimationFailRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void CorpseMissingRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion

    private void TransferCharacter(Character character, Faction faction) {
        interactable.RemoveCorpse(character);
        character.ReturnToLife();
        if (character.faction != null) {
            character.faction.RemoveCharacter(character);
        }
        faction.AddNewCharacter(character);
        character.MigrateHomeTo(_characterInvolved.homeArea);
        interactable.AddCharacterToLocation(character);
        character.SetDailyInteractionGenerationTick();
        Reanimated trait = new Reanimated();
        character.AddTrait(trait);
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, character.specificLocation);
        character.SetForcedInteraction(interaction);
        character.ChangeRace(RACE.SKELETON);
    }
    public Corpse GetTargetCorpse(Character characterInvolved) {
        if (interactable.corpsesInArea.Count > 0) {
            return interactable.corpsesInArea[Random.Range(0, interactable.corpsesInArea.Count)];
        }
        return null;
        //throw new System.Exception("Could not find any character to recruit!");
    }
}
