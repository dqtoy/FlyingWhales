using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpySpawnInteraction2 : Interaction {

    private const string Start = "Start";
    private const string Character_1_Token_Obtained = "Character 1 Token Obtained";
    private const string Character_2_Token_Obtained = "Character 2 Token Obtained";
    private const string Character_3_Token_Obtained = "Character 3 Token Obtained";
    private const string Do_Nothing = "Do Nothing";

    private Character _character1;
    private Character _character2;
    private Character _character3;

    public SpySpawnInteraction2(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.SPY_SPAWN_INTERACTION_2, 0) {
        _name = "Spy Spawn Interaction 2";
        _jobFilter = new JOB[] { JOB.SPY };
    }

    #region Overrides
    public override void CreateStates() {
        SetCharacters123();

        InteractionState startState = new InteractionState(Start, this);
        InteractionState character1TokenObtainedState = new InteractionState(Character_1_Token_Obtained, this);
        InteractionState character2TokenObtainedState = new InteractionState(Character_2_Token_Obtained, this);
        InteractionState character3TokenObtainedState = new InteractionState(Character_3_Token_Obtained, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        CreateActionOptions(startState);
        startState.SetUseTokeneerMinionOnly(true);

        character1TokenObtainedState.SetEffect(() => CharacterToken1ObtainedEffect(character1TokenObtainedState));
        character2TokenObtainedState.SetEffect(() => CharacterToken2ObtainedEffect(character2TokenObtainedState));
        character3TokenObtainedState.SetEffect(() => CharacterToken3ObtainedEffect(character3TokenObtainedState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(character1TokenObtainedState.name, character1TokenObtainedState);
        _states.Add(character2TokenObtainedState.name, character2TokenObtainedState);
        _states.Add(character3TokenObtainedState.name, character3TokenObtainedState);
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
            ActionOption character2TokenOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Get " + _character2.characterToken.ToString(),
                duration = 0,
                canBeDoneAction = () => CanGetCharacterToken(_character2),
                effect = () => Character2TokenOption(),
                doesNotMeetRequirementsStr = "You already have this token."
            };
            ActionOption character3TokenObtainedState = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Get " + _character3.characterToken.ToString(),
                duration = 0,
                canBeDoneAction = () => CanGetCharacterToken(_character3),
                effect = () => Character3TokenOption(),
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
            state.AddActionOption(character2TokenOption);
            state.AddActionOption(character3TokenObtainedState);
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
    private void Character2TokenOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement(Character_2_Token_Obtained, 20);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Character_2_Token_Obtained]);
    }
    private void Character3TokenOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement(Character_3_Token_Obtained, 20);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Character_3_Token_Obtained]);
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
    #endregion

    #region State Effects
    private void CharacterToken1ObtainedEffect(InteractionState state) {
        state.SetUseTokeneerMinionOnly(true);

        PlayerManager.Instance.player.AddToken(_character1.characterToken);

        state.descriptionLog.AddToFillers(null, _character1.characterToken.ToString(), LOG_IDENTIFIER.STRING_1);

        //state.AddLogFiller(new LogFiller(null, _character1.characterToken.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void CharacterToken2ObtainedEffect(InteractionState state) {
        state.SetUseTokeneerMinionOnly(true);

        PlayerManager.Instance.player.AddToken(_character2.characterToken);

        state.descriptionLog.AddToFillers(null, _character2.characterToken.ToString(), LOG_IDENTIFIER.STRING_1);

        //state.AddLogFiller(new LogFiller(null, _character2.characterToken.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void CharacterToken3ObtainedEffect(InteractionState state) {
        state.SetUseTokeneerMinionOnly(true);

        PlayerManager.Instance.player.AddToken(_character3.characterToken);

        state.descriptionLog.AddToFillers(null, _character3.characterToken.ToString(), LOG_IDENTIFIER.STRING_1);

        //state.AddLogFiller(new LogFiller(null, _character3.characterToken.ToString(), LOG_IDENTIFIER.STRING_1));
    }
    private void DoNothingEffect(InteractionState state) {
    }
    #endregion

    private void SetCharacters123() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            Character character = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (character.faction.id != PlayerManager.Instance.player.playerFaction.id) {
                characters.Add(character);
            }
        }
        int index1 = UnityEngine.Random.Range(0, characters.Count);
        _character1 = characters[index1];
        characters.RemoveAt(index1);
        int index2 = UnityEngine.Random.Range(0, characters.Count);
        _character2 = characters[index2];
        characters.RemoveAt(index2);
        _character3 = characters[UnityEngine.Random.Range(0, characters.Count)];

    }
}
