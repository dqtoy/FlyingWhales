using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lure : PlayerJobAction {

    private int _lureRange;

    public List<Character> targetCharacters { get; private set; }
    public List<LocationGridTile> tileChoices { get; private set; }
    public Character currentAssignedCharacter { get; private set; }
    public bool isGamePausedOnLure { get; private set; }

    public Lure() : base(INTERVENTION_ABILITY.LURE) {
        description = "Force a character to go to a specified nearby location.";
        SetDefaultCooldownTime(24);
        abilityTags.Add(ABILITY_TAG.NONE);
        targetCharacters = new List<Character>();
        tileChoices = new List<LocationGridTile>();
        targetType = JOB_ACTION_TARGET.CHARACTER;
        hasSecondPhase = true;
    }

    #region Overrides
    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        targetCharacters.Clear();
        tileChoices.Clear();
        currentAssignedCharacter = null;

        List<Character> targets = new List<Character>();
        if (targetPOI is Character) {
            targets.Add(targetPOI as Character);
        } else if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) { targets.AddRange(to.users); }
        } else {
            return;
        }
        if (targets.Count > 0) {
            for (int i = 0; i < targets.Count; i++) {
                Character currTarget = targets[i];
                if (CanPerformActionTowards(assignedCharacter, currTarget)) {
                    targetCharacters.Add(currTarget);
                }
            }
            if(targetCharacters.Count > 0) {
                currentAssignedCharacter = assignedCharacter;
                tileChoices = targetCharacters[0].GetTilesInRadius(_lureRange, includeTilesInDifferentStructure: true);
                LurePhase2();
                base.ActivateAction(currentAssignedCharacter, targetCharacters[0]);
            }
        }
    }
    protected override bool CanPerformActionTowards(Character character, IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) {
                for (int i = 0; i < to.users.Length; i++) {
                    Character currUser = to.users[i];
                    bool canTarget = CanPerformActionTowards(character, currUser);
                    if (canTarget) { return true; }
                }
            }
        }
        return false;
    }
    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (!targetCharacter.IsInOwnParty()) {
            return false;
        }
        if (character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        return base.CanPerformActionTowards(character, targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            return CanTarget(targetPOI as Character);
        } else if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) {
                for (int i = 0; i < to.users.Length; i++) {
                    Character currUser = to.users[i];
                    if (currUser != null) {
                        bool canTarget = CanTarget(currUser);
                        if (canTarget) { return true; }
                    }
                }
            }
        }
        return false;
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if (lvl == 1) {
            _lureRange = 3;
        } else if (lvl == 2) {
            _lureRange = 4;
        } else if (lvl == 3) {
            _lureRange = 5;
        }
    }
    #endregion

    private bool CanTarget(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (!targetCharacter.IsInOwnParty()) {
            return false;
        }
        if (targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        return true;
    }

    #region Utilities
    private void LurePhase2() {
        isGamePausedOnLure = GameManager.Instance.isPaused;
        GameManager.Instance.SetPausedState(true);
        InteriorMapManager.Instance.HighlightTiles(tileChoices);
        CursorManager.Instance.AddRightClickAction(() => PickTileToGoTo());
        CursorManager.Instance.AddPendingLeftClickAction(() => PickTileToGoTo());
    }
    private void CancelLure() {
        InteriorMapManager.Instance.UnhighlightTiles();
        GameManager.Instance.SetPausedState(isGamePausedOnLure);
    }
    private void PickTileToGoTo() {
        LocationGridTile hoveredTile = InteriorMapManager.Instance.currentlyHoveredTile;
        if (hoveredTile != null && tileChoices.Contains(hoveredTile)) {
            for (int i = 0; i < targetCharacters.Count; i++) {
                Character character = targetCharacters[i];
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
                        //else {
                        //    character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                        //}
                    }
                    character.AdjustIsWaitingForInteraction(1);
                    character.currentAction.StopAction(true);
                    character.AdjustIsWaitingForInteraction(-1);
                }
                character.marker.UpdateActionIcon();
                character.marker.GoTo(hoveredTile);
            }
        }
        CursorManager.Instance.ClearLeftClickActions();
        CursorManager.Instance.ClearRightClickActions();
        InteriorMapManager.Instance.UnhighlightTiles();
        GameManager.Instance.SetPausedState(isGamePausedOnLure);
    }
    #endregion
}
