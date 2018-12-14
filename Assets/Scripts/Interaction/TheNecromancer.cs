
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheNecromancer : Interaction {

    private BaseLandmark landmark;
    private Character chosenCharacter;

    private static string[] motivations = new string[] { "Pride", "Wrath", "Envy" };
    private Dictionary<string, string> motivationStrings = new Dictionary<string, string>() {
        { motivations[0], "He left home after losing all of his wealth and family due to a failed business venture." },
        { motivations[1], "He left home after some soldiers wrongfully accused him of criminal activities he didn't commit and took all of his belongings away." },
        { motivations[2], "He left home after the woman he courted became engaged with a close friend." },
    };

    private string chosenMotivation;

    private string Inflamed_Resentment = "Inflamed Resentment";
    private string Ritual_Success = "Ritual Success";
    private string Ritual_Critical_Success = "Ritual Critical Success";
    private string Mana_Drain_Success = "Mana Drain Success";
    private string Mana_Drain_Failed = "Mana Drain Failed";
    private string Necromancer_Supplied = "Necromancer Supplied";
    private string Necromancer_Given_Minion = "Necromancer Given Minion";
    private string Necromancer_Created = "Necromancer Created";
    private string Do_Nothing = "Do nothing";

    public TheNecromancer(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.CREATE_NECROMANCER, 250) {
        _name = "The Necromancer";
    }

    #region Overrides
    public override void CreateStates() {
        landmark = interactable;

        InteractionState startState = new InteractionState("Start", this);

        chosenMotivation = motivations[Random.Range(0, motivations.Length)];
        string motivationString = motivationStrings[chosenMotivation];

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(chosenCharacter, chosenCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        startStateDescriptionLog.AddToFillers(null, motivationString, LOG_IDENTIFIER.STRING_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);


        //action option states
        InteractionState inflamedResentmentState = new InteractionState(Inflamed_Resentment, this);
        InteractionState ritualSuccessState = new InteractionState(Ritual_Success, this);
        InteractionState ritualCriticalSuccessState = new InteractionState(Ritual_Critical_Success, this);
        InteractionState manaDrainSuccessState = new InteractionState(Mana_Drain_Success, this);
        InteractionState manaDrainFailedState = new InteractionState(Mana_Drain_Failed, this);
        InteractionState minionMisdirectedState = new InteractionState(Necromancer_Supplied, this);
        InteractionState necromancerGivenMinionState = new InteractionState(Necromancer_Given_Minion, this);
        InteractionState necromancerCreatedState = new InteractionState(Necromancer_Created, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        CreateActionOptions(startState);

        //inflamedResentmentState.SetEndEffect(() => SuccessfullyFoundOutLocationRewardEffect(inflamedResentmentState));
        //ritualSuccessState.SetEndEffect(() => MinionCaughtTailingRewardEffect(ritualSuccessState));
        //ritualCriticalSuccessState.SetEndEffect(() => LostTheCharacterRewardEffect(ritualCriticalSuccessState));
        //manaDrainSuccessState.SetEndEffect(() => SuccessfullyGotAheadOfCharacterRewardEffect(manaDrainSuccessState));
        //manaDrainFailedState.SetEndEffect(() => MinionMisdirectedRewardEffect(manaDrainFailedState));
        //doNothingState.SetEndEffect(() => DoNothingRewardEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(inflamedResentmentState.name, inflamedResentmentState);
        _states.Add(ritualSuccessState.name, ritualSuccessState);
        _states.Add(ritualCriticalSuccessState.name, ritualCriticalSuccessState);
        _states.Add(manaDrainSuccessState.name, manaDrainSuccessState);
        _states.Add(manaDrainFailedState.name, manaDrainFailedState);
        _states.Add(minionMisdirectedState.name, minionMisdirectedState);
        _states.Add(necromancerGivenMinionState.name, necromancerGivenMinionState);
        _states.Add(necromancerCreatedState.name, necromancerCreatedState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        string pronoun = "his";
        if (chosenCharacter.gender == GENDER.FEMALE) {
            pronoun = "her";
        }
        if (state.name == "Start") {
            ActionOption inflame = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Inflame " + pronoun + " resentment.",
                duration = 0,
                effect = () => InflameResentmentOptionEffect(state),
                canBeDoneAction = () => AssignedMinionIsOfClass(chosenMotivation),
                doesNotMeetRequirementsStr = "Minion must be " + chosenMotivation,
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(inflame);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        } else if (state.name == Inflamed_Resentment) {
            ActionOption preformRitual = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Perform the ritual.",
                duration = 0,
                effect = () => PerformRitualOptionEffect(state),
            };
            ActionOption drainResentment = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Drain " + pronoun + " resentment for Mana.",
                duration = 0,
                effect = () => DrainResentmentOptionEffect(state),
            };
            ActionOption leaveAlone = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Leave " + pronoun + " alone.",
                duration = 0,
                effect = () => LeaveAloneEffect(state),
            };
            state.AddActionOption(preformRitual);
            state.AddActionOption(drainResentment);
            state.AddActionOption(leaveAlone);
            state.SetDefaultOption(leaveAlone);
        } else if (state.name == Ritual_Success || state.name == Ritual_Critical_Success) {
            ActionOption provideSupplies = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Provide " + pronoun + " with some Supplies.",
                duration = 0,
                effect = () => PerformRitualOptionEffect(state),
            };
            ActionOption provideMinion = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Provide " + pronoun + " with your Minion.",
                duration = 0,
                effect = () => DrainResentmentOptionEffect(state),
            };
            ActionOption leaveAlone = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Leave " + pronoun + " alone.",
                duration = 0,
                effect = () => LeaveAloneEffect(state),
            };
            state.AddActionOption(provideSupplies);
            state.AddActionOption(provideMinion);
            state.AddActionOption(leaveAlone);
            state.SetDefaultOption(leaveAlone);
        }
    }
    //public override void OnInteractionActive() {
    //    base.OnInteractionActive();
    //    //If you dont have it yet, gain Intel of selected character (Check if minion is exploring)
    //    if (chosenCharacter is Character) {
    //        PlayerManager.Instance.player.AddIntel((chosenCharacter as Character).characterIntel);
    //    }
    //}
    #endregion

    #region Action Option Effects
    private void InflameResentmentOptionEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Inflamed_Resentment, 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        if (chosenEffect == Inflamed_Resentment) {
            InflamedResentmentEffect(state, chosenEffect);
        }
        //SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Do_Nothing]);
    }

    private void PerformRitualOptionEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Ritual_Success, 25);
        effectWeights.AddElement(Ritual_Critical_Success, 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DrainResentmentOptionEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Mana_Drain_Success, 25);
        effectWeights.AddElement(Mana_Drain_Failed, 5);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void LeaveHimAloneOptionEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Do_Nothing, 15);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }

    private void ProvideSuppliesOptionEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Necromancer_Supplied, 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void ProvideMinionOptionEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Necromancer_Given_Minion, 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void LeaveAloneOptionEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Necromancer_Created, 25);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    #endregion

    #region Result Effects
    private void RitualSuccessEffect(InteractionState state) {

    }
    private void InflamedResentmentEffect(InteractionState state, string effectName) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(chosenCharacter, chosenCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            state.descriptionLog.AddToFillers(null, chosenMotivation, LOG_IDENTIFIER.STRING_1);
        }
        SetCurrentState(_states[effectName]);
    }
    #endregion

    #region End Result Effects
    private void ManaDrainSuccessRewardEffect(InteractionState state) {
        //**Reward**: Mana Cache 1, Demon gains Exp 1
        investigatorMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
        PlayerManager.Instance.player.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Mana_Cache_Reward_1));
    }
    private void ManaDrainFailedRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        investigatorMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Level_Reward_1));
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //**Mechanics**: Characters home area will gain Supply Cache Reward 1
        
    }
    #endregion

}
