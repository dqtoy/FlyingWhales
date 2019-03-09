using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbductAction : Interaction {


    private const string Start = "Start";
    private const string Abduction_Success = "Abduction Success";
    private const string Abductor_Injured = "Abductor Injured";
    private const string Abductor_Knocked_Out = "Abductor Knocked Out";

    public override LocationStructure actionStructureLocation {
        get { return _targetStructure; }
    }

    public AbductAction(Area interactable): base(interactable, INTERACTION_TYPE.ABDUCT_ACTION, 0) {
        _name = "Abduct Action";
        //_categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.RECRUITMENT, INTERACTION_CATEGORY.SUBTERFUGE };
        //_alignment = INTERACTION_ALIGNMENT.EVIL;
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState abductionSuccess = new InteractionState(Abduction_Success, this);
        InteractionState abductorInjured = new InteractionState(Abductor_Injured, this);
        InteractionState abductorKnockedOut = new InteractionState(Abductor_Knocked_Out, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_characterInvolved.homeArea, _characterInvolved.homeArea.name, LOG_IDENTIFIER.LANDMARK_2);
        startStateDescriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        abductionSuccess.SetEffect(() => AbductionSuccessEffect(abductionSuccess));
        abductorInjured.SetEffect(() => AbductorInjuredEffect(abductorInjured));
        abductorKnockedOut.SetEffect(() => AbductorKnockedOutEffect(abductorKnockedOut));

        _states.Add(startState.name, startState);
        _states.Add(abductionSuccess.name, abductionSuccess);
        _states.Add(abductorInjured.name, abductorInjured);
        _states.Add(abductorKnockedOut.name, abductorKnockedOut);

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
    public override bool CanInteractionBeDoneBy(Character character) {
        if(targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(character), character);
        }
        if (targetCharacter == null || character.homeArea.IsResidentsFull()) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character character, Character actor) {
        _targetCharacter = character;
        if (targetCharacter != null) {
            _targetStructure = _targetCharacter.currentStructure;
            targetGridLocation = _targetCharacter.GetNearestUnoccupiedTileFromThis();
        }
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect() {
        List<Character> attackers = new List<Character>();
        attackers.Add(_characterInvolved);

        List<Character> defenders = new List<Character>();
        defenders.Add(_targetCharacter);

        float attackersChance = 0f;
        float defendersChance = 0f;

        CombatManager.Instance.GetCombatChanceOfTwoLists(attackers, defenders, out attackersChance, out defendersChance);

        string nextState = string.Empty;
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < attackersChance) {
            //Abductor Win
            nextState = Abduction_Success;
        } else {
            //Target Win
            WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
            resultWeights.AddElement(Abductor_Injured, 20);
            resultWeights.AddElement(Abductor_Knocked_Out, 20);
            nextState = resultWeights.PickRandomElementGivenWeights();
        }

        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region State Effect
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetStructure, targetGridLocation, _targetCharacter);
    }
    private void AbductionSuccessEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        state.descriptionLog.AddToFillers(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(targetCharacter.faction, targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2));

        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        AbductCharacter(targetCharacter);
        //**Level Up**: Abductioner Character +1
        //_characterInvolved.LevelUp();
    }
    private void AbductorInjuredEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
 
        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.AddTrait("Injured");
    }
    private void AbductorKnockedOutEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.AddTrait("Unconscious");
    }
    #endregion

    private void AbductCharacter(Character character) {
        //only add abducted trait to characters that have not been abducted yet, this is to retain it's original faction
        Abducted abductedTrait = new Abducted(character.homeArea);
        character.AddTrait(abductedTrait, _characterInvolved);
        //character.MigrateHomeTo(_characterInvolved.homeArea);
        _characterInvolved.ownParty.AddCharacter(character);
        _characterInvolved.currentStructure.AddCharacterAtLocation(character);
        Interaction interactionAbductor = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, interactable);
        _characterInvolved.InduceInteraction(interactionAbductor);

        _targetStructure = _characterInvolved.homeStructure; //This is so that the interaction intel knows where the abducted character was dropped
    }
    public Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < interactable.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.charactersAtLocation[i];
            if (currCharacter.id != characterInvolved.id && !currCharacter.currentParty.icon.isTravelling && currCharacter.IsInOwnParty()) {
                int weight = 0;
                if (currCharacter.isFactionless) {
                    weight += 15;
                } else if (currCharacter.faction.id != characterInvolved.faction.id) {
                    FactionRelationship relationship = currCharacter.faction.GetRelationshipWith(characterInvolved.faction);
                    if(relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ENEMY || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.DISLIKED) {
                        weight += 25;
                    }else if (relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.ALLY || relationship.relationshipStatus == FACTION_RELATIONSHIP_STATUS.FRIEND) {
                        weight -= 10;
                    }
                    if(currCharacter.level > characterInvolved.level) {
                        weight -= 15;
                    }else if(currCharacter.level < characterInvolved.level) {
                        weight += 15;
                    }
                }
                if(weight > 0) {
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
}
