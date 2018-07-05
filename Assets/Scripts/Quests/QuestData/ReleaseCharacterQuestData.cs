using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseCharacterQuestData : CharacterQuestData {

    public enum Gain_Power_Type {
        None,
        Mentor,
        Equipment,
        Hunt
    }

    public ECS.Character targetCharacter { get; private set; }
    public int requiredPower { get; private set; }
    public Gain_Power_Type gainPowerType { get; private set; }
    public List<Vector3> vectorPathToTarget { get; private set; }

    public ReleaseCharacterQuestData(Quest parentQuest, ECS.Character owner, ECS.Character targetCharacter) : base(parentQuest, owner) {
        this.targetCharacter = targetCharacter;
    }

    public void UpdateVectorPath() { //TODO: Change this to somehow get the path of immediately
        PathfindingManager.Instance.GetPath(_owner.currLocation, targetCharacter.currLocation, OnVectorPathComputed);
    }
    private void OnVectorPathComputed(List<Vector3> path) {
        vectorPathToTarget = path;
    }

    public void SetRequiredPower(int power) {
        requiredPower = power;
    }
    public void SetGainPowerType(Gain_Power_Type gainPowerType) {
        this.gainPowerType = gainPowerType;
    }
}
