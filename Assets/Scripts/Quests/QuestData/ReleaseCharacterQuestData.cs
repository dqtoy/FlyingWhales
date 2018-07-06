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
    public float requiredPower { get; private set; }
    public Gain_Power_Type gainPowerType { get; private set; }
    public List<Vector3> vectorPathToTarget { get; private set; }
    public List<HexTile> tilePathToTarget { get; private set; }

    public bool isWaitingForPath { get { return _owner.party.icon.pathfinder.isWaitingForPathCalculation; } }

    private HexTile targetTile;

    public ReleaseCharacterQuestData(Quest parentQuest, ECS.Character owner, ECS.Character targetCharacter) : base(parentQuest, owner) {
        this.targetCharacter = targetCharacter;
        targetTile = targetCharacter.currLocation;
        requiredPower = 0f;
    }

    #region overrides
    public override IEnumerator SetupValuesCoroutine() {
        //Debug.Log(_owner.name + " setting up values for release character quest");
        UpdateVectorPath();
        while (isWaitingForPath) {
            yield return null;
        }
        tilePathToTarget =  _owner.party.icon.ConvertToTilePath(vectorPathToTarget);
        //for (int i = 0; i < tilePathToTarget.Count; i++) {
        //    tilePathToTarget[i].HighlightTile(Color.gray, 128f/255f);
        //    yield return null;
        //}
        //Debug.Log(_owner.name + " done setting up values for release character quest");
    }
    #endregion

    public void UpdateVectorPath() {
        _owner.party.icon.GetVectorPath(targetTile, OnVectorPathComputed);
    }
    private void OnVectorPathComputed(List<Vector3> path) {
        vectorPathToTarget = path;
    }

    public void SetRequiredPower(float power) {
        requiredPower = power;
    }
    public void SetGainPowerType(Gain_Power_Type gainPowerType) {
        this.gainPowerType = gainPowerType;
    }

    public bool HasHostilesInPath() {
        for (int i = 0; i < tilePathToTarget.Count; i++) {
            HexTile currTile = tilePathToTarget[i];
            BaseLandmark landmarkOnTile = currTile.landmarkOnTile;
            if (landmarkOnTile != null && landmarkOnTile.charactersAtLocation.Count > 0) {
                if (landmarkOnTile.HasHostileCharactersWith(_owner)) {
                    return true;
                }
            }
        }
        return false;
    }

    public IParty GetFirstHostileInPath() {
        for (int i = 0; i < tilePathToTarget.Count; i++) {
            HexTile currTile = tilePathToTarget[i];
            BaseLandmark landmarkOnTile = currTile.landmarkOnTile;
            if (landmarkOnTile != null && landmarkOnTile.charactersAtLocation.Count > 0) {
                for (int j = 0; j < landmarkOnTile.charactersAtLocation.Count; j++) {
                    IParty currParty = landmarkOnTile.charactersAtLocation[j];
                    if (currParty is MonsterParty) {
                        return currParty;
                    }
                }
            }
        }
        return null;
    }
}
