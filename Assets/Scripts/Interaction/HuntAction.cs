using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntAction : Interaction {
    private Character targetCharacter;

    private const string Start = "Start";
    private const string Assisted_Hunter_Killed_Character = "Assisted Hunter Killed Character";
    private const string Assisted_Hunter_Injured_Character = "Assisted Hunter Injured Character";
    private const string Assisted_Character_Killed_Hunter = "Assisted Character Killed Hunter";
    private const string Assisted_Character_Injured_Hunter = "Assisted Character Injured Hunter";

    private const string Thwarted_Hunter_Killed_Character = "Thwarted Hunter Killed Character";
    private const string Thwarted_Hunter_Injured_Character = "Thwarted Hunter Injured Character";
    private const string Thwarted_Character_Killed_Hunter = "Thwarted Character Killed Hunter";
    private const string Thwarted_Character_Injured_Hunter = "Thwarted Character Injured Hunter";

    private const string Redirected_Hunter_Killed_Character = "Redirected Hunter Killed Character";
    private const string Redirected_Hunter_Injured_Character = "Redirected Hunter Injured Character";
    private const string Redirected_Character_Killed_Hunter = "Redirected Character Killed Hunter";
    private const string Redirected_Character_Injured_Hunter = "Redirected Character Injured Hunter";

    private const string Default_Hunter_Killed_Character = "Default Hunter Killed Character";
    private const string Default_Hunter_Injured_Character = "Default Hunter Injured Character";
    private const string Default_Character_Killed_Hunter = "Default Character Killed Hunter";
    private const string Default_Character_Injured_Hunter = "Default Character Injured Hunter";

    public HuntAction(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.HUNT_ACTION, 0) {
        _name = "Hunt Action";
    }

    #region Override
    public override void CreateStates() {
        if (targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState assistedHunterKilledCharacter = new InteractionState(Assisted_Hunter_Killed_Character, this);
        InteractionState assistedHunterInjuredCharacter = new InteractionState(Assisted_Hunter_Injured_Character, this);
        InteractionState assistedCharacterKilledHunter = new InteractionState(Assisted_Character_Killed_Hunter, this);
        InteractionState assistedCharacterInjuredHunter = new InteractionState(Assisted_Character_Injured_Hunter, this);

        InteractionState thwartedHunterKilledCharacter = new InteractionState(Thwarted_Hunter_Killed_Character, this);
        InteractionState thwartedHunterInjuredCharacter = new InteractionState(Thwarted_Hunter_Injured_Character, this);
        InteractionState thwartedCharacterKilledHunter = new InteractionState(Thwarted_Character_Killed_Hunter, this);
        InteractionState thwartedCharacterInjuredHunter = new InteractionState(Thwarted_Character_Injured_Hunter, this);

        InteractionState redirectedHunterKilledCharacter = new InteractionState(Redirected_Hunter_Killed_Character, this);
        InteractionState redirectedHunterInjuredCharacter = new InteractionState(Redirected_Hunter_Injured_Character, this);
        InteractionState redirectedCharacterKilledHunter = new InteractionState(Redirected_Character_Killed_Hunter, this);
        InteractionState redirectedCharacterInjuredHunter = new InteractionState(Redirected_Character_Injured_Hunter, this);

        InteractionState defaultHunterKilledCharacter = new InteractionState(Default_Hunter_Killed_Character, this);
        InteractionState defaultHunterInjuredCharacter = new InteractionState(Default_Hunter_Injured_Character, this);
        InteractionState defaultCharacterKilledHunter = new InteractionState(Default_Character_Killed_Hunter, this);
        InteractionState defaultCharacterInjuredHunter = new InteractionState(Default_Character_Injured_Hunter, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        assistedHunterKilledCharacter.SetEffect(() => AssistedHunterKilledCharacterEffect(assistedHunterKilledCharacter));
        assistedHunterInjuredCharacter.SetEffect(() => AssistedHunterInjuredCharacterEffect(assistedHunterInjuredCharacter));
        assistedCharacterKilledHunter.SetEffect(() => AssistedCharacterKilledHunterEffect(assistedCharacterKilledHunter));
        assistedCharacterInjuredHunter.SetEffect(() => AssistedCharacterInjuredHunterEffect(assistedCharacterInjuredHunter));

        thwartedHunterKilledCharacter.SetEffect(() => ThwartedHunterKilledCharacterEffect(thwartedHunterKilledCharacter));
        thwartedHunterInjuredCharacter.SetEffect(() => ThwartedHunterInjuredCharacterEffect(thwartedHunterInjuredCharacter));
        thwartedCharacterKilledHunter.SetEffect(() => ThwartedCharacterKilledHunterEffect(thwartedCharacterKilledHunter));
        thwartedCharacterInjuredHunter.SetEffect(() => ThwartedCharacterInjuredHunterEffect(thwartedCharacterInjuredHunter));

        redirectedHunterKilledCharacter.SetEffect(() => RedirectedHunterKilledCharacterEffect(redirectedHunterKilledCharacter));
        redirectedHunterInjuredCharacter.SetEffect(() => RedirectedHunterInjuredCharacterEffect(redirectedHunterInjuredCharacter));
        redirectedCharacterKilledHunter.SetEffect(() => RedirectedCharacterKilledHunterEffect(redirectedCharacterKilledHunter));
        redirectedCharacterInjuredHunter.SetEffect(() => RedirectedCharacterInjuredHunterEffect(redirectedCharacterInjuredHunter));

        defaultHunterKilledCharacter.SetEffect(() => DefaultHunterKilledCharacterEffect(defaultHunterKilledCharacter));
        defaultHunterInjuredCharacter.SetEffect(() => DefaultHunterInjuredCharacterEffect(defaultHunterInjuredCharacter));
        defaultCharacterKilledHunter.SetEffect(() => DefaultCharacterKilledHunterEffect(defaultCharacterKilledHunter));
        defaultCharacterInjuredHunter.SetEffect(() => DefaultCharacterInjuredHunterEffect(defaultCharacterInjuredHunter));

        _states.Add(startState.name, startState);
        _states.Add(assistedHunterKilledCharacter.name, assistedHunterKilledCharacter);
        _states.Add(assistedHunterInjuredCharacter.name, assistedHunterInjuredCharacter);
        _states.Add(assistedCharacterKilledHunter.name, assistedCharacterKilledHunter);
        _states.Add(assistedCharacterInjuredHunter.name, assistedCharacterInjuredHunter);

        _states.Add(thwartedHunterKilledCharacter.name, thwartedHunterKilledCharacter);
        _states.Add(thwartedHunterInjuredCharacter.name, thwartedHunterInjuredCharacter);
        _states.Add(thwartedCharacterKilledHunter.name, thwartedCharacterKilledHunter);
        _states.Add(thwartedCharacterInjuredHunter.name, thwartedCharacterInjuredHunter);

        _states.Add(redirectedHunterKilledCharacter.name, redirectedHunterKilledCharacter);
        _states.Add(redirectedHunterInjuredCharacter.name, redirectedHunterInjuredCharacter);
        _states.Add(redirectedCharacterKilledHunter.name, redirectedCharacterKilledHunter);
        _states.Add(redirectedCharacterInjuredHunter.name, redirectedCharacterInjuredHunter);

        _states.Add(defaultHunterKilledCharacter.name, defaultHunterKilledCharacter);
        _states.Add(defaultHunterInjuredCharacter.name, defaultHunterInjuredCharacter);
        _states.Add(defaultCharacterKilledHunter.name, defaultCharacterKilledHunter);
        _states.Add(defaultCharacterInjuredHunter.name, defaultCharacterInjuredHunter);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption assist = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Assist with the hunt.",
                effect = () => AssistOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                disabledTooltipText = "Minion must be an Instigator",
            };
            ActionOption thwart = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Thwart the hunt.",
                effect = () => ThwartOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                disabledTooltipText = "Minion must be a Diplomat",
            };
            ActionOption lead = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Lead to a different target.",
                effect = () => LeadOptionEffect(state),
                neededObjects = new List<System.Type>() { typeof(Character) },
                neededObjectsChecker = new List<ActionOptionNeededObjectChecker>() {
                    new ActionOptionNeededObjectChecker(){ isMatchFunction = IsCharacterValidRedirectionTarget }
                }
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(assist);
            state.AddActionOption(thwart);
            state.AddActionOption(lead);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        SetTargetCharacter(GetTargetCharacter(character));
        if (targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void AssistOptionEffect(InteractionState state) {
        List<Character> attackers = new List<Character>();
        attackers.Add(_characterInvolved);
        attackers.Add(investigatorMinion.character);

        List<Character> defenders = new List<Character>();
        defenders.Add(targetCharacter);

        float attackersChance = 0f;
        float defendersChance = 0f;

        CombatManager.Instance.GetCombatChanceOfTwoLists(attackers, defenders, out attackersChance, out defendersChance);

        WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < attackersChance) {
            //Hunter Win
            resultWeights.AddElement(Assisted_Hunter_Killed_Character, 20);
            resultWeights.AddElement(Assisted_Hunter_Injured_Character, 40);
        } else {
            //Target Win
            resultWeights.AddElement(Assisted_Character_Killed_Hunter, 20);
            resultWeights.AddElement(Assisted_Character_Injured_Hunter, 40);
        }

        string nextState = resultWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[nextState]);
    }
    private void ThwartOptionEffect(InteractionState state) {
        List<Character> attackers = new List<Character>();
        attackers.Add(_characterInvolved);

        List<Character> defenders = new List<Character>();
        defenders.Add(targetCharacter);
        defenders.Add(investigatorMinion.character);

        float attackersChance = 0f;
        float defendersChance = 0f;

        CombatManager.Instance.GetCombatChanceOfTwoLists(attackers, defenders, out attackersChance, out defendersChance);

        WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < attackersChance) {
            //Hunter Win
            resultWeights.AddElement(Thwarted_Hunter_Killed_Character, 20);
            resultWeights.AddElement(Thwarted_Hunter_Injured_Character, 40);
        } else {
            //Target Win
            resultWeights.AddElement(Thwarted_Character_Killed_Hunter, 20);
            resultWeights.AddElement(Thwarted_Character_Injured_Hunter, 40);
        }

        string nextState = resultWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[nextState]);
    }
    private void LeadOptionEffect(InteractionState state) {
        List<Character> attackers = new List<Character>();
        attackers.Add(_characterInvolved);

        List<Character> defenders = new List<Character>();
        defenders.Add(state.assignedCharacter.character);

        float attackersChance = 0f;
        float defendersChance = 0f;

        CombatManager.Instance.GetCombatChanceOfTwoLists(attackers, defenders, out attackersChance, out defendersChance);

        WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < attackersChance) {
            //Hunter Win
            resultWeights.AddElement(Redirected_Hunter_Killed_Character, 20);
            resultWeights.AddElement(Redirected_Hunter_Injured_Character, 40);
        } else {
            //Target Win
            resultWeights.AddElement(Redirected_Character_Killed_Hunter, 20);
            resultWeights.AddElement(Redirected_Character_Injured_Hunter, 40);
        }

        string nextState = resultWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        List<Character> attackers = new List<Character>();
        attackers.Add(_characterInvolved);

        List<Character> defenders = new List<Character>();
        defenders.Add(targetCharacter);

        float attackersChance = 0f;
        float defendersChance = 0f;

        CombatManager.Instance.GetCombatChanceOfTwoLists(attackers, defenders, out attackersChance, out defendersChance);

        WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < attackersChance) {
            //Hunter Win
            resultWeights.AddElement(Default_Hunter_Killed_Character, 20);
            resultWeights.AddElement(Default_Hunter_Injured_Character, 40);
        } else {
            //Target Win
            resultWeights.AddElement(Default_Character_Killed_Hunter, 20);
            resultWeights.AddElement(Default_Character_Injured_Hunter, 40);
        }

        string nextState = resultWeights.PickRandomElementGivenWeights();
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void AssistedHunterKilledCharacterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.LevelUp();
        investigatorMinion.LevelUp();

        if(targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        targetCharacter.Death();
    }
    private void AssistedHunterInjuredCharacterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.LevelUp();
        investigatorMinion.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void AssistedCharacterKilledHunterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        _characterInvolved.Death();
    }
    private void AssistedCharacterInjuredHunterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void ThwartedHunterKilledCharacterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        targetCharacter.Death();
    }
    private void ThwartedHunterInjuredCharacterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void ThwartedCharacterKilledHunterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.LevelUp();
        investigatorMinion.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        _characterInvolved.Death();
    }
    private void ThwartedCharacterInjuredHunterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.LevelUp();
        investigatorMinion.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void RedirectedHunterKilledCharacterEffect(InteractionState state) {
        targetCharacter = state.assignedCharacter.character;

        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        targetCharacter.Death();
    }
    private void RedirectedHunterInjuredCharacterEffect(InteractionState state) {
        targetCharacter = state.assignedCharacter.character;

        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void RedirectedCharacterKilledHunterEffect(InteractionState state) {
        targetCharacter = state.assignedCharacter.character;

        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        _characterInvolved.Death();
    }
    private void RedirectedCharacterInjuredHunterEffect(InteractionState state) {
        targetCharacter = state.assignedCharacter.character;

        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void DefaultHunterKilledCharacterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        targetCharacter.Death();
    }
    private void DefaultHunterInjuredCharacterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        targetCharacter.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    private void DefaultCharacterKilledHunterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        _characterInvolved.Death();
    }
    private void DefaultCharacterInjuredHunterEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        targetCharacter.LevelUp();

        if (targetCharacter.faction.name != "Neutral" && _characterInvolved.faction.name != "Neutral") {
            AdjustFactionsRelationship(targetCharacter.faction, _characterInvolved.faction, -1, state);
        }

        _characterInvolved.AddTrait(AttributeManager.Instance.allTraits["Injured"]);
    }
    #endregion

    public void SetTargetCharacter(Character targetCharacter) {
        this.targetCharacter = targetCharacter;
    }
    public Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (currCharacter.id != _characterInvolved.id && !currCharacter.currentParty.icon.isTravelling && currCharacter.IsInOwnParty() && !currCharacter.isLeader && currCharacter.race != RACE.SKELETON) {
                int weight = 0;
                if (currCharacter.isFactionless) {
                    weight += 25;
                } else if (currCharacter.faction.id != _characterInvolved.faction.id) {
                    FactionRelationship relationship = currCharacter.faction.GetRelationshipWith(_characterInvolved.faction);
                    if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED) {
                        weight += 20;
                    } else if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ALLY || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                        weight += 5;
                    }
                    if (currCharacter.level > _characterInvolved.level) {
                        weight -= 15;
                    } else if (currCharacter.level < _characterInvolved.level) {
                        weight += 15;
                    }
                }
                if (weight > 0) {
                    characterWeights.AddElement(currCharacter, weight);
                }
            }
        }
        if (characterWeights.GetTotalOfWeights() > 0) {
            return characterWeights.PickRandomElementGivenWeights();
        }
        return null;
        //throw new System.Exception("Could not find any character to recruit!");
    }

    private bool IsCharacterValidRedirectionTarget(Character character) {
        return character.id != _characterInvolved.id && !character.currentParty.icon.isTravelling && character.IsInOwnParty() && !character.isLeader && character.race != RACE.SKELETON;
    }
}
