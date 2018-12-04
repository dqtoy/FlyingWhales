using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Spooked : CharacterAttribute {

    public Spooked() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.SPOOKED) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        if (_character.ownParty.specificLocation != _character.homeLandmark) {
            _character.ownParty.GoToLocation(_character.homeLandmark, PATHFINDING_MODE.PASSABLE);
        } else {
            List<BaseLandmark> inns = new List<BaseLandmark>();
            List<BaseLandmark> houses = new List<BaseLandmark>();
            for (int i = 0; i < _character.ownParty.specificLocation.tileLocation.areaOfTile.landmarks.Count; i++) {
                BaseLandmark landmark = _character.ownParty.specificLocation.tileLocation.areaOfTile.landmarks[i];
                if (landmark.landmarkObj.specificObjectType == LANDMARK_TYPE.INN) {
                    inns.Add(landmark);
                } else if (landmark.landmarkObj.specificObjectType == LANDMARK_TYPE.HOUSES
                    && _character.homeLandmark.id != landmark.id) {
                    houses.Add(landmark);
                }
            }
            if(inns.Count > 0) {
                BaseLandmark chosenInn = inns[Utilities.rng.Next(0, inns.Count)];
                _character.ownParty.GoToLocation(chosenInn, PATHFINDING_MODE.PASSABLE);
            } else if (houses.Count > 0) {
                BaseLandmark chosenHouse = houses[Utilities.rng.Next(0, houses.Count)];
                _character.ownParty.GoToLocation(chosenHouse, PATHFINDING_MODE.PASSABLE);
            }
        }
        _character.party.actionData.SetCannotPerformAction(true);
        ScheduleEndSpooked();
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        _character.party.actionData.SetCannotPerformAction(false);
    }
    #endregion

    private void ScheduleEndSpooked() {
        GameDate endDate = GameManager.Instance.Today();
        endDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(endDate, () => _character.RemoveAttribute(ATTRIBUTE.SPOOKED));
    }
}
