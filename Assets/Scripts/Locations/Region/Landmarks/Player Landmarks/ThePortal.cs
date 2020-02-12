using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actionables;
using Inner_Maps;
using UnityEngine;

public class ThePortal : BaseLandmark {
    public int currentMinionToSummonIndex { get; private set; }
    public int currentSummonTick { get; private set; }
    public int currentSummonDuration { get; private set; }
    public string minionSummonClassName { get; private set; }

    private List<Character> validMinions;

    public ThePortal(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        currentMinionToSummonIndex = -1;
        validMinions = new List<Character>();
        
        PlayerAction summonMinion = new PlayerAction(PlayerManager.Summon_Minion_Action, 
            () => PlayerManager.Instance.player.mana >= EditableValuesManager.Instance.summonMinionManaCost, 
            SummonMinion);
        location.AddPlayerAction(summonMinion);
    }

    public ThePortal(HexTile location, SaveDataLandmark data) : base(location, data) {
        currentMinionToSummonIndex = -1;
    }

    // public void LoadSummonMinion(SaveDataThePortal data) {
    //     if (data.currentMinionToSummonIndex != -1) {
    //         StartSummon(data.currentMinionToSummonIndex, data.currentSummonTick, data.currentSummonDuration, data.currentSummonClassName);
    //     } else {
    //         currentMinionToSummonIndex = data.currentMinionToSummonIndex;
    //         currentSummonTick = data.currentSummonTick;
    //         currentSummonDuration = data.currentSummonDuration;
    //     }
    // }
    // public void StartSummon(int minionToSummonIndex, int currentSummonTick, int summonDuration = 0, string summonClassName = "") {
    //     currentMinionToSummonIndex = minionToSummonIndex;
    //     this.currentSummonTick = currentSummonTick;
    //     if(summonDuration != 0) {
    //         currentSummonDuration = summonDuration;
    //     } else {
    //         currentSummonDuration = LandmarkManager.SUMMON_MINION_DURATION;
    //         if(tileLocation.region.assignedMinion != null) {
    //             int speedUpDuration = Mathf.CeilToInt(LandmarkManager.SUMMON_MINION_DURATION * 0.25f);
    //             currentSummonDuration -= speedUpDuration;
    //         }
    //     }
    //     if (string.IsNullOrEmpty(summonClassName)) {
    //         minionSummonClassName = PlayerManager.Instance.player.minionsToSummon[currentMinionToSummonIndex].className;
    //     } else {
    //         minionSummonClassName = summonClassName;
    //     }
    //     TimerHubUI.Instance.AddItem("Summoning " + minionSummonClassName + " Minion", currentSummonDuration - currentSummonTick, null);
    //     Messenger.AddListener(Signals.TICK_STARTED, PerTickSummon);
    //     Messenger.Broadcast<Region>(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    // }
    // private void PerTickSummon() {
    //     currentSummonTick++;
    //     if (currentSummonTick >= currentSummonDuration) {
    //         Messenger.RemoveListener(Signals.TICK_STARTED, PerTickSummon);
    //         SummonMinion();
    //         StopSummon();
    //     }
    // }
    // private void SummonMinion() {
        // UnsummonedMinionData minionData = PlayerManager.Instance.player.minionsToSummon[currentMinionToSummonIndex];
        // Minion minion = PlayerManager.Instance.player.CreateNewMinion(minionData.className, RACE.DEMON, false);
        // minion.character.SetName(minionData.minionName);
        // minion.SetCombatAbility(minionData.combatAbility);
        // minion.SetRandomResearchInterventionAbilities(minionData.interventionAbilitiesToResearch);
        //
        // if (PlayerManager.Instance.player.minions.Count < Player.MAX_MINIONS) {
        //     PlayerManager.Instance.player.AddMinion(minion);
        //     UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new Minion!", null);
        // } else {
        //     UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), "Gained new Minion!", () => PlayerManager.Instance.player.AddMinion(minion, true));
        // }
        //
        // PlayerManager.Instance.player.GenerateMinionsToSummon();
    // }
    // private void StopSummon() {
    //     currentSummonTick = 0;
    //     currentSummonDuration = 0;
    //     currentMinionToSummonIndex = -1;
    //     if (tileLocation.region.assignedMinion != null) {
    //         tileLocation.region.assignedMinion.SetAssignedRegion(null);
    //         tileLocation.region.SetAssignedMinion(null);
    //     }
    //     Messenger.Broadcast(Signals.REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    // }
    
    private void SummonMinion() {
        validMinions.Clear();
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            Minion minion = PlayerManager.Instance.player.minions[i];
            if(minion.character.currentHP >= minion.character.maxHP && minion.character.gridTileLocation == null && !minion.character.isDead) {
                if (PlayerManager.Instance.player.archetype.CanSummonMinion(minion)) {
                    validMinions.Add(minion.character);
                }
            }
        }
        //List<Character> validMinions = new List<Character>(PlayerManager.Instance.player.minions
        //    .Where(x => x.character.currentHP >= x.character.maxHP && x.character.gridTileLocation == null)
        //    .Select(x => x.character));
        UIManager.Instance.ShowClickableObjectPicker(validMinions,
            OnSelectMinion, null, CanSummonMinion, 
            "Choose Minion to Summon", showCover: true);
    }
    private bool CanSummonMinion(Character character) {
        //return character.currentHP >= character.maxHP && character.gridTileLocation == null;
        return true;
    }
    private void OnSelectMinion(object obj) {
        Character character = obj as Character;
        character.minion.Summon(this);
        UIManager.Instance.HideObjectPicker();
    }
}

public class SaveDataThePortal : SaveDataLandmark {
    public int currentMinionToSummonIndex;
    public int currentSummonTick;
    public int currentSummonDuration;
    public string currentSummonClassName;

    public override void Save(BaseLandmark landmark) {
        base.Save(landmark);
        ThePortal portal = landmark as ThePortal;
        currentMinionToSummonIndex = portal.currentMinionToSummonIndex;
        currentSummonTick = portal.currentSummonTick;
        currentSummonDuration = portal.currentSummonDuration;
        currentSummonClassName = portal.minionSummonClassName;
    }
    public override void LoadSpecificLandmarkData(BaseLandmark landmark) {
        base.LoadSpecificLandmarkData(landmark);
        ThePortal portal = landmark as ThePortal;
        // portal.LoadSummonMinion(this);
    }
}
