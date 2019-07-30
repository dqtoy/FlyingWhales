using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Characters will avoid the area. If any character gets caught within, they will gain Poisoned status effect. Any objects inside the radius are disabled.
public class Miasma_Emitter : Artifact {

    private int range;
    private int duration; //in ticks
    private int currentDuration; //how many ticks has this object been alive

    private List<LocationGridTile> tilesInRange;

    private AOEParticle particle;

    public Miasma_Emitter() : base(ARTIFACT_TYPE.Miasma_Emitter) {
        range = 3;
        duration = 50;
    }
    public Miasma_Emitter(SaveDataArtifact data) : base(data) {
        range = 3;
        duration = 50;
    }
    #region Overrides
    protected override void OnPlaceArtifactOn(LocationGridTile tile) {
        base.OnPlaceArtifactOn(tile);
        currentDuration = 0;
        for (int i = 0; i < tile.parentAreaMap.area.charactersAtLocation.Count; i++) {
            Character currCharacter = tile.parentAreaMap.area.charactersAtLocation[i];
            if (currCharacter.faction != PlayerManager.Instance.player.playerFaction) { //only characters that are not part of the player faction will be terrified by this
                currCharacter.marker.AddTerrifyingObject(this);
            }
        }
        tilesInRange = gridTileLocation.parentAreaMap.GetTilesInRadius(gridTileLocation, range);
        for (int i = 0; i < tilesInRange.Count; i++) {
            LocationGridTile currTile = tilesInRange[i];
            if (currTile.structure != this.gridTileLocation.structure) {
                if (!this.gridTileLocation.structure.structureType.IsOpenSpace() && !currTile.structure.structureType.IsOpenSpace()) {
                    continue; //skip tiles that are not of the same structure and are bot not open space structures.
                }
            }
            if (currTile.objHere is TileObject) {
                TileObject obj = currTile.objHere as TileObject;
                Trait existing = obj.GetNormalTrait("Disabled");
                if (existing != null) {
                    existing.OverrideDuration(0);
                    obj.RefreshTraitExpiry(existing);
                } else {
                    Trait disabled = new Disabled();
                    disabled.OverrideDuration(0);
                    obj.AddTrait(disabled);
                }
            }
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
                obj.RemoveTrait("Disabled");
            }
        }
        for (int i = 0; i < previousTile.parentAreaMap.area.charactersAtLocation.Count; i++) {
            Character currCharacter = previousTile.parentAreaMap.area.charactersAtLocation[i];
            if (currCharacter.faction != PlayerManager.Instance.player.playerFaction) {
                currCharacter.marker.RemoveTerrifyingObject(this);
            }
        }
    }
    #endregion

    private void CheckPerTick() {
        if (currentDuration == duration) {
            gridTileLocation.structure.RemovePOI(this);
        } else {
            currentDuration++;
            List<Character> characters = new List<Character>();
            for (int i = 0; i < tilesInRange.Count; i++) {
                LocationGridTile currTile = tilesInRange[i];
                characters.AddRange(currTile.charactersHere);
            }

            for (int i = 0; i < characters.Count; i++) {
                Character currCharacter = characters[i];
                currCharacter.AddTrait(new Poisoned());
            }
        }
    }

    private void OnTileObjectPlaced(TileObject obj, LocationGridTile tile) {
        if (tilesInRange.Contains(tile)) {
            Trait existing = obj.GetNormalTrait("Disabled");
            if (existing != null) {
                existing.OverrideDuration(0);
                obj.RefreshTraitExpiry(existing);
            } else {
                Trait disabled = new Disabled();
                disabled.OverrideDuration(0);
                obj.AddTrait(disabled);
            }
        }
    }

}
