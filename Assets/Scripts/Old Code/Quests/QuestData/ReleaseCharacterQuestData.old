using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseCharacterQuestData : CharacterQuestData {

    //public enum Gain_Power_Type {
    //    None,
    //    Mentor,
    //    Equipment,
    //    Hunt
    //}

    //public float requiredPower { get; private set; }
    //public Gain_Power_Type gainPowerType { get; private set; }
    public List<Vector3> vectorPathToTarget { get; private set; }
    public List<HexTile> tilePathToTarget { get; private set; }
    //public List<Character> elligibleMentors { get; private set; }
    //public int idleActionsInARow { get; private set; }
    //public int huntedCharactersCount { get; private set; }
    //public IParty huntedParty { get; private set; }

    //public bool isWaitingForPath { get { return _owner.currentParty.icon.pathfinder.isWaitingForPathCalculation; } }

    //private int huntExp = 500;
    private HexTile targetTile;

    public ReleaseCharacterQuestData(Quest parentQuest, Character owner, ECS.Character targetCharacter) : base(parentQuest, owner) {
        targetTile = (parentQuest as ReleaseCharacterQuest).targetCharacter.currLocation;
        //requiredPower = 0f;
    }

    #region overrides
    public override IEnumerator SetupValuesCoroutine() {
        //UpdateVectorPath();
        //while (isWaitingForPath) {
        yield return null;
        //}
        //tilePathToTarget = _owner.currentParty.icon.ConvertToTilePath(vectorPathToTarget);
    }
    #endregion

    public void UpdateVectorPath() {
        //_owner.currentParty.icon.GetVectorPath(targetTile, OnVectorPathComputed);
    }
    private void OnVectorPathComputed(List<Vector3> path) {
        vectorPathToTarget = path;
    }
    //public void SetRequiredPower(float power) {
    //    requiredPower = power;
    //}
    //public void SetGainPowerType(Gain_Power_Type gainPowerType) {
    //    this.gainPowerType = gainPowerType;
    //}
    //public void SetElligibleMentors(List<Character> elligibleMentors) {
    //    this.elligibleMentors = elligibleMentors;
    //}

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

    public NewParty GetFirstHostileInPath() {
        for (int i = 0; i < tilePathToTarget.Count; i++) {
            HexTile currTile = tilePathToTarget[i];
            BaseLandmark landmarkOnTile = currTile.landmarkOnTile;
            if (landmarkOnTile != null && landmarkOnTile.charactersAtLocation.Count > 0) {
                for (int j = 0; j < landmarkOnTile.charactersAtLocation.Count; j++) {
                    NewParty currParty = landmarkOnTile.charactersAtLocation[j];
                    if (currParty is MonsterParty) {
                        return currParty;
                    }
                }
            }
        }
        return null;
    }

    //public void OnChooseHuntCharacter(IParty partyToHunt) {
    //    huntedParty = partyToHunt;
    //    Messenger.AddListener<CharacterParty, CharacterAction>(Signals.ACTION_SUCCESS, OnHuntCharacterDone);
    //}

    //public void OnDoIdleActionFromHunt() {
    //    idleActionsInARow++;
    //}
    //public void ResetIdleActions() {
    //    idleActionsInARow = 0;
    //}
    //public void OnHuntCharacterDone(CharacterParty succeededParty, CharacterAction succeededAction) {
    //    if (huntedParty.icharacterObject.OwnsAction(succeededAction) && succeededParty.id == _owner.party.id) {
    //        Messenger.RemoveListener<CharacterParty, CharacterAction>(Signals.ACTION_SUCCESS, OnHuntCharacterDone);
    //        Debug.Log(_owner.name + " successfully hunted " + huntedParty.name);
    //        huntedParty = null;
    //        huntedCharactersCount++;
    //        //After successfully killing 5, gain ? Experience and set Gain Power Type to None
    //        if (huntedCharactersCount == 5) {
    //            huntedCharactersCount = 0;
    //            _owner.AdjustExperience(huntExp);
    //            SetGainPowerType(Gain_Power_Type.None);
    //        }
    //    }
    //}
}
