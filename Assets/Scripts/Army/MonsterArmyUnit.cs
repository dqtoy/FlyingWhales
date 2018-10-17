using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterArmyUnit : Monster {

    public int armyCount { get; private set; }
    private int armyCap;

    public MonsterArmyUnit(int armyCount) {
        this.armyCap = GetArmyCap();
        this.armyCount = armyCount;
    }

    public void AdjustArmyCount(int adjustment) {
        armyCount -= adjustment;
        if (armyCap == -1) {
            armyCount = Mathf.Max(0, armyCount);
        } else {
            armyCount = Mathf.Clamp(armyCount, 0, armyCap);
        }
    }
    private int GetArmyCap() {
        return -1;
    }
    public int GetProductionCost() {
        return 20;
    }
}
