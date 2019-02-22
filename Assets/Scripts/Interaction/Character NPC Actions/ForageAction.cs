using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForageAction : Interaction {
    private const string Start = "Start";
    private const string Forage_Success = "Forage Success";
    private const string Forage_Mild_Success = "Forage Mild Success";
    private const string Forage_Fail = "Forage Fail";

    private List<Food> _nearbyFood;

    public ForageAction(Area interactable): base(interactable, INTERACTION_TYPE.FORAGE_ACTION, 0) {
        _name = "Forage Action";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState forageSuccess = new InteractionState(Forage_Success, this);
        InteractionState forageMildSuccess = new InteractionState(Forage_Mild_Success, this);
        InteractionState forageFail = new InteractionState(Forage_Fail, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        forageSuccess.SetEffect(() => ForageSuccessEffect(forageSuccess));
        forageMildSuccess.SetEffect(() => ForageMildSuccessEffect(forageMildSuccess));
        forageFail.SetEffect(() => ForageFailEffect(forageFail));

        _states.Add(startState.name, startState);
        _states.Add(forageSuccess.name, forageSuccess);
        _states.Add(forageMildSuccess.name, forageMildSuccess);
        _states.Add(forageFail.name, forageFail);

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
        //if (!InteractionManager.Instance.CanCreateInteraction(type, character)) {
        //    return false;
        //}
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect() {
        List<LocationGridTile> tilesInRadius = _characterInvolved.specificLocation.areaMap.GetTilesInRadius(_characterInvolved.gridTileLocation, 3);
        _nearbyFood = new List<Food>();
        for (int i = 0; i < tilesInRadius.Count; i++) {
            if (tilesInRadius[i].objHere != null && tilesInRadius[i].objHere.poiType == POINT_OF_INTEREST_TYPE.FOOD) {
                Food food = tilesInRadius[i].objHere as Food;
                if (food.foodType == FOOD.BERRY || food.foodType == FOOD.MUSHROOM) {
                    _nearbyFood.Add(food);
                }
            }
        }
        if (_nearbyFood.Count >= 2) {
            SetCurrentState(_states[Forage_Success]);
        } else if (_nearbyFood.Count == 1) {
            SetCurrentState(_states[Forage_Mild_Success]);
        } else {
            SetCurrentState(_states[Forage_Fail]);
        }

    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        //Move the character to a random Wilderness or Explore Area tile
        //TODO: Explore area and randomization
        _characterInvolved.MoveToRandomStructureInArea(STRUCTURE_TYPE.WILDERNESS);
    }
    private void ForageSuccessEffect(InteractionState state) {
        _characterInvolved.ResetFullnessMeter();
        int index1 = UnityEngine.Random.Range(0, _nearbyFood.Count);
        _characterInvolved.currentStructure.RemovePOI(_nearbyFood[index1]);
        _nearbyFood.RemoveAt(index1);

        int index2 = UnityEngine.Random.Range(0, _nearbyFood.Count);
        _characterInvolved.currentStructure.RemovePOI(_nearbyFood[index2]);
    }
    private void ForageMildSuccessEffect(InteractionState state) {
        _characterInvolved.AdjustFullness(80);
        _characterInvolved.currentStructure.RemovePOI(_nearbyFood[0]);
    }
    private void ForageFailEffect(InteractionState state) {
    }
    #endregion
}
