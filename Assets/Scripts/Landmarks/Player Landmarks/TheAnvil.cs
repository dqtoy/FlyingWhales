using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheAnvil : BaseLandmark {

    public static readonly string All_Intervention = "Improved Spells";
    public static readonly string All_Summon = "Improved Summons";
    public static readonly string All_Artifact = "Improved Artifacts";
    //unimplemented
    public static readonly string Increased_Mana_Capacity = "Increased Mana Capacity";
    public static readonly string Increased_Mana_Regen = "Increased Mana Regen";
    public static readonly string Faster_Portal_Invocation = "Faster Portal Invocation";
    public static readonly string Faster_Invasion = "Faster Invasion";
    public static readonly string Reduce_Spire_Cooldown = "Reduce Spire Cooldown";
    public static readonly string Reduce_Eye_Cooldown = "Reduce Eye Cooldown";

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
        TimerHubUI.Instance.AddItem("Research " + upgradeIdentifier, upgradeDuration, () => UIManager.Instance.ShowHextileInfo(this.tileLocation));
    }
    private void UpgradeDone() {
        if (upgradeIdentifier == All_Intervention) {
            for (int i = 0; i < PlayerManager.Instance.player.interventionAbilitySlots.Length; i++) {
                PlayerJobActionSlot slot = PlayerManager.Instance.player.interventionAbilitySlots[i];
                slot.LevelUp();
            }
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "All Intervention Abilities upgraded!", () => UIManager.Instance.ShowHextileInfo(this.tileLocation));
        } else if (upgradeIdentifier == All_Summon) {
            for (int i = 0; i < PlayerManager.Instance.player.summonSlots.Count; i++) {
                SummonSlot slot = PlayerManager.Instance.player.summonSlots[i];
                slot.LevelUp();
            }
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "All Summon Slots upgraded!", () => UIManager.Instance.ShowHextileInfo(this.tileLocation));
        } else if (upgradeIdentifier == All_Artifact) {
            for (int i = 0; i < PlayerManager.Instance.player.artifactSlots.Count; i++) {
                ArtifactSlot slot = PlayerManager.Instance.player.artifactSlots[i];
                slot.LevelUp();
            }
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "All Artifact Slots upgraded!", () => UIManager.Instance.ShowHextileInfo(this.tileLocation));
        }
        upgradeIdentifier = string.Empty;
        tileLocation.region.assignedMinion.SetAssignedRegion(null);
        tileLocation.region.SetAssignedMinion(null); //reset assigned minion
        UIManager.Instance.regionInfoUI.OnPlayerUpgradeDone();
    }

    public override void DestroyLandmark() {
        base.DestroyLandmark();
        SchedulingManager.Instance.ClearAllSchedulesBy(this);
    }

    #region Static
    public static int GetUpgradeDuration(string upgrade) {
        if (upgrade == All_Intervention) {
            return GameManager.Instance.GetTicksBasedOnHour(8);
        } else if (upgrade == All_Summon) {
            return GameManager.Instance.GetTicksBasedOnHour(8);
        } else if (upgrade == All_Artifact) {
            return GameManager.Instance.GetTicksBasedOnHour(8);
        }
        return GameManager.Instance.GetTicksBasedOnHour(8);
    }
    public static string GetUpgradeDescription(string upgrade) {
        if (upgrade == All_Intervention) {
            return "Upgrade all your intervention abilities by 1 level.";
        } else if (upgrade == All_Summon) {
            return "Upgrade all your summon slots by 1 level.";
        } else if (upgrade == All_Artifact) {
            return "Upgrade all your artifact slots by 1 level.";
        }
        return upgrade;
    }
    #endregion
}
