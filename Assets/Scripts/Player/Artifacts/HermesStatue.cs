using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HermesStatue : Artifact {

    private Region chosenRegion;
    private int uses;
    public int currentUses { get; private set; }

    public HermesStatue() : base(ARTIFACT_TYPE.Hermes_Statue) {
        //poiGoapActions.Add(INTERACTION_TYPE.INSPECT);
        uses = 1;
    }
    //public Hermes_Statue(SaveDataArtifactSlot data) : base(data) {
    //    //poiGoapActions.Add(INTERACTION_TYPE.INSPECT);
    //    uses = 1;
    //}
    public HermesStatue(SaveDataArtifact data) : base(data) {
        //poiGoapActions.Add(INTERACTION_TYPE.INSPECT);
        uses = 1;
    }

    public void SetCurrentUses(int amount) {
        currentUses = amount;
    }

    #region Overrides
    public override void OnInspect(Character inspectedBy) { //, out Log result
        base.OnInspect(inspectedBy); //, out result
        List<Region> choices = GridMap.Instance.allRegions.Where(x => !x.coreTile.isCorrupted).ToList();
        choices.Remove(inspectedBy.specificLocation.region);
        if (choices.Count > 0 && currentUses < uses) {
            chosenRegion = choices[Random.Range(0, choices.Count)];
            OnInspectActionDone(inspectedBy);
            if (LocalizationManager.Instance.HasLocalizedValue("Artifact", name, "on_inspect")) {
                Log result = new Log(GameManager.Instance.Today(), "Artifact", name, "on_inspect");
                result.AddToFillers(inspectedBy, inspectedBy.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                result.AddToFillers(chosenRegion, chosenRegion.name, LOG_IDENTIFIER.LANDMARK_1);
                inspectedBy.RegisterLogAndShowNotifToThisCharacterOnly(result, onlyClickedCharacter: false);
            }
        } else {
            Debug.LogWarning(inspectedBy.name + " inspected an hermes statue, but there were no more settlements to teleport to. Statue is useless.");
        }
    }
    public override void LevelUp() {
        base.LevelUp();
        uses++;
    }
    public override void SetLevel(int amount) {
        base.SetLevel(amount);
        uses = amount;
    }
    #endregion

    private void OnInspectActionDone(Character inspectedBy) { //string result, GoapAction action
        //action.actor.GoapActionResult(result, action);
        //Characters that inspect this will be teleported to a different settlement. If no other settlement exists, this will be useless.
        if (chosenRegion.owner != null) {
            inspectedBy.ChangeFactionTo(chosenRegion.owner);
        } else {
            inspectedBy.ChangeFactionTo(FactionManager.Instance.neutralFaction);
        }
        inspectedBy.MigrateHomeTo(chosenRegion);
        inspectedBy.SetPOIState(POI_STATE.INACTIVE);
        inspectedBy.marker.gameObject.SetActive(false);
        inspectedBy.marker.StopMovement();
        Messenger.Broadcast(Signals.PARTY_STARTED_TRAVELLING, inspectedBy.ownParty);
        inspectedBy.specificLocation.RemoveCharacterFromLocation(inspectedBy);
        //inspectedBy.DestroyMarker();
        chosenRegion.AddCharacterToLocation(inspectedBy);
        inspectedBy.SetPOIState(POI_STATE.ACTIVE);

        inspectedBy.UnsubscribeSignals();
        //inspectedBy.ClearAllAwareness(); //so teleported character won't revisit old area.
        ////remove character from other character's awareness
        //for (int i = 0; i < gridTileLocation.parentAreaMap.area.charactersAtLocation.Count; i++) {
        //    Character currCharacter = gridTileLocation.parentAreaMap.area.charactersAtLocation[i];
        //    currCharacter.RemoveAwareness(inspectedBy);
        //}
        currentUses++;
        if (currentUses == uses) {
            gridTileLocation.structure.RemovePOI(this);
        }
    }

    public override string ToString() {
        return "Hermes Statue";
    }

}

public class SaveDataHermesStatue : SaveDataArtifact {
    public int currentUses;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        HermesStatue obj = tileObject as HermesStatue;
        currentUses = obj.currentUses;
    }

    public override TileObject Load() {
        HermesStatue obj = base.Load() as HermesStatue;
        obj.SetCurrentUses(currentUses);
        return obj;
    }
}