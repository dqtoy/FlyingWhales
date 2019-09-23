using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ankh_Of_Anubis : Artifact {

    private bool isActivated;

    private int range;
    private int deathChance; //out of 100%
    private int duration; //in ticks
    private int currentDuration; //how many ticks has this object been alive

    private AOEParticle particle;

    public Ankh_Of_Anubis() : base(ARTIFACT_TYPE.Ankh_Of_Anubis) {
        range = 1;
        deathChance = 60;
        duration = 50;
    }
    //public Ankh_Of_Anubis(SaveDataArtifactSlot data) : base(data) {
    //    range = 1;
    //    deathChance = 60;
    //    duration = 20;
    //}
    public Ankh_Of_Anubis(SaveDataArtifact data) : base(data) {
        range = 1;
        deathChance = 60;
        duration = 20;
    }

    #region Overrides
    //protected override void OnPlaceArtifactOn(LocationGridTile tile) {
    //    base.OnPlaceArtifactOn(tile);
    //    Activate();
    //}
    protected override void OnRemoveArtifact() {
        base.OnRemoveArtifact();
        Messenger.RemoveListener(Signals.TICK_ENDED, CheckPerTick);
        ObjectPoolManager.Instance.DestroyObject(particle.gameObject);
    }
    public override void LevelUp() {
        base.LevelUp();
        duration += 10;
    }
    public override void OnInspect(Character inspectedBy, out Log result) {
        base.OnInspect(inspectedBy, out result);
        if (!isActivated) {
            Activate();
        }
    }
    #endregion

    private void Activate() {
        isActivated = true;
        currentDuration = 0;
        Messenger.AddListener(Signals.TICK_ENDED, CheckPerTick);
        particle = GameManager.Instance.CreateAOEEffectAt(tile, range);
    }

    private void CheckPerTick() {
        if (gridTileLocation == null) { //this is needed because this can still be executed when the object was destroyed on the same frame that this ticks
            return;
        }
        if (currentDuration == duration) {
            gridTileLocation.structure.RemovePOI(this);
        } else {
            currentDuration++;
            List<LocationGridTile> tilesInRange = gridTileLocation.parentAreaMap.GetTilesInRadius(gridTileLocation, range);
            List<Character> characters = new List<Character>();
            for (int i = 0; i < tilesInRange.Count; i++) {
                LocationGridTile currTile = tilesInRange[i];
                characters.AddRange(currTile.charactersHere);
            }

            for (int i = 0; i < characters.Count; i++) {
                Character currCharacter = characters[i];
                if (Random.Range(0, 100) > currCharacter.speed) {
                    if (currCharacter.GetNormalTrait("Encumbered") == null) {
                        currCharacter.AddTrait("Encumbered");
                    } else {
                        //roll for death
                        if (Random.Range(0, 100) < deathChance) {
                            currCharacter.Death("Ankh Of Anubis");
                        }
                    }
                }
            }
        }
    }

}
