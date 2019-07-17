using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ankh_Of_Anubis : Artifact {

    private int range;
    private int deathChance; //out of 100%
    private int duration; //in ticks
    private int currentDuration; //how many ticks has this object been alive

    public Ankh_Of_Anubis() : base(ARTIFACT_TYPE.Ankh_Of_Anubis) {
        range = 3;
        deathChance = 60;
        duration = 50;
    }

    #region Overrides
    protected override void OnPlaceArtifactOn(LocationGridTile tile) {
        base.OnPlaceArtifactOn(tile);
        currentDuration = 0;
        Messenger.AddListener(Signals.TICK_ENDED, CheckPerTick);
    }
    protected override void OnRemoveArtifact() {
        base.OnRemoveArtifact();
        Messenger.RemoveListener(Signals.TICK_ENDED, CheckPerTick);
    }
    #endregion

    private void CheckPerTick() {
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
