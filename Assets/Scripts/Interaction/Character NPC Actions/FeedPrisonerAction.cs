using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedPrisonerAction : Interaction { 
    private const string Start = "Start";
    private const string Actor_Disappointed = "Actor Disappointed";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public FeedPrisonerAction(Area interactable): base(interactable, INTERACTION_TYPE.FEED_PRISONER_ACTION, 0) {
        _name = "Feed Prisoner Action";
    }

    #region Override
    public override void CreateStates() {
        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState actorDisappointed = new InteractionState(Actor_Disappointed, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        actorDisappointed.SetEffect(() => ActorDisappointedEffect(actorDisappointed));

        _states.Add(startState.name, startState);
        _states.Add(actorDisappointed.name, actorDisappointed);

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
        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(character));
        }
        if (_targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character targetCharacter) {
        this._targetCharacter = targetCharacter;
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect() {
        SetCurrentState(_states[Actor_Disappointed]);
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.currentStructure);
    }
    private void ActorDisappointedEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.ResetFullnessMeter();
    }
    #endregion

    private Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        List<LocationStructure> structures = characterInvolved.specificLocation.GetStructuresAtLocation(true);
        for (int i = 0; i < structures.Count; i++) {
            LocationStructure currStructure = structures[i];
            for (int j = 0; j < currStructure.charactersHere.Count; j++) {
                Character currCharacter = currStructure.charactersHere[j];
                if (currCharacter.GetTraitOr("Abducted", "Restrained") != null) {
                    int weight = 0;
                    Trait hungryOrStarving = currCharacter.GetTraitOr("Hungry", "Starving");
                    if (hungryOrStarving != null) {
                        if (hungryOrStarving.name == "Hungry") {
                            weight += 5;
                        } else {
                            weight += 100;
                        }
                    }
                    if (characterInvolved.GetRelationshipTraitWith(currCharacter, RELATIONSHIP_TRAIT.ENEMY) != null) {
                        weight -= 100;
                    } else {
                        weight += 50;
                    }
                    if (weight > 0) {
                        characterWeights.AddElement(currCharacter, weight);
                    }
                }
            }
        }
        if(characterWeights.Count > 0) {
            return characterWeights.PickRandomElementGivenWeights();
        }
        return null;
    }
}
