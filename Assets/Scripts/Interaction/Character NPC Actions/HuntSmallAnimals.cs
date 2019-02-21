using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntSmallAnimals : Interaction {
    private const string Start = "Start";
    private const string Hunt_Success = "Hunt Success";
    private const string Hunt_Mild_Success = "Hunt Mild Success";
    private const string Hunt_Fail = "Hunt Fail";

    private List<Food> _nearbyFood;

    public HuntSmallAnimals(Area interactable): base(interactable, INTERACTION_TYPE.HUNT_SMALL_ANIMALS, 0) {
        _name = "Hunt Small Animals";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState huntSuccess = new InteractionState(Hunt_Success, this);
        InteractionState huntMildSuccess = new InteractionState(Hunt_Mild_Success, this);
        InteractionState huntFail = new InteractionState(Hunt_Fail, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        huntSuccess.SetEffect(() => HuntSuccessEffect(huntSuccess));
        huntMildSuccess.SetEffect(() => HuntMildSuccessEffect(huntMildSuccess));
        huntFail.SetEffect(() => HuntFailEffect(huntFail));

        _states.Add(startState.name, startState);
        _states.Add(huntSuccess.name, huntSuccess);
        _states.Add(huntMildSuccess.name, huntMildSuccess);
        _states.Add(huntFail.name, huntFail);

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
        if (!InteractionManager.Instance.CanCreateInteraction(type, character)) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect() {
        List<LocationGridTile> tilesInRadius = _characterInvolved.specificLocation.areaMap.GetTilesInRadius(_characterInvolved.gridTileLocation, 3);
        _nearbyFood = new List<Food>();
        for (int i = 0; i < tilesInRadius.Count; i++) {
            if(tilesInRadius[i].objHere != null && tilesInRadius[i].objHere.poiType == POINT_OF_INTEREST_TYPE.FOOD) {
                Food food = tilesInRadius[i].objHere as Food;
                if(food.foodType == FOOD.RABBIT || food.foodType == FOOD.RAT) {
                    _nearbyFood.Add(food);
                }
            }
        }
        if(_nearbyFood.Count >= 2) {
            SetCurrentState(_states[Hunt_Success]);
        } else if (_nearbyFood.Count == 1) {
            SetCurrentState(_states[Hunt_Mild_Success]);
        } else {
            SetCurrentState(_states[Hunt_Fail]);
        }

    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        //Move the character to a random Wilderness or Explore Area tile
        //TODO: Explore area and randomization
        _characterInvolved.MoveToRandomStructureInArea(STRUCTURE_TYPE.WILDERNESS);
    }
    private void HuntSuccessEffect(InteractionState state) {
        _characterInvolved.ResetFullnessMeter();
        int index1 = UnityEngine.Random.Range(0, _nearbyFood.Count);
        _characterInvolved.currentStructure.RemovePOI(_nearbyFood[index1]);
        _nearbyFood.RemoveAt(index1);

        int index2 = UnityEngine.Random.Range(0, _nearbyFood.Count);
        _characterInvolved.currentStructure.RemovePOI(_nearbyFood[index2]);
    }
    private void HuntMildSuccessEffect(InteractionState state) {
        _characterInvolved.AdjustFullness(80);
        _characterInvolved.currentStructure.RemovePOI(_nearbyFood[0]);
    }
    private void HuntFailEffect(InteractionState state) {
    }
    #endregion
}
