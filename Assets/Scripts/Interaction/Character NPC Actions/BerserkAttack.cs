using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkAttack : Interaction {
    private const string Start = "Start";
    private const string Berserker_Killed_Character = "Berserker Killed Character";
    private const string Berserker_Injured_Character = "Berserker Injured Character";
    private const string Berserker_Knocked_Out_Character = "Berserker Knocked Out Character";
    private const string Character_Injured_Berserker = "Character Injured Berserker";
    private const string Character_Knocked_Out_Berserker = "Character Knocked Out Berserker";
    private const string Berserk_Attack_Item = "Berserk Attack Item";
    private const string Berserk_No_Target = "Berserk No Target";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public BerserkAttack(Area interactable) : base(interactable, INTERACTION_TYPE.BERSERK_ATTACK, 0) {
        _name = "Berserk Attack";
    }

    #region Override
    public override void CreateStates() {

        InteractionState startState = new InteractionState(Start, this);
        InteractionState berserkerKilledCharacter = new InteractionState(Berserker_Killed_Character, this);
        InteractionState berserkerInjuredCharacter = new InteractionState(Berserker_Injured_Character, this);
        InteractionState berserkerKnockedOutCharacter = new InteractionState(Berserker_Knocked_Out_Character, this);
        InteractionState characterInjuredBerserker = new InteractionState(Character_Injured_Berserker, this);
        InteractionState characterKnockedOutBerserker = new InteractionState(Character_Knocked_Out_Berserker, this);
        InteractionState berserkAttackItem = new InteractionState(Berserk_Attack_Item, this);
        InteractionState berserkNoTarget = new InteractionState(Berserk_No_Target, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        berserkerKilledCharacter.SetEffect(() => BerserkerKilledCharacterEffect(berserkerKilledCharacter));
        berserkerInjuredCharacter.SetEffect(() => BerserkerInjuredCharacterEffect(berserkerInjuredCharacter));
        berserkerKnockedOutCharacter.SetEffect(() => BerserkerKnockedOutCharacterEffect(berserkerKnockedOutCharacter));
        characterInjuredBerserker.SetEffect(() => CharacterInjuredBerserkerEffect(characterInjuredBerserker));
        characterKnockedOutBerserker.SetEffect(() => CharacterKnockedOutBerserkerEffect(characterKnockedOutBerserker));
        berserkAttackItem.SetEffect(() => BerserkAttackItemEffect(berserkAttackItem));
        berserkNoTarget.SetEffect(() => BerserkNoTargetEffect(berserkNoTarget));

        _states.Add(startState.name, startState);
        _states.Add(berserkerKilledCharacter.name, berserkerKilledCharacter);
        _states.Add(berserkerInjuredCharacter.name, berserkerInjuredCharacter);
        _states.Add(berserkerKnockedOutCharacter.name, berserkerKnockedOutCharacter);
        _states.Add(characterInjuredBerserker.name, characterInjuredBerserker);
        _states.Add(characterKnockedOutBerserker.name, characterKnockedOutBerserker);
        _states.Add(berserkAttackItem.name, berserkAttackItem);
        _states.Add(berserkNoTarget.name, berserkNoTarget);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(),
            };
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect() {
        if(_characterInvolved.currentStructure.charactersHere.Count > 1) {
            List<Character> possibleTargets = new List<Character>();
            for (int i = 0; i < _characterInvolved.currentStructure.charactersHere.Count; i++) {
                if(_characterInvolved.currentStructure.charactersHere[i].id != _characterInvolved.id) {
                    possibleTargets.Add(_characterInvolved.currentStructure.charactersHere[i]);
                }
            }
            _targetCharacter = possibleTargets[UnityEngine.Random.Range(0, possibleTargets.Count)];

            List<Character> attackers = new List<Character>();
            attackers.Add(_characterInvolved);

            List<Character> defenders = new List<Character>();
            defenders.Add(_targetCharacter);

            float attackersChance = 0f;
            float defendersChance = 0f;

            CombatManager.Instance.GetCombatChanceOfTwoLists(attackers, defenders, out attackersChance, out defendersChance);

            WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < attackersChance) {
                //Berserker Win
                resultWeights.AddElement(Berserker_Killed_Character, 10);
                resultWeights.AddElement(Berserker_Injured_Character, 20);
                resultWeights.AddElement(Berserker_Knocked_Out_Character, 20);
            } else {
                //Target Win
                resultWeights.AddElement(Character_Injured_Berserker, 20);
                resultWeights.AddElement(Character_Knocked_Out_Berserker, 20);
            }

            string nextState = resultWeights.PickRandomElementGivenWeights();
            SetCurrentState(_states[nextState]);
        } else {
            if(_characterInvolved.currentStructure.itemsInStructure.Count > 0) {
                //No Characters but has Items
                SetCurrentState(_states[Berserk_Attack_Item]);
            } else {
                //No Characters and No Items
                SetCurrentState(_states[Berserk_No_Target]);
            }
        }
    }
    #endregion

    #region Reward Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToRandomStructureInArea();
    }
    private void BerserkerKilledCharacterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.Death();
    }
    private void BerserkerInjuredCharacterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.AddTrait("Injured");
    }
    private void BerserkerKnockedOutCharacterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.AddTrait("Unconscious");
    }
    private void CharacterInjuredBerserkerEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.AddTrait("Injured");
    }
    private void CharacterKnockedOutBerserkerEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.AddTrait("Unconscious");
    }
    private void BerserkAttackItemEffect(InteractionState state) {
        SpecialToken chosenToken = _characterInvolved.currentStructure.itemsInStructure[UnityEngine.Random.Range(0, _characterInvolved.currentStructure.itemsInStructure.Count)];
        _characterInvolved.specificLocation.RemoveSpecialTokenFromLocation(chosenToken);

        state.descriptionLog.AddToFillers(chosenToken, chosenToken.name, LOG_IDENTIFIER.ITEM_1);

        state.AddLogFiller(new LogFiller(chosenToken, chosenToken.name, LOG_IDENTIFIER.ITEM_1));
    }
    private void BerserkNoTargetEffect(InteractionState state) {
    }
    #endregion
}
