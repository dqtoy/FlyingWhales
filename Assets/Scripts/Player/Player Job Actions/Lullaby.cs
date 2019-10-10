using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lullaby : PlayerJobAction {
    public Lullaby() : base(INTERVENTION_ABILITY.LULLABY) {
        tier = 2;
        abilityRadius = 1;
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.TILE };
        //abilityTags.Add(ABILITY_TAG.NONE);
    }

    #region Overrides
    protected override void OnLevelUp() {
        base.OnLevelUp();
        abilityRadius = level;
    }
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        //List<ITraitable> flammables = new List<ITraitable>();
        List<LocationGridTile> tiles = targetTile.parentAreaMap.GetTilesInRadius(targetTile, abilityRadius, includeCenterTile: true, includeTilesInDifferentStructure: true);

        List<Character> charactersInHighlightedTiles = new List<Character>();
        for (int i = 0; i < targetTile.parentAreaMap.area.charactersAtLocation.Count; i++) {
            Character currCharacter = targetTile.parentAreaMap.area.charactersAtLocation[i];
            if (tiles.Contains(currCharacter.gridTileLocation)) {
                charactersInHighlightedTiles.Add(currCharacter);
            }
        }

        for (int i = 0; i < charactersInHighlightedTiles.Count; i++) {
            Character character = charactersInHighlightedTiles[i];

            if(character.GetNormalTrait("Resting") == null) {
                if (character.stateComponent.currentState != null) {
                    character.stateComponent.currentState.OnExitThisState();
                    //This call is doubled so that it will also exit the previous major state if there's any
                    if (character.stateComponent.currentState != null) {
                        character.stateComponent.currentState.OnExitThisState();
                    }
                } else if (character.stateComponent.stateToDo != null) {
                    character.stateComponent.SetStateToDo(null);
                } else {
                    if (character.currentParty.icon.isTravelling) {
                        if (character.currentParty.icon.travelLine == null) {
                            character.marker.StopMovement();
                        }
                    }
                    character.AdjustIsWaitingForInteraction(1);
                    if (character.currentAction != null) {
                        character.currentAction.StopAction(true, "Stopped by the player");
                    }
                    character.AdjustIsWaitingForInteraction(-1);
                }
                character.marker.UpdateActionIcon();
            }
            character.ExhaustCharacter();
        }
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        List<LocationGridTile> tiles = targetTile.parentAreaMap.GetTilesInRadius(targetTile, abilityRadius, includeCenterTile: true, includeTilesInDifferentStructure: true);
        InteriorMapManager.Instance.HighlightTiles(tiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        List<LocationGridTile> tiles = targetTile.parentAreaMap.GetTilesInRadius(targetTile, abilityRadius, includeCenterTile: true, includeTilesInDifferentStructure: true);
        InteriorMapManager.Instance.UnhighlightTiles(tiles);
    }
    #endregion
}

public class LullabyData : PlayerJobActionData {
    public override string name { get { return "Lullaby"; } }
    public override string description { get { return "Makes characters in an area exhausted."; } }
    public override INTERVENTION_ABILITY_CATEGORY category { get { return INTERVENTION_ABILITY_CATEGORY.SABOTAGE; } }
}