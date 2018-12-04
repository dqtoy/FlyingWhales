using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class RaceSetting {
    public RACE race;
    public int baseAttackPower;
    public int baseSpeed;
    public int baseHP;
    public int[] hpPerLevel;
    public int[] attackPerLevel;
    public string[] traitNames;

    public RaceSetting CreateNewCopy() {
        RaceSetting newRaceSetting = new RaceSetting();
        newRaceSetting.race = this.race;
        newRaceSetting.baseAttackPower = this.baseAttackPower;
        newRaceSetting.baseSpeed = this.baseSpeed;
        newRaceSetting.baseHP = this.baseHP;
        newRaceSetting.hpPerLevel = this.hpPerLevel;
        newRaceSetting.attackPerLevel = this.attackPerLevel;
        newRaceSetting.traitNames = this.traitNames;
        return newRaceSetting;
    }

    public void SetDataFromRacePanelUI() {
        this.race = (RACE) System.Enum.Parse(typeof(RACE), RacePanelUI.Instance.raceOptions.options[RacePanelUI.Instance.raceOptions.value].text);
        this.baseAttackPower = int.Parse(RacePanelUI.Instance.baseAttackInput.text);
        this.baseSpeed = int.Parse(RacePanelUI.Instance.baseSpeedInput.text);
        this.baseHP = int.Parse(RacePanelUI.Instance.baseHPInput.text);
        this.hpPerLevel = RacePanelUI.Instance.hpPerLevel.ToArray();
        this.attackPerLevel = RacePanelUI.Instance.attackPerLevel.ToArray();
        this.traitNames = RacePanelUI.Instance.traitNames.ToArray();
    }
}