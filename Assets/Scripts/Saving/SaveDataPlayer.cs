using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataPlayer {

    public int playerFactionID; //TODO: SaveDataFaction
    public int playerAreaID;
    public int threat;

    //public List<Intel> allIntel { get; private set; } //TODO: SaveDataIntel
    //public Minion[] minions { get; private set; } //TODO: SaveDataMinion
    //public Dictionary<SUMMON_TYPE, List<int>> summons; //TODO: SaveDataSummon
    //public Artifact[] artifacts; //TODO: SaveDataArtifact

    //public Minion currentMinionLeader { get; private set; } //TODO: SaveDataMinion

    public int maxSummonSlots; //how many summons can the player have
    public int maxArtifactSlots; //how many artifacts can the player have

    public void Save(Player player) {
        playerFactionID = player.playerFaction.id;
        playerAreaID = player.playerArea.id;
        threat = player.threat;
        maxSummonSlots = player.maxSummonSlots;
        maxArtifactSlots = player.maxArtifactSlots;
    }
}
