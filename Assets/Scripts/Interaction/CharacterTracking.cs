
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterTracking : Interaction {

    private Area targetArea;

    private const string Successfully_Found_Out_Location = "Successfully Found Out Location";
    private const string Minion_Caught_Tailing_Character = "Minion Caught Tailing Character";
    private const string Lost_The_Character = "Lost The Character";
    private const string Successfully_Got_Ahead_Of_Character = "Successfully Got Ahead Of Character";
    private const string Minion_Misdirected = "Minion Misdirected";
    private const string Do_Nothing = "Do nothing";


    public CharacterTracking(Area interactable) : base(interactable, INTERACTION_TYPE.CHARACTER_TRACKING, 70) {
        _name = "Character Tracking";
    }

    #region Overrides
    public override void CreateStates() {
        //Select a random area other than the character's home
        targetArea = GetTargetArea();

        InteractionState startState = new InteractionState("Start", this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(characterInvolved.race), LOG_IDENTIFIER.STRING_1);
        startStateDescriptionLog.AddToFillers(null, Utilities.NormalizeString(characterInvolved.role.roleType.ToString()), LOG_IDENTIFIER.STRING_2);
        //startStateDescriptionLog.AddToFillers(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);


        //action option states
        InteractionState successfullyFoundOutLocationState = new InteractionState(Successfully_Found_Out_Location, this);
        InteractionState minionCaughtTailingCharacterState = new InteractionState(Minion_Caught_Tailing_Character, this);
        InteractionState lostTheCharacterState = new InteractionState(Lost_The_Character, this);
        InteractionState successfullyGotAheadOfCharacterState = new InteractionState(Successfully_Got_Ahead_Of_Character, this);
        InteractionState minionMisdirectedState = new InteractionState(Minion_Misdirected, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        CreateActionOptions(startState);

        successfullyFoundOutLocationState.SetEffect(() => SuccessfullyFoundOutLocationRewardEffect(successfullyFoundOutLocationState));
        minionCaughtTailingCharacterState.SetEffect(() => MinionCaughtTailingRewardEffect(minionCaughtTailingCharacterState));
        lostTheCharacterState.SetEffect(() => LostTheCharacterRewardEffect(lostTheCharacterState));
        successfullyGotAheadOfCharacterState.SetEffect(() => SuccessfullyGotAheadOfCharacterRewardEffect(successfullyGotAheadOfCharacterState));
        minionMisdirectedState.SetEffect(() => MinionMisdirectedRewardEffect(minionMisdirectedState));
        doNothingState.SetEffect(() => DoNothingRewardEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(successfullyFoundOutLocationState.name, successfullyFoundOutLocationState);
        _states.Add(minionCaughtTailingCharacterState.name, minionCaughtTailingCharacterState);
        _states.Add(lostTheCharacterState.name, lostTheCharacterState);
        _states.Add(successfullyGotAheadOfCharacterState.name, successfullyGotAheadOfCharacterState);
        _states.Add(minionMisdirectedState.name, minionMisdirectedState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        string subjectivePronoun = "him";
        if (characterInvolved.gender == GENDER.FEMALE) {
            subjectivePronoun = "her";
        }
        if (state.name == "Start") {
            ActionOption follow = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Follow " + subjectivePronoun + " closely",
                duration = 0,
                effect = () => FollowThemCloselyEffect(state),
            };
            ActionOption getAhead = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Attempt to get ahead of " + characterInvolved.name + ".",
                duration = 0,
                effect = () => GetAheadEffect(state),
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(follow);
            state.AddActionOption(getAhead);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override void OnInteractionActive() {
        base.OnInteractionActive();
        //If you dont have it yet, gain Intel of selected character (Check if minion is exploring)
        //PlayerManager.Instance.player.AddIntel(characterInvolved.characterIntel);
    }
    #endregion

    #region Action Option Effects
    private void FollowThemCloselyEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Successfully_Found_Out_Location, 25);
        effectWeights.AddElement(Lost_The_Character, 10);
        effectWeights.AddElement(Minion_Caught_Tailing_Character, 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void GetAheadEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Successfully_Got_Ahead_Of_Character, 25);
        effectWeights.AddElement(Minion_Misdirected, 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region End Result Effects
    private void SuccessfullyFoundOutLocationRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        investigatorCharacter.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
        //**Mechanics**: Player get's the location intel that the character has chosen.
        PlayerManager.Instance.player.AddToken(targetArea.locationToken);

        if (state.descriptionLog != null) {
            //state.descriptionLog.AddToFillers(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        //state.AddLogFiller(new LogFiller(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    private void MinionCaughtTailingRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        investigatorCharacter.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
        //TODO: **Mechanics**: Start combat between Character and Demon
        //if (state.descriptionLog != null) {
        //    state.descriptionLog.AddToFillers(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //}
        //state.AddLogFiller(new LogFiller(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
    }
    private void LostTheCharacterRewardEffect(InteractionState state) {
        //if (state.descriptionLog != null) {
        //    state.descriptionLog.AddToFillers(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //}
        //state.AddLogFiller(new LogFiller(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
    }
    private void SuccessfullyGotAheadOfCharacterRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        investigatorCharacter.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
        //**Mechanics**: Player gains Mana Cache Reward 1 and Supply Cache Reward 1
        PlayerManager.Instance.player.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Mana_Cache_Reward_1));
        PlayerManager.Instance.player.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
        if (state.descriptionLog != null) {
            //state.descriptionLog.AddToFillers(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        //state.AddLogFiller(new LogFiller(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    private void MinionMisdirectedRewardEffect(InteractionState state) {
        //**Mechanics**: Remove minion from player
        PlayerManager.Instance.player.RemoveMinion(investigatorCharacter.minion);
        //if (state.descriptionLog != null) {
        //    state.descriptionLog.AddToFillers(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //}
        //state.AddLogFiller(new LogFiller(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //**Mechanics**: Characters home area will gain Supply Cache Reward 1
        //if (characterInvolved.homeLandmark != null && characterInvolved.homeLandmark.tileLocation.areaOfTile != null) {
        //    characterInvolved.homeLandmark.tileLocation.areaOfTile.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Supply_Cache_Reward_1));
        //}
        if (state.descriptionLog != null) {
            //state.descriptionLog.AddToFillers(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        state.AddLogFiller(new LogFiller(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    #endregion

    private Area GetTargetArea() {
        List<Area> choices = new List<Area>(LandmarkManager.Instance.allAreas);
        //if (characterInvolved.homeLandmark != null && characterInvolved.homeLandmark.tileLocation.areaOfTile != null) {
        //    choices.Remove(characterInvolved.homeLandmark.tileLocation.areaOfTile);
        //}
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }
}
