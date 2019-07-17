using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Necronomicon : Artifact {

    private int raiseDeadLevel; //what level will the dead that was risen will be?

	public Necronomicon() : base(ARTIFACT_TYPE.Necronomicon) {
        raiseDeadLevel = 5;
    }

    #region Override
    protected override void OnPlaceArtifactOn(LocationGridTile tile) {
        base.OnPlaceArtifactOn(tile);
        List<Character> characters = tile.parentAreaMap.area.GetAllDeadCharactersInArea();
        for (int i = 0; i < characters.Count; i++) {
            Character currCharacter = characters[i];
            currCharacter.RaiseFromDeath(raiseDeadLevel, OnCharacterReturnedToLife);
        }
    }
    public override void LevelUp() {
        base.LevelUp();
        raiseDeadLevel++;
    }
    #endregion


    private void OnCharacterReturnedToLife(Character character) {
        CharacterState state = character.stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, gridTileLocation.parentAreaMap.area);
        state.SetIsUnending(true);
    }

    public override string ToString() {
        return "Necronomicon";
    }
}
