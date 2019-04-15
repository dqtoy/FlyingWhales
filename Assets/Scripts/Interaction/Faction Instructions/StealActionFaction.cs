using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealActionFaction : Interaction {
    
    private const string Theft_Success = "Theft Success";
    private const string Theft_Failed = "Theft Failed";
    private const string Thief_Caught = "Thief Caught";
    private const string Target_Itemless = "Target Itemless";
    private const string Target_Missing = "Target Missing";

    public StealActionFaction(Area interactable)
        : base(interactable, INTERACTION_TYPE.STEAL_ACTION_FACTION, 0) {
        _name = "Steal Action Faction";
        //_jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState theftSuccess = new InteractionState(Theft_Success, this);
        InteractionState theftFailed = new InteractionState(Theft_Failed, this);
        InteractionState thiefCaught = new InteractionState(Thief_Caught, this);
        InteractionState targetItemless = new InteractionState(Target_Itemless, this);
        InteractionState targetMissing = new InteractionState(Target_Missing, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        theftSuccess.SetEffect(() => TheftSuccessRewardEffect(theftSuccess));
        theftFailed.SetEffect(() => TheftFailedRewardEffect(theftFailed));
        thiefCaught.SetEffect(() => ThiefCaughtRewardEffect(thiefCaught));
        targetItemless.SetEffect(() => TargetItemlessRewardEffect(targetItemless));
        targetMissing.SetEffect(() => TargetMissingRewardEffect(targetMissing));

        _states.Add(startState.name, startState);
        _states.Add(theftSuccess.name, theftSuccess);
        _states.Add(theftFailed.name, theftFailed);
        _states.Add(thiefCaught.name, thiefCaught);
        _states.Add(targetItemless.name, targetItemless);
        _states.Add(targetMissing.name, targetMissing);

        //SetCurrentState(startState);
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
        Character target = GetTargetCharacter(character);
        if (character.isHoldingItem || target == null) {
            return false;
        }
        SetTargetCharacter(target, character);
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character character, Character actor) {
        _targetCharacter = character;
        _targetStructure = _targetCharacter.currentStructure;
        targetGridLocation = _targetCharacter.GetNearestUnoccupiedTileFromThis();
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        string nextState = string.Empty;
        if (_targetCharacter.currentStructure == _targetStructure) {
            if (_targetCharacter.isHoldingItem) {
                WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
                resultWeights.AddElement(Theft_Success, 50);
                resultWeights.AddElement(Theft_Failed, 30);
                resultWeights.AddElement(Thief_Caught, 15);
                nextState = resultWeights.PickRandomElementGivenWeights();
            } else {
                nextState = Target_Itemless;
            }
        } else {
            nextState = Target_Missing;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetStructure, targetGridLocation, _targetCharacter);
    }
    private void TheftSuccessRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            //state.descriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        //state.AddLogFiller(new LogFiller(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Transfer item from target character to the thief.
        TransferItem(_targetCharacter, _characterInvolved);
    }
    private void TheftFailedRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            //state.descriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        //state.AddLogFiller(new LogFiller(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void ThiefCaughtRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            //state.descriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        //state.AddLogFiller(new LogFiller(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Actor gains Restrained trait and is transferred to Work Area.
        _characterInvolved.AddTrait("Restrained");
        _characterInvolved.MoveToAnotherStructure(interactable.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA));
    }
    private void TargetItemlessRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void TargetMissingRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion

    private void TransferItem(Character sourceCharacter, Character thief) {
        //thief.ObtainToken(sourceCharacter.tokenInInventory);
        //sourceCharacter.UnobtainToken();
    }
    public Character GetTargetCharacter(Character characterInvolved) {
        /*
         Once the actual action is triggered, the character will randomly select any character in the location holding 
         an item that is not part of the character's faction. 
         */
        List<Character> choices = new List<Character>();
        for (int i = 0; i < interactable.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.charactersAtLocation[i];
            if (currCharacter.id != characterInvolved.id
                && !currCharacter.currentParty.icon.isTravelling
                && currCharacter.minion == null
                && currCharacter.currentStructure.isInside) {
                choices.Add(currCharacter);
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }
}
