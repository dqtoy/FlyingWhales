using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstigatorCharacterEncounter : Interaction {

    private const string Start = "Start";
    private const string Minion_Killed_Character = "Minion Killed Character";
    private const string Minion_Injured_Character = "Minion Injured Character";
    private const string Character_Killed_Minion = "Character Killed Minion";
    private const string Character_Injured_Minion = "Character Injured Minion";
    private const string Do_Nothing = "Do Nothing";

    public InstigatorCharacterEncounter(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.INSTIGATOR_CHARACTER_ENCOUNTER, 0) {
        _name = "Instigator Character Encounter";
        _jobFilter = new JOB[] { JOB.INSTIGATOR};
    }
    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState minionKilledCharacterState = new InteractionState(Minion_Killed_Character, this);
        InteractionState minionInjuredCharacterState = new InteractionState(Minion_Injured_Character, this);
        InteractionState characterKilledMinionState = new InteractionState(Character_Killed_Minion, this);
        InteractionState characterInjuredMinionState = new InteractionState(Character_Injured_Minion, this);
        InteractionState doNothingState = new InteractionState(Do_Nothing, this);

        CreateActionOptions(startState);

        minionKilledCharacterState.SetEffect(() => MinionKilledCharacterEffect(minionKilledCharacterState));
        minionInjuredCharacterState.SetEffect(() => MinionInjuredCharacterEffect(minionInjuredCharacterState));
        characterKilledMinionState.SetEffect(() => CharacterKilledMinionEffect(characterKilledMinionState));
        characterInjuredMinionState.SetEffect(() => CharacterInjuredMinionEffect(characterInjuredMinionState));
        doNothingState.SetEffect(() => DoNothingEffect(doNothingState));

        _states.Add(startState.name, startState);
        _states.Add(minionKilledCharacterState.name, minionKilledCharacterState);
        _states.Add(minionInjuredCharacterState.name, minionInjuredCharacterState);
        _states.Add(characterKilledMinionState.name, characterKilledMinionState);
        _states.Add(characterInjuredMinionState.name, characterInjuredMinionState);
        _states.Add(doNothingState.name, doNothingState);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption useTokenOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Use a token.",
                enabledTooltipText = "Check token details.",
                disabledTooltipText = "This token cannot be used here.",
                neededObjects = new List<System.Type>() { typeof(SpecialToken) },
                effect = () => UseTokenOption(state),
            };
            ActionOption assaultOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.MANA },
                name = "Assault " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) +  ".",
                enabledTooltipText = "May lead to death or injury...",
                effect = () => AssaultOption(),
            };
            ActionOption doNothingOption = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Leave " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " alone.",
                effect = () => DoNothingOption(),
            };

            state.AddActionOption(useTokenOption);
            state.AddActionOption(assaultOption);
            state.AddActionOption(doNothingOption);
            state.SetDefaultOption(doNothingOption);
        }
    }
    #endregion

    #region Action Options
    private void UseTokenOption(InteractionState state) {
        SpecialToken specialToken = state.assignedSpecialToken;
        specialToken.CreateJointInteractionStates(this);
    }
    private void AssaultOption() {
        int minionWeight = 0;
        int characterWeight = 0;
        CombatManager.Instance.GetCombatWeightsOfTwoLists(investigatorMinion.character.currentParty.characters, _characterInvolved.currentParty.characters, out minionWeight, out characterWeight);
        WeightedDictionary<string> effectWeights = new WeightedDictionary<string>();
        effectWeights.AddElement("Minion", minionWeight);
        effectWeights.AddElement("Character", characterWeight);
        string combatResult = effectWeights.PickRandomElementGivenWeights();
        effectWeights.Clear();
        if (combatResult == "Minion") {
            //Minion won
            effectWeights.AddElement(Minion_Killed_Character, 20);
            effectWeights.AddElement(Minion_Injured_Character, 40);

        } else {
            //Character won
            effectWeights.AddElement(Character_Killed_Minion, 20);
            effectWeights.AddElement(Character_Injured_Minion, 40);
        }
        string result = effectWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[result]);
    }
    private void DoNothingOption() {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region State Effects
    private void MinionKilledCharacterEffect(InteractionState state) {
        investigatorMinion.LevelUp();
        characterInvolved.Death();

        state.descriptionLog.AddToFillers(null, characterInvolved.characterClass.className, LOG_IDENTIFIER.STRING_1);
    }
    private void MinionInjuredCharacterEffect(InteractionState state) {
        investigatorMinion.LevelUp();
        Trait injuredTrait = AttributeManager.Instance.allTraits["Injured"];
        characterInvolved.AddTrait(injuredTrait);

        state.descriptionLog.AddToFillers(null, characterInvolved.characterClass.className, LOG_IDENTIFIER.STRING_1);

        state.AddLogFiller(new LogFiller(null, injuredTrait.name, LOG_IDENTIFIER.STRING_1));
    }
    private void CharacterKilledMinionEffect(InteractionState state) {
        characterInvolved.LevelUp();

        state.descriptionLog.AddToFillers(investigatorMinion, investigatorMinion.name, LOG_IDENTIFIER.MINION_1);

        state.AddLogFiller(new LogFiller(investigatorMinion, investigatorMinion.name, LOG_IDENTIFIER.MINION_1));

        DemonDisappearsRewardEffect(state);
    }
    private void CharacterInjuredMinionEffect(InteractionState state) {
        characterInvolved.LevelUp();
        Trait injuredTrait = AttributeManager.Instance.allTraits["Injured"];
        investigatorMinion.character.AddTrait(injuredTrait);

        state.AddLogFiller(new LogFiller(null, injuredTrait.name, LOG_IDENTIFIER.STRING_1));
    }
    private void DoNothingEffect(InteractionState state) {
    }
    #endregion
}
