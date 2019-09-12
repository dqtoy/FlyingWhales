using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheAnvil : BaseLandmark {

    public static readonly string All_Intervention = "All Intervention Abilities";
    public static readonly string All_Summon = "All Summons";
    public static readonly string All_Artifact = "All Artifacts";

    public string upgradeIdentifier { get; private set; }
    public int currentUpgradeTick {
        get {
            return Utilities.GetTicksInBetweenDates(upgradeStartDate, GameManager.Instance.Today());
        }
    }
    public int upgradeDuration { get; private set; }

    private GameDate upgradeStartDate;
    private GameDate dueDate;

    public TheAnvil(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }
    public TheAnvil(HexTile location, SaveDataLandmark data) : base(location, data) { }

    public void SetUpgradeIdentifier(string upgradeIdentifier) {
        this.upgradeIdentifier = upgradeIdentifier;
        upgradeDuration = GetUpgradeDuration(upgradeIdentifier);
    }

    public void StartUpgradeProcess() {
        upgradeStartDate = GameManager.Instance.Today();
        dueDate = GameManager.Instance.Today().AddTicks(upgradeDuration);
        int tickDiff = Utilities.GetTicksInBetweenDates(upgradeStartDate, dueDate);
        SchedulingManager.Instance.AddEntry(dueDate, UpgradeDone, this);
        TimerHubUI.Instance.AddItem("Upgrade " + upgradeIdentifier, upgradeDuration, () => UIManager.Instance.ShowHextileInfo(this.tileLocation));
    }
    private void UpgradeDone() {
        if (upgradeIdentifier == All_Intervention) {
            for (int i = 0; i < PlayerManager.Instance.player.interventionAbilitySlots.Length; i++) {
                PlayerJobActionSlot slot = PlayerManager.Instance.player.interventionAbilitySlots[i];
                slot.LevelUp();
            }
            PlayerUI.Instance.ShowGeneralConfirmation("Upgrade Done", "All Intervention Abilities upgraded!");
        } else if (upgradeIdentifier == All_Summon) {
            for (int i = 0; i < PlayerManager.Instance.player.summonSlots.Count; i++) {
                SummonSlot slot = PlayerManager.Instance.player.summonSlots[i];
                if (slot.summon != null) {
                    slot.LevelUp();
                }
            }
            PlayerUI.Instance.ShowGeneralConfirmation("Upgrade Done", "All Summon Slots upgraded!");
        } else if (upgradeIdentifier == All_Artifact) {
            for (int i = 0; i < PlayerManager.Instance.player.artifactSlots.Count; i++) {
                ArtifactSlot slot = PlayerManager.Instance.player.artifactSlots[i];
                if (slot.artifact != null) {
                    slot.LevelUp();
                }
            }
            PlayerUI.Instance.ShowGeneralConfirmation("Upgrade Done", "All Artifact Slots upgraded!");
        }
        upgradeIdentifier = string.Empty;
        tileLocation.region.assignedMinion.SetAssignedRegion(null);
        tileLocation.region.SetAssignedMinion(null); //reset assigned minion
        UIManager.Instance.areaInfoUI.OnPlayerUpgradeDone();
    }

    #region Static
    public static int GetUpgradeDuration(string upgrade) {
        if (upgrade == All_Intervention) {
            return GameManager.ticksPerHour;
        } else if (upgrade == All_Summon) {
            return GameManager.ticksPerHour;
        } else if (upgrade == All_Artifact) {
            return GameManager.ticksPerHour;
        }
        return 0;
    }
    public static string GetUpgradeDescription(string upgrade) {
        if (upgrade == All_Intervention) {
            return "Upgrade all your intervention abilities by 1 level.";
        } else if (upgrade == All_Summon) {
            return "Upgrade all your summon slots by 1 level.";
        } else if (upgrade == All_Artifact) {
            return "Upgrade all your artifact slots by 1 level.";
        }
        return string.Empty;
    }
    #endregion
}
