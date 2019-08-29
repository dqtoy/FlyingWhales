using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;

[System.Serializable]
public class SaveDataRegion {
    public int id;
    public string name;
    public List<int> tileIDs;
    public int coreTileID;
    public int ticksInInvasion;
    public ColorSave regionColor;

    public int invadingMinionID;

    public void Save(Region region) {
        id = region.id;
        name = region.name;

        tileIDs = new List<int>();
        for (int i = 0; i < region.tiles.Count; i++) {
            tileIDs.Add(region.tiles[i].id);
        }

        coreTileID = region.coreTile.id;
        ticksInInvasion = region.ticksInInvasion;
        regionColor = region.regionColor;

        if (region.invadingMinion != null) {
            invadingMinionID = region.invadingMinion.character.id;
        } else {
            invadingMinionID = -1;
        }
    }

    public Region Load() {
        Region region = new Region(this);
        for (int i = 0; i < tileIDs.Count; i++) {
            region.AddTile(GridMap.Instance.hexTiles[tileIDs[i]]);
        }

        if(ticksInInvasion > 0) {
            //TODO: what to do if saved in the middle of invading a tile
        }
        return region;
    }

    public Minion LoadInvadingMinion() {
        if(invadingMinionID != -1) {
            Minion minion = CharacterManager.Instance.GetCharacterByID(invadingMinionID).minion;
            return minion;
        }
        return null;
    }
}
