using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataPlayer {

    public int playerFactionID;
    public int playerAreaID;
    public int threat;

    public List<SaveDataMinion> minions;
    public List<SaveDataSummon> summonSlots;
    public List<SaveDataArtifact> artifactSlots;
    public List<SaveDataInterventionAbility> interventionAbilitySlots;

    public int currentMinionLeaderID;

    public int maxSummonSlots;
    public int maxArtifactSlots;

    public int invadingRegionID;

    public int currentInterventionAbilityTimerTick;
    public int currentNewInterventionAbilityCycleIndex;
    public INTERVENTION_ABILITY interventionAbilityToResearch;

    public void Save(Player player) {
        playerFactionID = player.playerFaction.id;
        playerAreaID = player.playerArea.id;
        threat = player.threat;
        maxSummonSlots = player.maxSummonSlots;
        maxArtifactSlots = player.maxArtifactSlots;

        minions = new List<SaveDataMinion>();
        for (int i = 0; i < player.minions.Count; i++) {
            SaveDataMinion saveDataMinion = new SaveDataMinion();
            saveDataMinion.Save(player.minions[i]);
            minions.Add(saveDataMinion);
        }

        summonSlots = new List<SaveDataSummon>();
        for (int i = 0; i < player.summonSlots.Length; i++) {
            SaveDataSummon data = new SaveDataSummon();
            data.Save(player.summonSlots[i]);
            summonSlots.Add(data);
        }

        artifactSlots = new List<SaveDataArtifact>();
        for (int i = 0; i < player.artifactSlots.Length; i++) {
            SaveDataArtifact saveDataArtifact = new SaveDataArtifact();
            saveDataArtifact.Save(player.artifactSlots[i]);
            artifactSlots.Add(saveDataArtifact);
        }

        //currentMinionLeaderID = player.currentMinionLeader.character.id;

        interventionAbilitySlots = new List<SaveDataInterventionAbility>();
        for (int i = 0; i < player.interventionAbilitySlots.Length; i++) {
            SaveDataInterventionAbility saveDataInterventionAbility = new SaveDataInterventionAbility();
            saveDataInterventionAbility.Save(player.interventionAbilitySlots[i]);
            interventionAbilitySlots.Add(saveDataInterventionAbility);
        }

        if(player.isInvadingRegion) {
            invadingRegionID = player.invadingRegion.id;
        } else {
            invadingRegionID = -1;
        }

        interventionAbilityToResearch = player.interventionAbilityToResearch;
        currentInterventionAbilityTimerTick = player.currentInterventionAbilityTimerTick;
        currentNewInterventionAbilityCycleIndex = player.currentNewInterventionAbilityCycleIndex;
    }
    public void Load() {
        PlayerManager.Instance.InitializePlayer(this);
    }
    public void LoadInvasion(Save save) {
        if (invadingRegionID != -1) {
            SaveDataRegion invadingRegionSave = null;
            for (int i = 0; i < save.regionSaves.Count; i++) {
                if (save.regionSaves[i].id == invadingRegionID) {
                    invadingRegionSave = save.regionSaves[i];
                    break;
                }
            }

            Region region = GridMap.Instance.GetRegionByID(invadingRegionSave.id);
            region.LoadInvasion(invadingRegionSave.LoadInvadingMinion(), invadingRegionSave.ticksInInvasion);
        }
    }
    private void SortAddSaveDataArtifact(SaveDataArtifact newSaveData) {
        bool hasBeenInserted = false;
        for (int i = 0; i < artifactSlots.Count; i++) {
            SaveDataArtifact currSaveData = artifactSlots[i];
            if (newSaveData.id < currSaveData.id) {
                artifactSlots.Insert(i, newSaveData);
                hasBeenInserted = true;
                break;
            }
        }
        if (!hasBeenInserted) {
            artifactSlots.Add(newSaveData);
        }
    }
}
