using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploreEvent : Interaction {

    private const string Trapped_Explore_Success_Obtain_Item = "Trapped Explore Success - Obtain Item";
    private const string Trapped_Explore_Success_No_Item = "Trapped Explore Success - No Item";
    private const string Trapped_Explore_Fail = "Trapped Explore Fail";
    private const string Trapped_Explore_Critical_Fail = "Trapped Explore Critical Fail";
    private const string Assisted_Explore_Success_Obtain_Item = "Assisted Explore Success - Obtain Item";
    private const string Assisted_Explore_Success_No_Item = "Assisted Explore Success - No Item";
    private const string Assisted_Explore_Fail = "Assisted Explore Fail";
    private const string Assisted_Explore_Critical_Fail = "Assisted Explore Critical Fail";
    private const string Normal_Explore_Success_Obtain_Item = "Normal Explore Success - Obtain Item";
    private const string Normal_Explore_Success_No_Item = "Normal Explore Success - No Item";
    private const string Normal_Explore_Fail = "Normal Explore Fail";
    private const string Normal_Explore_Critical_Fail = "Normal Explore Critical Fail";

    public ExploreEvent(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.EXPLORE_EVENT, 0) {
        _name = "Explore Event";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState trappedExploreSuccessObtainItem = new InteractionState(Trapped_Explore_Success_Obtain_Item, this);
        InteractionState trappedExploreSuccessNoItem = new InteractionState(Trapped_Explore_Success_No_Item, this);
        InteractionState trappedExploreFail = new InteractionState(Trapped_Explore_Fail, this);
        InteractionState trappedExploreCriticalFail = new InteractionState(Trapped_Explore_Critical_Fail, this);

        InteractionState assistedExploreSuccessObtainItem = new InteractionState(Assisted_Explore_Success_Obtain_Item, this);
        InteractionState assistedExploreSuccessNoItem = new InteractionState(Assisted_Explore_Success_No_Item, this);
        InteractionState assistedExploreFail = new InteractionState(Assisted_Explore_Fail, this);
        InteractionState assistedExploreCriticalFail = new InteractionState(Assisted_Explore_Critical_Fail, this);

        InteractionState normalExploreSuccessObtainItem = new InteractionState(Normal_Explore_Success_Obtain_Item, this);
        InteractionState normalExploreSuccessNoItem = new InteractionState(Normal_Explore_Success_No_Item, this);
        InteractionState normalExploreFail = new InteractionState(Normal_Explore_Fail, this);
        InteractionState normalExploreCriticalFail = new InteractionState(Normal_Explore_Critical_Fail, this);

        CreateActionOptions(startState);
        trappedExploreSuccessObtainItem.SetEffect(() => ExploreSuccessObtainItemRewardEffect(trappedExploreSuccessObtainItem));
        assistedExploreSuccessObtainItem.SetEffect(() => ExploreSuccessObtainItemRewardEffect(assistedExploreSuccessObtainItem));
        normalExploreSuccessObtainItem.SetEffect(() => ExploreSuccessObtainItemRewardEffect(normalExploreSuccessObtainItem));

        trappedExploreSuccessNoItem.SetEffect(() => ExploreSuccessNoItemRewardEffect(trappedExploreSuccessNoItem));
        assistedExploreSuccessNoItem.SetEffect(() => ExploreSuccessNoItemRewardEffect(assistedExploreSuccessNoItem));
        normalExploreSuccessNoItem.SetEffect(() => ExploreSuccessNoItemRewardEffect(normalExploreSuccessNoItem));

        trappedExploreFail.SetEffect(() => ExploreFailRewardEffect(trappedExploreFail));
        assistedExploreFail.SetEffect(() => ExploreFailRewardEffect(assistedExploreFail));
        normalExploreFail.SetEffect(() => ExploreFailRewardEffect(normalExploreFail));

        trappedExploreCriticalFail.SetEffect(() => ExploreCriticalFailRewardEffect(trappedExploreCriticalFail));
        assistedExploreCriticalFail.SetEffect(() => ExploreCriticalFailRewardEffect(assistedExploreCriticalFail));
        normalExploreCriticalFail.SetEffect(() => ExploreCriticalFailRewardEffect(normalExploreCriticalFail));

        _states.Add(startState.name, startState);
        _states.Add(trappedExploreSuccessObtainItem.name, trappedExploreSuccessObtainItem);
        _states.Add(trappedExploreSuccessNoItem.name, trappedExploreSuccessNoItem);
        _states.Add(trappedExploreFail.name, trappedExploreFail);
        _states.Add(trappedExploreCriticalFail.name, trappedExploreCriticalFail);

        _states.Add(assistedExploreSuccessObtainItem.name, assistedExploreSuccessObtainItem);
        _states.Add(assistedExploreSuccessNoItem.name, assistedExploreSuccessNoItem);
        _states.Add(assistedExploreFail.name, assistedExploreFail);
        _states.Add(assistedExploreCriticalFail.name, assistedExploreCriticalFail);

        _states.Add(normalExploreSuccessObtainItem.name, normalExploreSuccessObtainItem);
        _states.Add(normalExploreSuccessNoItem.name, normalExploreSuccessNoItem);
        _states.Add(normalExploreFail.name, normalExploreFail);
        _states.Add(normalExploreCriticalFail.name, normalExploreCriticalFail);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption traps = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Lay some traps.",
                duration = 0,
                effect = () => TrapsOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Must have instigator minion.",
            };
            ActionOption assist = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Assist with the exploration.",
                duration = 0,
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Must have diplomat minion.",
                effect = () => AssistOptionEffect(state),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(traps);
            state.AddActionOption(assist);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    #region Action Option Effects
    private void TrapsOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        WeightedDictionary<RESULT> minionInstigatorWeights = investigatorCharacter.job.GetJobRateWeights();
        switch (minionInstigatorWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                resultWeights.AddWeightToElement(RESULT.FAIL, 30);
                resultWeights.AddWeightToElement(RESULT.CRITICAL_FAIL, 20);
                break;
        }

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                if (interactable.tileLocation.areaOfTile.GetElligibleTokensForCharacter(_characterInvolved).Count > 0) {
                    nextState = Trapped_Explore_Success_Obtain_Item;
                } else {
                    nextState = Trapped_Explore_Success_No_Item;
                }
                break;
            case RESULT.FAIL:
                nextState = Trapped_Explore_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Trapped_Explore_Critical_Fail;
                break;
            default:
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void AssistOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        WeightedDictionary<RESULT> minionInstigatorWeights = investigatorCharacter.job.GetJobRateWeights();
        switch (minionInstigatorWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                resultWeights.AddWeightToElement(RESULT.FAIL, 30);
                resultWeights.AddWeightToElement(RESULT.CRITICAL_FAIL, 20);
                break;
        }

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                if (interactable.tileLocation.areaOfTile.GetElligibleTokensForCharacter(_characterInvolved).Count > 0) {
                    nextState = Assisted_Explore_Success_Obtain_Item;
                } else {
                    nextState = Assisted_Explore_Success_No_Item;
                }
                break;
            case RESULT.FAIL:
                nextState = Assisted_Explore_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Assisted_Explore_Critical_Fail;
                break;
            default:
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        //WeightedDictionary<RESULT> minionInstigatorWeights = investigatorMinion.character.job.GetJobRateWeights();
        //switch (minionInstigatorWeights.PickRandomElementGivenWeights()) {
        //    case RESULT.SUCCESS:
        //        resultWeights.AddWeightToElement(RESULT.FAIL, 30);
        //        resultWeights.AddWeightToElement(RESULT.CRITICAL_FAIL, 20);
        //        break;
        //}

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                if (interactable.tileLocation.areaOfTile.GetElligibleTokensForCharacter(_characterInvolved).Count > 0) {
                    nextState = Normal_Explore_Success_Obtain_Item;
                } else {
                    nextState = Normal_Explore_Success_No_Item;
                }
                break;
            case RESULT.FAIL:
                nextState = Normal_Explore_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Normal_Explore_Critical_Fail;
                break;
            default:
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    private void ExploreSuccessObtainItemRewardEffect(InteractionState state) {
        //**Mechanics**: Give an Item Token to the character, select from Location pool
        SpecialToken gainedToken = GiveSpecialTokenToCharacter();

        state.descriptionLog.AddToFillers(null, gainedToken.nameInBold, LOG_IDENTIFIER.STRING_1);
        state.AddLogFiller(new LogFiller(null, gainedToken.nameInBold, LOG_IDENTIFIER.STRING_1));
    }

    private void ExploreSuccessNoItemRewardEffect(InteractionState state) {
        
    }
    private void ExploreFailRewardEffect(InteractionState state) {
        if (investigatorCharacter != null) {
            investigatorCharacter.LevelUp();
        }
    }
    private void ExploreCriticalFailRewardEffect(InteractionState state) {
        if (interactable.tileLocation.areaOfTile.owner != null 
            && interactable.tileLocation.areaOfTile.owner.id != _characterInvolved.faction.id) {
            AdjustFactionsRelationship(interactable.tileLocation.areaOfTile.owner, _characterInvolved.faction, -1, state);
        }
        //**Mechanic**: Character dies.
        _characterInvolved.Death();
        if (investigatorCharacter != null) {
            investigatorCharacter.LevelUp();
        }
    }

    private SpecialToken GiveSpecialTokenToCharacter() {
        List<SpecialToken> choices = interactable.tileLocation.areaOfTile.GetElligibleTokensForCharacter(_characterInvolved);
        //WeightedDictionary<SpecialToken> tokenWeights = new WeightedDictionary<SpecialToken>();
        //for (int i = 0; i < choices.Count; i++) {
        //    tokenWeights.AddElement(choices[i], choices[i].weight);
        //}
        SpecialToken token = choices[Random.Range(0, choices.Count)];
        _characterInvolved.ObtainToken(token);
        interactable.tileLocation.areaOfTile.RemoveSpecialTokenFromLocation(token);
        Debug.LogWarning("[Day " + GameManager.Instance.continuousDays + "] " + _characterInvolved.name + " obtained " + token.tokenName + " at " + interactable.tileLocation.areaOfTile.name);
        return token;
    }
}
