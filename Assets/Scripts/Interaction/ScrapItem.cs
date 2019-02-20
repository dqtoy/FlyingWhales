using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapItem : Interaction {
    private const string Start = "Start";
    private const string Scrapping_Cancelled = "Scrapping Cancelled";
    private const string Scrapping_Continues = "Scrapping Continues";
    private const string Normal_Scrapping = "Normal Scrapping";

    private SpecialToken _targetItem;

    public override LocationStructure actionStructureLocation {
        get { return _targetStructure; }
    }
    public ScrapItem(Area interactable): base(interactable, INTERACTION_TYPE.SCRAP_ITEM, 0) {
        _name = "Scrap Item";
    }

    #region Override
    public override void CreateStates() {
        if (_targetItem == null) {
            _targetItem = GetTargetItem(_characterInvolved);
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState scrappingCancelled = new InteractionState(Scrapping_Cancelled, this);
        InteractionState scrappingContinues = new InteractionState(Scrapping_Continues, this);
        InteractionState normalScrapping = new InteractionState(Normal_Scrapping, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetItem, _targetItem.tokenName, LOG_IDENTIFIER.ITEM_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        scrappingCancelled.SetEffect(() => ScrappingCancelledEffect(scrappingCancelled));
        scrappingContinues.SetEffect(() => ScrappingContinuesEffect(scrappingContinues));
        normalScrapping.SetEffect(() => NormalScrappingEffect(normalScrapping));

        _states.Add(startState.name, startState);
        _states.Add(scrappingCancelled.name, scrappingCancelled);
        _states.Add(scrappingContinues.name, scrappingContinues);
        _states.Add(normalScrapping.name, normalScrapping);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption preventOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from scrapping " + _targetItem.nameInBold,
                duration = 0,
                jobNeeded = JOB.DEBILITATOR,
                disabledTooltipText = "Must be a Dissuader.",
                effect = () => PreventOption(),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOption(),
            };
            state.AddActionOption(preventOption);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        _targetItem = GetTargetItem(character);
        if (_targetItem == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override bool CanStillDoInteraction(Character character) {
        if(_targetItem.structureLocation == null) {
            return false;
        }
        return base.CanStillDoInteraction(character);
    }
    #endregion

    #region Option Effect
    private void PreventOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Scrapping_Cancelled, investigatorCharacter.job.GetSuccessRate());
        effectWeights.AddElement(Scrapping_Continues, investigatorCharacter.job.GetFailRate());
        string result = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[result]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Normal_Scrapping]);
    }
    #endregion

    #region State Effect
    private void StartEffect(InteractionState state) {
        _targetStructure = _targetItem.structureLocation;
        _characterInvolved.MoveToAnotherStructure(_targetStructure);
    }
    private void ScrappingCancelledEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetItem, _targetItem.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(_targetItem, _targetItem.name, LOG_IDENTIFIER.ITEM_1));
    }
    private void ScrappingContinuesEffect(InteractionState state) {
        int scrappedSupply = UnityEngine.Random.Range(25, 151);
        _characterInvolved.homeArea.AdjustSuppliesInBank(scrappedSupply);
        _characterInvolved.specificLocation.RemoveSpecialTokenFromLocation(_targetItem);

        state.descriptionLog.AddToFillers(_targetItem, _targetItem.name, LOG_IDENTIFIER.ITEM_1);
        state.descriptionLog.AddToFillers(null, scrappedSupply.ToString(), LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(_targetItem, _targetItem.name, LOG_IDENTIFIER.ITEM_1));
        state.AddLogFiller(new LogFiller(null, scrappedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void NormalScrappingEffect(InteractionState state) {
        int scrappedSupply = UnityEngine.Random.Range(25, 151);
        _characterInvolved.homeArea.AdjustSuppliesInBank(scrappedSupply);
        _characterInvolved.specificLocation.RemoveSpecialTokenFromLocation(_targetItem);

        state.descriptionLog.AddToFillers(_targetItem, _targetItem.name, LOG_IDENTIFIER.ITEM_1);
        state.descriptionLog.AddToFillers(null, scrappedSupply.ToString(), LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(_targetItem, _targetItem.name, LOG_IDENTIFIER.ITEM_1));
        state.AddLogFiller(new LogFiller(null, scrappedSupply.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    #endregion

    private SpecialToken GetTargetItem(Character characterInvolved) {
        if(characterInvolved.specificLocation.possibleSpecialTokenSpawns.Count > 0) {
            return characterInvolved.specificLocation.possibleSpecialTokenSpawns[UnityEngine.Random.Range(0, characterInvolved.specificLocation.possibleSpecialTokenSpawns.Count)];
        }
        return null;
    }
}
