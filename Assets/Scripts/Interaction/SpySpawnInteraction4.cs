using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpySpawnInteraction4 : Interaction {
    private const string Start = "Start";
    private const string Character_1_Token_Obtained = "Character 1 Token Obtained";
    private const string Character_1_Home_Location_Token_Obtained = "Character 1 Home Location Token Obtained";
    private const string Faction_Token_Obtained = "Character 1 Faction Token Obtained";
    private const string Do_Nothing = "Do Nothing";

    private Character _character1;

    public SpySpawnInteraction4(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.SPY_SPAWN_INTERACTION_4, 0) {
        _name = "Spy Spawn Interaction 4";
        _jobFilter = new JOB[] { JOB.SPY };
    }

    #region Overrides
    public override void CreateStates() {
        SetCharacter1();

        InteractionState startState = new InteractionState(Start, this);
        InteractionState character1TokenObtainedState = new InteractionState(Character_1_Token_Obtained, this);
        InteractionState homeTokenObtainedState = new InteractionState(Character_1_Home_Location_Token_Obtained, this);
        InteractionState factionTokenObtainedState = new InteractionState(Faction_Token_Obtained, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        CreateActionOptions(startState);
        startState.SetUseTokeneerMinionOnly(true);

        character1TokenObtainedState.SetEffect(() => CharacterToken1ObtainedEffect(character1TokenObtainedState));
        homeTokenObtainedState.SetEffect(() => HomeTokenObtainedEffect(homeTokenObtainedState));
        factionTokenObtainedState.SetEffect(() => FactionTokenObtainedEffect(factionTokenObtainedState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(character1TokenObtainedState.name, character1TokenObtainedState);
        _states.Add(homeTokenObtainedState.name, homeTokenObtainedState);
        _states.Add(factionTokenObtainedState.name, factionTokenObtainedState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption character1TokenOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Get " + _character1.characterToken.ToString(),
                duration = 0,
                canBeDoneAction = () => CanGetCharacterToken(_character1),
                effect = () => Character1TokenOption(),
                doesNotMeetRequirementsStr = "You already have this token."
            };
            ActionOption homeTokenOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Get " + _character1.homeLandmark.tileLocation.areaOfTile.locationToken.ToString(),
                duration = 0,
                canBeDoneAction = () => CanGetLocationToken(_character1.homeLandmark.tileLocation.areaOfTile),
                effect = () => Character1TokenOption(),
                doesNotMeetRequirementsStr = "You already have this token."
            };
            ActionOption factionTokenOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Get " + _character1.faction.factionToken.ToString(),
                duration = 0,
                canBeDoneAction = () => CanGetFactionToken(_character1.faction),
                effect = () => FactionTokenOption(),
                doesNotMeetRequirementsStr = "You already have this token."
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(character1TokenOption);
            state.AddActionOption(homeTokenOption);
            state.AddActionOption(factionTokenOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private void Character1TokenOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement(Character_1_Token_Obtained, 20);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Character_1_Token_Obtained]);
    }
    private void HomeTokenOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement(Character_1_Home_Location_Token_Obtained, 20);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Character_1_Home_Location_Token_Obtained]);
    }
    private void FactionTokenOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement(Faction_Owner_Token_Obtained, 20);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Faction_Token_Obtained]);
    }
    private void DoNothingOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement(Do_Nothing, 20);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Do_Nothing]);
    }
    private bool CanGetCharacterToken(Character character) {
        return PlayerManager.Instance.player.GetToken(character.characterToken) == null;
    }
    private bool CanGetFactionToken(Faction faction) {
        return PlayerManager.Instance.player.GetToken(faction.factionToken) == null;
    }
    private bool CanGetLocationToken(Area area) {
        return PlayerManager.Instance.player.GetToken(area.locationToken) == null;
    }
    #endregion

    #region State Effects
    private void CharacterToken1ObtainedEffect(InteractionState state) {
        state.SetUseTokeneerMinionOnly(true);

        PlayerManager.Instance.player.AddToken(_character1.characterToken);

        state.descriptionLog.AddToFillers(null, _character1.characterToken.ToString(), LOG_IDENTIFIER.STRING_1);

        //state.AddLogFiller(new LogFiller(null, _character1.characterToken.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void HomeTokenObtainedEffect(InteractionState state) {
        state.SetUseTokeneerMinionOnly(true);

        PlayerManager.Instance.player.AddToken(_character1.homeLandmark.tileLocation.areaOfTile.locationToken);

        state.descriptionLog.AddToFillers(null, _character1.homeLandmark.tileLocation.areaOfTile.locationToken.ToString(), LOG_IDENTIFIER.STRING_1);

        //state.AddLogFiller(new LogFiller(null, _character1.characterToken.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void FactionTokenObtainedEffect(InteractionState state) {
        state.SetUseTokeneerMinionOnly(true);

        PlayerManager.Instance.player.AddToken(_character1.faction.factionToken);

        state.descriptionLog.AddToFillers(null, _character1.faction.factionToken.ToString(), LOG_IDENTIFIER.STRING_1);

        //state.AddLogFiller(new LogFiller(null, interactable.tileLocation.areaOfTile.locationToken.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void DoNothingEffect(InteractionState state) {
        state.SetUseTokeneerMinionOnly(true);
    }
    #endregion

    private void SetCharacter1() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            Character character = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (character.faction.id != PlayerManager.Instance.player.playerFaction.id && character.faction.id != FactionManager.Instance.neutralFaction.id && character.homeLandmark.tileLocation.areaOfTile.id != interactable.tileLocation.areaOfTile.id) {
                characters.Add(character);
            }
        }
        _character1 = characters[UnityEngine.Random.Range(0, characters.Count)];
    }
}
