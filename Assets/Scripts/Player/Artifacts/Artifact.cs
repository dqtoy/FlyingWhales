using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artifact : TileObject {

    public ARTIFACT_TYPE type { get; private set; }
    public int level { get; private set; }

    public bool hasBeenUsed { get; private set; }
    public Artifact(ARTIFACT_TYPE type) {
        this.type = type;
        level = 1;
        TILE_OBJECT_TYPE parsed = (TILE_OBJECT_TYPE) Enum.Parse(typeof(TILE_OBJECT_TYPE), type.ToString(), true);
        poiGoapActions = new List<INTERACTION_TYPE>();
        Initialize(parsed);
    }
    public Artifact(SaveDataArtifact data) {
        this.type = data.type;
        level = 1;
        TILE_OBJECT_TYPE parsed = (TILE_OBJECT_TYPE) Enum.Parse(typeof(TILE_OBJECT_TYPE), type.ToString(), true);
        poiGoapActions = new List<INTERACTION_TYPE>();
        Initialize(data, parsed);
    }

    #region Overrides
    public override void SetGridTileLocation(LocationGridTile tile) {
        base.SetGridTileLocation(tile);
        if (tile != null) {
            Debug.Log("Placed artifact " + type.ToString() + " at " + tile.ToString());
            OnPlaceArtifactOn(tile);
        } else {
            OnRemoveArtifact();
        }
    }
    public override string ToString() {
        return Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
    }
    #endregion

    #region Virtuals
    protected virtual void OnPlaceArtifactOn(LocationGridTile tile) {
        hasBeenUsed = true;
        Messenger.AddListener<Area>(Signals.SUCCESS_INVASION_AREA, OnSuccessInvadeArea);
    }
    protected virtual void OnRemoveArtifact() {
        Messenger.RemoveListener<Area>(Signals.SUCCESS_INVASION_AREA, OnSuccessInvadeArea);
    }
    public virtual void OnInspect(Character inspectedBy, out Log result) {
        result = new Log(GameManager.Instance.Today(), "Artifact", this.GetType().ToString(), "on_inspect");
    }
    public virtual void LevelUp() {
        level++;
    }
    public virtual void SetLevel(int amount) {
        level = amount;
    } 
    #endregion

    private void OnSuccessInvadeArea(Area area) {
        hasBeenUsed = false;
        gridTileLocation.structure.RemovePOI(this);
    }

    public void Reset() {
        hasBeenUsed = false;
    }

}
