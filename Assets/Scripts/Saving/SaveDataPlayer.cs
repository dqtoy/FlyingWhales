﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataPlayer {

    public int playerFactionID;
    public int playerAreaID;
    public int threat;

    public List<SaveDataMinion> minions;
    public List<int> summonIDs;
    public List<SaveDataArtifact> artifacts;

    public int currentMinionLeaderID;

    public int maxSummonSlots;
    public int maxArtifactSlots;

    public void Save(Player player) {
        playerFactionID = player.playerFaction.id;
        playerAreaID = player.playerArea.id;
        threat = player.threat;
        maxSummonSlots = player.maxSummonSlots;
        maxArtifactSlots = player.maxArtifactSlots;

        minions = new List<SaveDataMinion>();
        for (int i = 0; i < player.minions.Length; i++) {
            if(player.minions[i] != null) {
                SaveDataMinion saveDataMinion = new SaveDataMinion();
                saveDataMinion.Save(player.minions[i]);
                minions.Add(saveDataMinion);
            }
        }

        summonIDs = new List<int>();
        foreach(List<Summon> summons in player.summons.Values) {
            for (int i = 0; i < summons.Count; i++) {
                summonIDs.Add(summons[i].id);
            }
        }

        //Sort artifacts by id
        artifacts = new List<SaveDataArtifact>();
        for (int i = 0; i < player.artifacts.Length; i++) {
            if(player.artifacts[i] != null) {
                SaveDataArtifact saveDataArtifact = new SaveDataArtifact();
                saveDataArtifact.Save(player.artifacts[i]);
                SortAddSaveDataArtifact(saveDataArtifact);
            }
        }

        currentMinionLeaderID = player.currentMinionLeader.character.id;
    }
    public void Load() {
        PlayerManager.Instance.InitializePlayer(this);
    }
    private void SortAddSaveDataArtifact(SaveDataArtifact newSaveData) {
        bool hasBeenInserted = false;
        for (int i = 0; i < artifacts.Count; i++) {
            SaveDataArtifact currSaveData = artifacts[i];
            if (newSaveData.id < currSaveData.id) {
                artifacts.Insert(i, newSaveData);
                hasBeenInserted = true;
                break;
            }
        }
        if (!hasBeenInserted) {
            artifacts.Add(newSaveData);
        }
    }
}