using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Necronomicon : Artifact {

    private bool isActivated;

    private int raiseDeadLevel; //what level will the dead that was risen will be?

	public Necronomicon() : base(ARTIFACT_TYPE.Necronomicon) {
        raiseDeadLevel = 5;
    }
    public Necronomicon(SaveDataArtifact data) : base(data) {
        raiseDeadLevel = 5;
    }

    #region Override
    //protected override void OnPlaceArtifactOn(LocationGridTile tile) {
    //    base.OnPlaceArtifactOn(tile);
    //    Activate();
    //}
    public override void OnInspect(Character inspectedBy, out Log result) {
        base.OnInspect(inspectedBy, out result);
        if (!isActivated) {
            Activate();
        }
    }
    public override void LevelUp() {
        base.LevelUp();
        raiseDeadLevel++;
    }
    public override void SetLevel(int amount) {
        base.SetLevel(amount);
        raiseDeadLevel = amount;
    }
    #endregion


    private void OnCharacterReturnedToLife(Character character) {
        CharacterState state = character.stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, gridTileLocation.parentAreaMap.area);
        state.SetIsUnending(true);
    }
    private void Activate() {
        isActivated = true;
        List<Character> characters = gridTileLocation.parentAreaMap.area.GetAllDeadCharactersInArea();
        for (int i = 0; i < characters.Count; i++) {
            Character currCharacter = characters[i];
            if (currCharacter is Summon) {
                //character is summon, not raising to life
            } else {
                currCharacter.RaiseFromDeath(raiseDeadLevel, OnCharacterReturnedToLife, PlayerManager.Instance.player.playerFaction);
            }

        }
    }

    public override string ToString() {
        return "Necronomicon";
    }
}
