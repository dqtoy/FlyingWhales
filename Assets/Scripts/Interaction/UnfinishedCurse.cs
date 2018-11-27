using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnfinishedCurse : Interaction {

    private const string curseCompleted = "Curse Completed";
    private const string curseFailedToComplete = "Curse Failed To Complete";
    private const string curseBackfires = "Curse Backfires";
    private const string obtainMana = "Obtain Mana";
    private const string doNothing = "Do nothing";


    private WeightedDictionary<string> curseWeights;

    public UnfinishedCurse(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.UNFINISHED_CURSE, 100) {
        _name = "Unfinished Curse";
    }

    #region Overrides
    public override void CreateStates() {
        ConstructCurseWeights();
        InteractionState startState = new InteractionState("Start", this);
        //string startStateDesc = "Our imp has reported what appears to be an ancient unfinished curse placed within one of the cemetery mausoleums. We may be able to complete the curse but we aren't aware of what it's actual effect would be, if any.";
        //startState.SetDescription(startStateDesc);
        CreateActionOptions(startState);
        //GameDate dueDate = GameManager.Instance.Today();
        //dueDate.AddHours(100);
        //startState.SetTimeSchedule(startState.actionOptions[2], dueDate); //default is do nothing

        //action option states
        InteractionState curseCompletedState = new InteractionState(curseCompleted, this);
        InteractionState curseFailedToCompleteState = new InteractionState(curseFailedToComplete, this);
        InteractionState curseBackfiresState = new InteractionState(curseBackfires, this);
        InteractionState obtainManaState = new InteractionState(obtainMana, this);
        InteractionState doNothingState = new InteractionState(doNothing, this);
            
        curseCompletedState.SetEffect(() => CurseCompletedRewardEffect(curseCompletedState));
        curseFailedToCompleteState.SetEffect(() => CurseFailedToCompleteRewardEffect(curseFailedToCompleteState));
        curseBackfiresState.SetEffect(() => CurseBackfiresRewardEffect(curseBackfiresState));
        obtainManaState.SetEffect(() => ObtainManaRewardEffect(obtainManaState));
        doNothingState.SetEffect(() => DoNothingRewardEffect(doNothingState));
            

        _states.Add(startState.name, startState);
        _states.Add(curseCompletedState.name, curseCompletedState);
        _states.Add(curseBackfiresState.name, curseBackfiresState);
        _states.Add(doNothingState.name, doNothingState);
        _states.Add(curseFailedToCompleteState.name, curseFailedToCompleteState);
        _states.Add(obtainManaState.name, obtainManaState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption completeCurse = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 5, currency = CURRENCY.MANA },
                name = "Attempt to complete the ritual.",
                duration = 0,
                needsMinion = false,
                neededObjects = new List<System.Type>() { typeof(CharacterIntel) },
                effect = () => CompleteCurseEffect(state),
            };
            ActionOption harnessMagic = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 20, currency = CURRENCY.SUPPLY },
                name = "Harness its magic into Mana.",
                duration = 0,
                needsMinion = false,
                effect = () => HarnessMagicEffect(state),
                canBeDoneAction = () => AssignedMinionIsOfClass("Gluttony"),
                doesNotMeetRequirementsStr = "Minion must be Gluttony.",
            };
            ActionOption leaveAlone = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Leave it alone.",
                duration = 0,
                needsMinion = false,
                effect = () => LeaveItAloneEffect(state),
            };

            state.AddActionOption(completeCurse);
            state.AddActionOption(harnessMagic);
            state.AddActionOption(leaveAlone);
        }
    }
    #endregion

    #region Action Option Effects
    private void CompleteCurseEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(curseCompleted, 30);
        effectWeights.AddElement(curseFailedToComplete, 10);
        effectWeights.AddElement(curseBackfires, 5);


        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
        //if (chosenEffect == curse1Completed) {
        //    Curse1Completed(state, chosenEffect);
        //} else if (chosenEffect == curse2Completed) {
        //    Curse2Completed(state, chosenEffect);
        //} else if (chosenEffect == curse3Completed) {
        //    Curse3Completed(state, chosenEffect);
        //} else if (chosenEffect == curseFailedToComplete) {
        //    CurseFailedToComplete(state, chosenEffect);
        //}
    }
    private void HarnessMagicEffect(InteractionState state) {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(obtainMana, 10);

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
        //if (chosenEffect == obtainMana) {
        //    CurseCompleted(state, chosenEffect);
        //}
    }
    private void LeaveItAloneEffect(InteractionState state) {
        SetCurrentState(_states[doNothing]);
    }
    #endregion

    private void CurseCompletedRewardEffect(InteractionState state) {
        this.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
        //**Effect**: Character should gain a random curse from the Curse checklist below
        string chosenCurse = curseWeights.PickRandomElementGivenWeights();
        Trait chosenAttribute = AttributeManager.Instance.allTraits[chosenCurse];
        state.assignedCharacter.character.AddTrait(chosenAttribute);
        state.AddLogFiller(new LogFiller(state.assignedCharacter.character, state.assignedCharacter.character.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(null, chosenAttribute.name, LOG_IDENTIFIER.STRING_1));
    }
    private void CurseFailedToCompleteRewardEffect(InteractionState state) {
        //**Reward**: Demon gains Exp 1
        this.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        //state.AddLogFiller(new LogFiller(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
    }
    private void CurseBackfiresRewardEffect(InteractionState state) {
        this.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1)); //**Reward**: Demon gains Exp 1
        //**Effect**: Demon Minion should gain a random curse from the Curse checklist below
        string chosenCurse = curseWeights.PickRandomElementGivenWeights();
        state.assignedMinion.icharacter.AddTrait(AttributeManager.Instance.allTraits[chosenCurse]);
        //state.AddLogFiller(new LogFiller(characterInvolved, characterInvolved.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
    }
    private void ObtainManaRewardEffect(InteractionState state) {
        //**Reward**: Mana Cache 1, Demon gains Exp 1
        this.explorerMinion.ClaimReward(InteractionManager.Instance.GetReward(InteractionManager.Exp_Reward_1));
        Reward reward = InteractionManager.Instance.GetReward(InteractionManager.Mana_Cache_Reward_1);
        PlayerManager.Instance.player.ClaimReward(reward);
    }
    private void DoNothingRewardEffect(InteractionState state) {
        
    }

    private void ConstructCurseWeights() {
        curseWeights = new WeightedDictionary<string>();
        curseWeights.AddElement("Placeholder Curse 1", 20);
        curseWeights.AddElement("Placeholder Curse 2", 30);
        curseWeights.AddElement("Placeholder Curse 3", 5);
    }
}
