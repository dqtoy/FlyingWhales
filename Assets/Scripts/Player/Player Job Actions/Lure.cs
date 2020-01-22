using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Lure : PlayerSpell {

    //private int _lureRange;

    //public List<Character> targetCharacters { get; private set; }
    //public List<LocationGridTile> tileChoices { get; private set; }
    public bool isGamePausedOnLure { get; private set; }
    public Character targetCharacter { get; private set; }

    public Lure() : base(SPELL_TYPE.LURE) {
        SetDefaultCooldownTime(24);
        //abilityTags.Add(ABILITY_TAG.NONE);
        //targetCharacters = new List<Character>();
        //tileChoices = new List<LocationGridTile>();
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
        //hasSecondPhase = true;
    }

    #region Overrides
    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            isGamePausedOnLure = GameManager.Instance.isPaused;
            targetCharacter = targetPOI as Character;
            UIManager.Instance.ShowClickableObjectPicker(GridMap.Instance.allRegions.ToList(), GoToRegion, null, CanChooseRegion, "Select Region to Lure " + targetCharacter.name, showCover: true);
        } 
    }
    protected override bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (!targetCharacter.IsInOwnParty()) {
            return false;
        }
        //if (targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return base.CanPerformActionTowards(targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI, ref string hoverText) {
        if (targetPOI is Character) {
            return CanTarget(targetPOI as Character, ref hoverText);
        } 
        return false;
    }
    //protected override void OnLevelUp() {
    //    base.OnLevelUp();
    //    if (level == 1) {
    //        _lureRange = 3;
    //    } else if (level == 2) {
    //        _lureRange = 4;
    //    } else if (level == 3) {
    //        _lureRange = 5;
    //    }
    //}
    #endregion

    private bool CanChooseRegion(Region region) {
        bool isCharacterAlreadyInRegion = targetCharacter.currentRegion == region;
        //if(targetCharacter.currentRegion != null) {
        //    isCharacterAlreadyInRegion = targetCharacter.currentRegion == region;
        //} else {
        //    isCharacterAlreadyInRegion = targetCharacter.currentArea.region == region;
        //}
        return !region.coreTile.isCorrupted && !isCharacterAlreadyInRegion;
    }
    private void GoToRegion(object r) {
        Region region = r as Region;
        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.SEARCHING, region.regionTileObject, targetCharacter);
        targetCharacter.jobQueue.AddJobInQueue(job);
        UIManager.Instance.HideObjectPicker();
        GameManager.Instance.SetPausedState(isGamePausedOnLure);
        base.ActivateAction(targetCharacter);
        // CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.SEARCHING_WORLD_EVENT, CHARACTER_STATE.MOVE_OUT, region, targetCharacter);
        // targetCharacter.jobQueue.AddJobInQueue(job);
        // UIManager.Instance.HideObjectPicker();
        // GameManager.Instance.SetPausedState(isGamePausedOnLure);
    }

    private bool CanTarget(Character targetCharacter, ref string hoverText) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (!targetCharacter.IsInOwnParty()) {
            return false;
        }
        //if (targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return base.CanTarget(targetCharacter, ref hoverText);
    }

    //#region Utilities
    //private void LurePhase2() {
    //    isGamePausedOnLure = GameManager.Instance.isPaused;
    //    GameManager.Instance.SetPausedState(true);
    //    InteriorMapManager.Instance.HighlightTiles(tileChoices);
    //    CursorManager.Instance.AddRightClickAction(() => PickTileToGoTo());
    //    CursorManager.Instance.AddPendingLeftClickAction(() => PickTileToGoTo());
    //}
    //private void CancelLure() {
    //    InteriorMapManager.Instance.UnhighlightTiles();
    //    GameManager.Instance.SetPausedState(isGamePausedOnLure);
    //}
    //private void PickTileToGoTo() {
    //    GameManager.Instance.SetPausedState(false);
    //    LocationGridTile hoveredTile = InteriorMapManager.Instance.currentlyHoveredTile;
    //    if (hoveredTile != null && tileChoices.Contains(hoveredTile)) {
    //        for (int i = 0; i < targetCharacters.Count; i++) {
    //            Character character = targetCharacters[i];
    //            if (character.stateComponent.currentState != null) {
    //                character.stateComponent.currentState.OnExitThisState();
    //                //This call is doubled so that it will also exit the previous major state if there's any
    //                if (character.stateComponent.currentState != null) {
    //                    character.stateComponent.currentState.OnExitThisState();
    //                }
    //            } else if (character.stateComponent.stateToDo != null) {
    //                character.stateComponent.SetStateToDo(null);
    //            } else {
    //                if (character.currentParty.icon.isTravelling) {
    //                    if (character.currentParty.icon.travelLine == null) {
    //                        character.marker.StopMovement();
    //                    }
    //                    //else {
    //                    //    character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
    //                    //}
    //                }
    //                character.AdjustIsWaitingForInteraction(1);
    //                if (character.currentAction != null) {
    //                    character.currentAction.StopAction(true);
    //                }
    //                character.AdjustIsWaitingForInteraction(-1);
    //            }
    //            character.marker.UpdateActionIcon();
    //            character.marker.GoTo(hoveredTile);

    //            UIManager.Instance.ShowCharacterInfo(character);
    //        }
    //    }
    //    CursorManager.Instance.ClearLeftClickActions();
    //    CursorManager.Instance.ClearRightClickActions();
    //    InteriorMapManager.Instance.UnhighlightTiles();
    //    GameManager.Instance.SetDelayedPausedState(isGamePausedOnLure);
    //}
    //#endregion
}

public class LureData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.LURE;
    public override string name { get { return "Lure"; } }
    public override string description { get { return "Force a character to go to a specified nearby location."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SABOTAGE; } }
}