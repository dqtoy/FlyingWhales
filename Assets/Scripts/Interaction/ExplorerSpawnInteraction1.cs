using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorerSpawnInteraction1 : Interaction {

    private const string Start = "Start";
    private const string Special_Token_Obtained = "Special Token Obtained";
    private const string Current_Location_Token_Obtained = "Current Location Token Obtained";
    private const string Do_Nothing = "Do Nothing";

    private SpecialToken _chosenSpecialToken;

    public ExplorerSpawnInteraction1(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.EXPLORER_SPAWN_INTERACTION_1, 0) {
        _name = "Explorer Spawn Interaction 1";
        _jobFilter = new JOB[] { JOB.EXPLORER };
    }

    #region Overrides
    public override void CreateStates() {
        SetChosenSpecialToken();

        InteractionState startState = new InteractionState(Start, this);
        InteractionState specialTokenObtainedState = new InteractionState(Special_Token_Obtained, this);
        InteractionState locationTokenObtainedState = new InteractionState(Current_Location_Token_Obtained, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        CreateActionOptions(startState);
        startState.SetUseTokeneerMinionOnly(true);

        specialTokenObtainedState.SetEffect(() => SpecialTokenObtainedEffect(specialTokenObtainedState));
        locationTokenObtainedState.SetEffect(() => CurrentLocationTokenObtainedEffect(locationTokenObtainedState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(specialTokenObtainedState.name, specialTokenObtainedState);
        _states.Add(locationTokenObtainedState.name, locationTokenObtainedState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption specialTokenOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Get " + _chosenSpecialToken.ToString(),
                duration = 0,
                canBeDoneAction = () => CanGetSpecialToken(_chosenSpecialToken),
                effect = () => SpecialTokenOption(),
                doesNotMeetRequirementsStr = "You already have this token."
            };
            ActionOption locationTokenOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Get " + interactable.tileLocation.areaOfTile.locationToken.ToString(),
                duration = 0,
                canBeDoneAction = () => CanGetLocationToken(),
                effect = () => LocationTokenOption(),
                doesNotMeetRequirementsStr = "You already have this token."
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(specialTokenOption);
            state.AddActionOption(locationTokenOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private void SpecialTokenOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement(Special_Token_Obtained, 20);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Special_Token_Obtained]);
    }
    private void LocationTokenOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement(Current_Location_Token_Obtained, 20);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Current_Location_Token_Obtained]);
    }
    private void DoNothingOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement(Do_Nothing, 20);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Do_Nothing]);
    }
    private bool CanGetSpecialToken(SpecialToken token) {
        return PlayerManager.Instance.player.GetToken(token) == null;
    }
    private bool CanGetLocationToken() {
        return PlayerManager.Instance.player.GetToken(interactable.tileLocation.areaOfTile.locationToken) == null;
    }
    #endregion

    #region State Effects
    private void SpecialTokenObtainedEffect(InteractionState state) {
        state.SetUseTokeneerMinionOnly(true);

        PlayerManager.Instance.player.AddToken(_chosenSpecialToken);

        state.descriptionLog.AddToFillers(null, _chosenSpecialToken.ToString(), LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(null, _chosenSpecialToken.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void CurrentLocationTokenObtainedEffect(InteractionState state) {
        state.SetUseTokeneerMinionOnly(true);

        PlayerManager.Instance.player.AddToken(interactable.tileLocation.areaOfTile.locationToken);

        state.descriptionLog.AddToFillers(null, interactable.tileLocation.areaOfTile.locationToken.ToString(), LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(null, interactable.tileLocation.areaOfTile.locationToken.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void DoNothingEffect(InteractionState state) {
        state.SetUseTokeneerMinionOnly(true);
    }
    #endregion

    private void SetChosenSpecialToken() {
        if (interactable.tileLocation.areaOfTile.possibleSpecialTokenSpawns.Count == 0) {
            throw new System.Exception("No more special token spawns in " + interactable.tileLocation.areaOfTile.name);
        }
        _chosenSpecialToken = interactable.tileLocation.areaOfTile.possibleSpecialTokenSpawns[Random.Range(0, interactable.tileLocation.areaOfTile.possibleSpecialTokenSpawns.Count)];
    }
}
