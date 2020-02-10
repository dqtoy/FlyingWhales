using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using Traits;

//Characters will avoid the settlement. If any character gets caught within, they will gain Poisoned status effect. Any objects inside the radius are disabled.
public class MiasmaEmitter : Artifact {

    private int range;
    private int duration; //in ticks
    public int currentDuration { get; private set; } //how many ticks has this object been alive

    private List<LocationGridTile> tilesInRange;

    private AOEParticle particle;

    public MiasmaEmitter() : base(ARTIFACT_TYPE.Miasma_Emitter) {
        range = 1;
        duration = 50;
    }   
    //public Miasma_Emitter(SaveDataArtifactSlot data) : base(data) {
    //    range = 1;
    //    duration = 50;
    //}
    public MiasmaEmitter(SaveDataArtifact data) : base(data) {
        range = 1;
        duration = 50;
    }
    #region Overrides
    protected override void OnPlaceArtifactOn(LocationGridTile tile) {
        base.OnPlaceArtifactOn(tile);
        currentDuration = 0;
//        for (int i = 0; i < tile.parentMap.settlement.charactersAtLocation.Count; i++) {
//            Character currCharacter = tile.parentMap.settlement.charactersAtLocation[i];
//            if (!currCharacter.faction.isPlayerFaction) { //only characters that are not part of the player faction will be terrified by this
//                currCharacter.marker.AddTerrifyingObject(this);
//            }
//        }
        tilesInRange = gridTileLocation.parentMap.GetTilesInRadius(gridTileLocation, range);
        for (int i = 0; i < tilesInRange.Count; i++) {
            LocationGridTile currTile = tilesInRange[i];
            if (currTile.structure != this.gridTileLocation.structure) {
                if (!this.gridTileLocation.structure.structureType.IsOpenSpace() && !currTile.structure.structureType.IsOpenSpace()) {
                    continue; //skip tiles that are not of the same structure and are bot not open space structures.
                }
            }
            //if (currTile.objHere is TileObject) {
            //    TileObject obj = currTile.objHere as TileObject;
            //    Trait existing = obj.traitContainer.GetNormalTrait<Trait>("Disabled");
            //    //if (existing != null) {
            //    //    existing.OverrideDuration(0);
            //    //    obj.RefreshTraitExpiry(existing);
            //    //} else {
            //    //    Trait disabled = new Disabled();
            //    //    disabled.OverrideDuration(0);
            //    //    obj.traitContainer.AddTrait(obj, disabled);
            //    //}
            //}
        }
        particle = GameManager.Instance.CreateAOEEffectAt(tile, range);
        Messenger.AddListener(Signals.TICK_ENDED, CheckPerTick);
        Messenger.AddListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
    }
    protected override void OnRemoveArtifact() {
        base.OnRemoveArtifact();
        Messenger.RemoveListener(Signals.TICK_ENDED, CheckPerTick);
        Messenger.RemoveListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED, OnTileObjectPlaced);
        for (int i = 0; i < tilesInRange.Count; i++) {
            LocationGridTile currTile = tilesInRange[i];
            if (currTile.objHere is TileObject) {
                TileObject obj = currTile.objHere as TileObject;
                obj.traitContainer.RemoveTrait(obj, "Disabled");
            }
        }
//        for (int i = 0; i < previousTile.parentMap.settlement.charactersAtLocation.Count; i++) {
//            Character currCharacter = previousTile.parentMap.settlement.charactersAtLocation[i];
//            if (!currCharacter.faction.isPlayerFaction) {
//                currCharacter.marker.RemoveTerrifyingObject(this);
//            }
//        }
        ObjectPoolManager.Instance.DestroyObject(particle);
    }
    public override void LevelUp() {
        base.LevelUp();
        range++;
    }
    #endregion

    private void CheckPerTick() {
        if (gridTileLocation == null) { //this is needed because this can still be executed when the object was destroyed on the same frame that this ticks
            return;
        }
        if (currentDuration == duration) {
            gridTileLocation.structure.RemovePOI(this);
        } else {
            currentDuration++;
            List<Character> characters = new List<Character>();
            for (int i = 0; i < tilesInRange.Count; i++) {
                LocationGridTile currTile = tilesInRange[i];
                characters.AddRange(currTile.charactersHere);
            }

            //for (int i = 0; i < characters.Count; i++) {
            //    Character currCharacter = characters[i];
            //    currCharacter.AddTrait(new Poisoned());
            //}
        }
    }

    private void OnTileObjectPlaced(TileObject obj, LocationGridTile tile) {
        if (tilesInRange.Contains(tile)) {
            //Trait existing = obj.GetNormalTrait<Trait>("Disabled");
            //if (existing != null) {
            //    existing.OverrideDuration(0);
            //    obj.RefreshTraitExpiry(existing);
            //} else {
            //    Trait disabled = new Disabled();
            //    disabled.OverrideDuration(0);
            //    obj.AddTrait(disabled);
            //}
        }
    }

    public void SetCurrentDuration(int amount) {
        currentDuration = amount;
    }
}

public class SaveDataMiasmaEmitter : SaveDataArtifact {
    public int currentDuration;

    public override void Save(TileObject tileObject) {
        base.Save(tileObject);
        MiasmaEmitter obj = tileObject as MiasmaEmitter;
        currentDuration = obj.currentDuration;
    }

    public override TileObject Load() {
        MiasmaEmitter obj = base.Load() as MiasmaEmitter;
        return obj;
    }
    public override void LoadAfterLoadingAreaMap() {
        MiasmaEmitter obj = loadedTileObject as MiasmaEmitter;
        obj.SetCurrentDuration(currentDuration);
        base.LoadAfterLoadingAreaMap();
    }
}