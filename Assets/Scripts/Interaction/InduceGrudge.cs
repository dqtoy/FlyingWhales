using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InduceGrudge : Interaction {
    private const string Start = "Start";
    private const string Induce_Grudge_Successful = "Induce Grudge Successful";
    private const string Induce_Grudge_Fail = "Induce Grudge Fail";
    private const string Induce_Grudge_Critical_Fail = "Induce Grudge Critical Fail";
    private const string Do_Nothing = "Do Nothing";

    public InduceGrudge(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.INDUCE_GRUDGE, 0) {
        _name = "Induce Grudge";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState induceGrudgeSuccessState = new InteractionState(Induce_Grudge_Successful, this);
        InteractionState induceGrudgeFailState = new InteractionState(Induce_Grudge_Fail, this);
        InteractionState induceGrudgeCritFailtate = new InteractionState(Induce_Grudge_Critical_Fail, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        CreateActionOptions(startState);

        induceGrudgeSuccessState.SetEffect(() => InduceGrudgeSuccessEffect(induceGrudgeSuccessState));
        induceGrudgeFailState.SetEffect(() => InduceGrudgeFailEffect(induceGrudgeFailState));
        induceGrudgeCritFailtate.SetEffect(() => InduceGrudgeCritFailEffect(induceGrudgeCritFailtate));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(induceGrudgeSuccessState.name, induceGrudgeSuccessState);
        _states.Add(induceGrudgeFailState.name, induceGrudgeFailState);
        _states.Add(induceGrudgeCritFailtate.name, induceGrudgeCritFailtate);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption induceOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Do it.",
                duration = 0,
                effect = () => InduceOption(),
                neededObjectsChecker = new List<ActionOptionNeededObjectChecker>() {
                    new ActionOptionTraitRequirement {
                        categoryReq = TRAIT_REQUIREMENT.RACE,
                        requirements = new string[] { RACE.HUMANS.ToString(), RACE.ELVES.ToString(), RACE.GOBLIN.ToString() }
                    }
                },
                neededObjects = new List<System.Type>() { typeof(CharacterToken) }
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "That sounds too risky.",
                duration = 0,
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(induceOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private void InduceOption() {
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement(Induce_Grudge_Successful, investigatorMinion.character.job.GetSuccessRate());
        effectWeights.AddElement(Induce_Grudge_Fail, investigatorMinion.character.job.GetFailRate());
        effectWeights.AddElement(Induce_Grudge_Critical_Fail, investigatorMinion.character.job.GetCritFailRate());

        string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[chosenEffect]);
    }
    private void DoNothingOption() {
        //WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        //effectWeights.AddElement(Normal_Faction_Upgrade, 25);

        //string chosenEffect = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region State Effects
    private void InduceGrudgeSuccessEffect(InteractionState state) {
        Character targetCharacter = _previousState.assignedCharacter.character;
        Grudge newGrudge = new Grudge(targetCharacter);
        _characterInvolved.AddTrait(newGrudge);
        investigatorMinion.LevelUp();

        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void InduceGrudgeFailEffect(InteractionState state) {
        Character targetCharacter = _previousState.assignedCharacter.character;
        _characterInvolved.faction.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, -2);

        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void InduceGrudgeCritFailEffect(InteractionState state) {
        Character targetCharacter = _previousState.assignedCharacter.character;
        _characterInvolved.faction.AdjustRelationshipFor(PlayerManager.Instance.player.playerFaction, -2);

        state.descriptionLog.AddToFillers(investigatorMinion.character, investigatorMinion.character.name, LOG_IDENTIFIER.MINION_1);
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(investigatorMinion.character, investigatorMinion.character.name, LOG_IDENTIFIER.MINION_1));

        DemonDisappearsRewardEffect(state);
    }
    private void DoNothingEffect(InteractionState state) {
    }
    #endregion
}
