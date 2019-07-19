using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Hermes_Statue : Artifact {

    private Area chosenArea;
    private int uses;
    private int currentUses;

    public Hermes_Statue() : base(ARTIFACT_TYPE.Hermes_Statue) {
        poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.INSPECT };
        uses = 3;
    }

    #region Overrides
    public override void OnInspect(Character inspectedBy, out Log result) {
        base.OnInspect(inspectedBy, out result);
        List<Area> choices = new List<Area>(LandmarkManager.Instance.allNonPlayerAreas.Where(x => !x.coreTile.isCorrupted));
        choices.Remove(inspectedBy.specificLocation);
        if (choices.Count > 0 && currentUses < uses) {
            chosenArea = choices[Random.Range(0, choices.Count)];
            inspectedBy.currentAction.SetEndAction(OnInspectActionDone);
            result.AddToFillers(chosenArea, chosenArea.name, LOG_IDENTIFIER.LANDMARK_1);
        } else {
            Debug.LogWarning(inspectedBy.name + " inspected an hermes statue, but there were no more settlements to teleport to. Statue is useless.");
        }
    }
    public override void LevelUp() {
        base.LevelUp();
        uses++;
    }
    #endregion

    private void OnInspectActionDone(string result, GoapAction action) {
        action.actor.GoapActionResult(result, action);
        //Characters that inspect this will be teleported to a different settlement. If no other settlement exists, this will be useless.
        action.actor.ChangeFactionTo(chosenArea.owner);
        action.actor.MigrateHomeTo(chosenArea);
        action.actor.specificLocation.RemoveCharacterFromLocation(action.actor);
        action.actor.DestroyMarker();
        chosenArea.AddCharacterToLocation(action.actor);
        action.actor.UnsubscribeSignals();
        action.actor.ClearAllAwareness(); //so teleported character won't revisit old area.
        //remove character from other character's awareness
        for (int i = 0; i < gridTileLocation.parentAreaMap.area.charactersAtLocation.Count; i++) {
            Character currCharacter = gridTileLocation.parentAreaMap.area.charactersAtLocation[i];
            currCharacter.RemoveAwareness(action.actor);
        }
        currentUses++;
        if (currentUses == uses) {
            gridTileLocation.structure.RemovePOI(this);
        }
    }

    public override string ToString() {
        return "Hermes Statue";
    }

}
