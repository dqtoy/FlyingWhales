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

    public void ExtractInterventionAbility(INTERVENTION_ABILITY ability) {
        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained Intervention Ability: " + Utilities.NormalizeStringUpperCaseFirstLetters(ability.ToString()), () => PlayerManager.Instance.player.GainNewInterventionAbility(ability, true));
        PlayerManager.Instance.player.AdjustMana(-PlayerManager.Instance.player.GetManaCostForInterventionAbility(ability));
        tileLocation.region.assignedMinion.SetAssignedRegion(null);
        tileLocation.region.SetAssignedMinion(null);
        Messenger.Broadcast(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);

        //Start Cooldown
        StartCooldown();
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
        Messenger.Broadcast(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    }

   
    //public void StartResearchNewInterventionAbility(INTERVENTION_ABILITY interventionAbility, int currentResearchTick) {
    //    interventionAbilityToResearch = interventionAbility;
    //    if (interventionAbilityToResearch != INTERVENTION_ABILITY.NONE) {
    //        researchDuration = PlayerManager.Instance.allInterventionAbilitiesData[interventionAbilityToResearch].durationInTicks;
    //    } else {
    //        researchDuration = 0;
    //    }
    //    this.currentResearchTick = currentResearchTick;
    //    TimerHubUI.Instance.AddItem("Research for " + Utilities.NormalizeStringUpperCaseFirstLetters(interventionAbilityToResearch.ToString()), researchDuration - currentResearchTick, null);
    //    Messenger.AddListener(Signals.TICK_STARTED, PerTickInterventionAbility);
    //}
    //private void PerTickInterventionAbility() {
    //    currentResearchTick++;
    //    if (currentResearchTick >= researchDuration) {
    //        Messenger.RemoveListener(Signals.TICK_STARTED, PerTickInterventionAbility);
    //        UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained Intervention Ability: " + Utilities.NormalizeStringUpperCaseFirstLetters(interventionAbilityToResearch.ToString()), () => PlayerManager.Instance.player.GainNewInterventionAbility(interventionAbilityToResearch, true));
    //        tileLocation.region.assignedMinion.RemoveInterventionAbilityToResearch(interventionAbilityToResearch);
    //        StopResearch();
    //    }
    //}
    //private void StopResearch() {
    //    interventionAbilityToResearch = INTERVENTION_ABILITY.NONE;
    //    currentResearchTick = 0;
    //    researchDuration = 0;
    //    tileLocation.region.assignedMinion.SetAssignedRegion(null);
    //    tileLocation.region.SetAssignedMinion(null);
    //    Messenger.Broadcast(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    //}

    public override void DestroyLandmark() {
        base.DestroyLandmark();
        //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickInterventionAbility);
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
