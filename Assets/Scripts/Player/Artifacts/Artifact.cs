using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class Artifact : TileObject, IWorldObject {
    public ARTIFACT_TYPE type { get; private set; }
    public int level { get; private set; }
    public bool hasBeenUsed { get; private set; }

    #region getters/setters
    public string worldObjectName {
        get { return name; }
    }
    public WORLD_OBJECT_TYPE worldObjectType {
        get { return WORLD_OBJECT_TYPE.ARTIFACT; }
    }
    #endregion

    public Artifact(ARTIFACT_TYPE type) {
        this.type = type;
        level = 1;
        TILE_OBJECT_TYPE parsed = (TILE_OBJECT_TYPE) Enum.Parse(typeof(TILE_OBJECT_TYPE), type.ToString(), true);
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(parsed);
    }
    //public Artifact(SaveDataArtifactSlot data) {
    //    this.type = data.type;
    //    level = 1;
    //    TILE_OBJECT_TYPE parsed = (TILE_OBJECT_TYPE) Enum.Parse(typeof(TILE_OBJECT_TYPE), type.ToString(), true);
    //    poiGoapActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TILE_OBJECT_DESTROY };
    //    Initialize(data, parsed);
    //}
    public Artifact(SaveDataArtifact data) {
        this.type = data.artifactType;
        level = 1;
        advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ASSAULT };
        Initialize(data);
    }

    #region Overrides
    //public override void SetGridTileLocation(LocationGridTile tile) {
    //    base.SetGridTileLocation(tile);
    //    if (tile != null) {
    //        //Debug.Log("Placed artifact " + type.ToString() + " at " + tile.ToString());
    //        OnPlaceArtifactOn(tile);
    //    } else {
    //        OnRemoveArtifact();
    //    }
    //}
    protected override void OnRemoveTileObject(Character removedBy, LocationGridTile removedFrom) {
        base.OnRemoveTileObject(removedBy, removedFrom);
        OnRemoveArtifact();
    }
    protected override void OnPlaceObjectAtTile(LocationGridTile tile) {
        base.OnPlaceObjectAtTile(tile);
        OnPlaceArtifactOn(tile);
    }
    public override string ToString() {
        return Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
    }
    #endregion

    #region Virtuals
    protected virtual void OnPlaceArtifactOn(LocationGridTile tile) {
        hasBeenUsed = true;
        Messenger.AddListener<Settlement>(Signals.SUCCESS_INVASION_AREA, OnSuccessInvadeArea);
    }
    protected virtual void OnRemoveArtifact() {
        Messenger.RemoveListener<Settlement>(Signals.SUCCESS_INVASION_AREA, OnSuccessInvadeArea);
        Debug.Log(GameManager.Instance.TodayLogString() + "Artifact " + name + " has been removed");
    }
    public override void OnInspect(Character inspectedBy) { //, out Log result
        //if (LocalizationManager.Instance.HasLocalizedValue("Artifact", this.GetType().ToString(), "on_inspect")) {
        //    result = new Log(GameManager.Instance.Today(), "Artifact", this.GetType().ToString(), "on_inspect");
        //} else {
        //    result = null;
        //}
    }
    public virtual void LevelUp() {
        level++;
    }
    public virtual void SetLevel(int amount) {
        level = amount;
    } 
    #endregion

    private void OnSuccessInvadeArea(Settlement settlement) {
        hasBeenUsed = false;
        gridTileLocation.structure.RemovePOI(this);
    }

    public void Reset() {
        hasBeenUsed = false;
    }

    #region World Object
    public void Obtain() {
        //- invading a region with an artifact will obtain that artifact for the player
        //UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new artifact: " + this.name + "!", () => PlayerManager.Instance.player.GainArtifact(this, true));
    }
    #endregion

}

public class ArtifactSlot {
    public int level;
    public Artifact artifact;
    public bool isLocked {
        get { return PlayerManager.Instance.player.GetIndexForArtifactSlot(this) >= PlayerManager.Instance.player.maxArtifactSlots; }
    }

    public ArtifactSlot() {
        level = 1;
        artifact = null;
    }

    public void SetArtifact(Artifact artifact) {
        this.artifact = artifact;
        if(this.artifact != null) {
            this.artifact.SetLevel(level);
        }
    }
    
    public void LevelUp() {
        level++;
        level = Mathf.Clamp(level, 1, PlayerManager.MAX_LEVEL_ARTIFACT);
        if (this.artifact != null) {
            this.artifact.SetLevel(level);
        }
        Messenger.Broadcast(Signals.PLAYER_GAINED_ARTIFACT_LEVEL, this);
    }
    public void SetLevel(int amount) {
        level = amount;
        level = Mathf.Clamp(level, 1, PlayerManager.MAX_LEVEL_ARTIFACT);
        if (this.artifact != null) {
            this.artifact.SetLevel(level);
        }
        Messenger.Broadcast(Signals.PLAYER_GAINED_ARTIFACT_LEVEL, this);
    }
}