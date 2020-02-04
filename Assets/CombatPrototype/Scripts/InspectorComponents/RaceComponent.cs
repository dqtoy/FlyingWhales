using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RaceComponent : MonoBehaviour {
	public RACE race;
    public int baseAttackPower;
    public int baseSpeed;
    public int baseHP;
    public int[] hpPerLevel;
    public int[] attackPerLevel;
    //public int baseStr;
    //public int baseInt;
    //public int baseAgi;
    //public int baseHP;
    //public int statAllocationPoints;
    //public int strWeightAllocation;
    //public int intWeightAllocation;
    //public int agiWeightAllocation;
    //public int hpWeightAllocation;
    public int restRegenAmount;
    public List<ATTRIBUTE> tags;
}