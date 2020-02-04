using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheSpire : BaseLandmark {
    public int currentCooldownTick { get; private set; }
    public int cooldownDuration { get; private set; }
    public bool isInCooldown {
        get { return currentCooldownTick < cooldownDuration; }
    }

    public TheSpire(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        cooldownDuration = GameManager.Instance.GetTicksBasedOnHour(4);
        currentCooldownTick = cooldownDuration;
    }

    public TheSpire(HexTile location, SaveDataLandmark data) : base(location, data) {
        cooldownDuration = GameManager.Instance.GetTicksBasedOnHour(4);
    }
    public void LoadSavedData(SaveDataTheSpire data) {
        if (data.currentCooldownTick < cooldownDuration) {
            StartCooldown();
        }
        currentCooldownTick = data.currentCooldownTick;
    }

    public void ExtractInterventionAbility(SPELL_TYPE ability) {
        //UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained Intervention Ability: " + Utilities.NormalizeStringUpperCaseFirstLetters(ability.ToString()), () => PlayerManager.Instance.player.GainNewInterventionAbility(ability, true));
        PlayerManager.Instance.player.GainNewInterventionAbility(ability, true);
        PlayerManager.Instance.player.AdjustMana(-PlayerManager.Instance.player.GetManaCostForInterventionAbility(ability));
        Minion assignedMinion = tileLocation.region.assignedMinion;
        assignedMinion.AdjustSpellExtractionCount(1);
        assignedMinion.SetAssignedRegion(null);
        tileLocation.region.SetAssignedMinion(null);
        Messenger.Broadcast(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);

        //Start Cooldown
        StartCooldown();

        CheckForMinionDeath(assignedMinion, ability);
    }
    private void StartCooldown() {
        currentCooldownTick = 0;
        Messenger.AddListener(Signals.TICK_ENDED, PerTickCooldown);
    }
    private void PerTickCooldown() {
        currentCooldownTick++;
        if (currentCooldownTick == cooldownDuration) {
            //coodlown done
            StopCooldown();
        }
    }
    private void StopCooldown() {
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickCooldown);
        Messenger.Broadcast(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    }

    #region Override
    public override void DestroyLandmark() {
        StopCooldown();
        base.DestroyLandmark();
    }
    #endregion

    private void CheckForMinionDeath(Minion minion, SPELL_TYPE ability) {
        //first extraction: 0% chance to die
        //second extraction: 10 % chance to die
        //third and future extractions: depends on the Spell's tier level. Tier 1: 35%, Tier 2: 25%, Tier 3: 15%
        string summary = minion.character.name + " will roll for death after extraction. Extraction count is: " + minion.spellExtractionCount.ToString();
        int deathChance = 0;
        if (minion.spellExtractionCount == 2) {
            deathChance = 10;
        } else if (minion.spellExtractionCount >= 3) {
            int tier = PlayerManager.Instance.GetSpellTier(ability);
            if (tier == 1) {
                deathChance = 35;
            } else if (tier == 2) {
                deathChance = 25;
            } else {
                deathChance = 15;
            }
        }
        int roll = Random.Range(0, 100);
        summary += "\nDeath Chance is: " + deathChance.ToString();
        summary += "\nRoll is: " + roll.ToString();
        if (roll < deathChance) {
            summary += minion.character.name + " died.";
            minion.Death("SpellExtraction");
        }
        Debug.Log(GameManager.Instance.TodayLogString() + summary);
    }
}

public class SaveDataTheSpire: SaveDataLandmark {
    public int currentCooldownTick;

    public override void Save(BaseLandmark landmark) {
        base.Save(landmark);
        TheSpire spire = landmark as TheSpire;
        currentCooldownTick = spire.currentCooldownTick;
    }
    public override void LoadSpecificLandmarkData(BaseLandmark landmark) {
        base.LoadSpecificLandmarkData(landmark);
        TheSpire spire = landmark as TheSpire;
        spire.LoadSavedData(this);
    }
}
