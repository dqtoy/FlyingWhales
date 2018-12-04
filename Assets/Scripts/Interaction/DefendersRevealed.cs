using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefendersRevealed : Interaction {
    public DefendersRevealed(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.DEFENDERS_REVEALED, 70) {
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        //**Text Description**: [Minion Name] has obtained information regarding the defenders of [Location Name].

        startState.SetEffect(() => DefenderRevealedEffect(startState));

        _states.Add(startState.name, startState);
        SetCurrentState(startState);
    }
    #endregion

    private void DefenderRevealedEffect(InteractionState state) {
        //**Mechanics**: Unlock Location's Defender Tile
        //**Log**: [Minion Name] obtained intel about [Location Name]'s defenders.
        PlayerManager.Instance.player.AddIntel(interactable.tileLocation.areaOfTile.defenderIntel);
        explorerMinion.LevelUp();
    }
}
