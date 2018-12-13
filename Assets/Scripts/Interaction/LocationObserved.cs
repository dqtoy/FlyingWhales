using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocationObserved : Interaction {

    public LocationObserved(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.LOCATION_OBSERVED, 70) {
        _name = "Location Observed";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        //**Text Description**: [Minion Name] has scouted and gathered enough intel about [Location Name] NOTE: Can already handle this in base interaction, no need to override.
        startState.SetEffect(() => LocationObservedEffect(startState));

        _states.Add(startState.name, startState);
        SetCurrentState(startState);
    }
    #endregion

    private void LocationObservedEffect(InteractionState state) {
        //**Mechanics**: Unlock Location Intel
        //**Log**: [Minion Name] obtained intel about [Location Name].
        PlayerManager.Instance.player.AddToken(interactable.tileLocation.areaOfTile.locationToken);
        investigatorMinion.LevelUp();
    }
}
