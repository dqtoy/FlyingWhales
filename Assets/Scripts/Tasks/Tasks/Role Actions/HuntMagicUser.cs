using UnityEngine;
using System.Collections;
using ECS;
using System.Collections.Generic;

public class HuntMagicUser : CharacterTask {

    private ECS.Character _targetCharacter;
    private BaseLandmark _targetLandmark;

    public HuntMagicUser(TaskCreator createdBy) : base(createdBy, TASK_TYPE.HUNT_MAGIC_USER) {
        SetStance(STANCE.STEALTHY);
        _specificTargetClassification = "character";
        _needsSpecificTarget = true;
        _filters = new TaskFilter[] {
            new MustBeClass(new List<CHARACTER_CLASS>(){ CHARACTER_CLASS.ARCANIST, CHARACTER_CLASS.MAGE, CHARACTER_CLASS.BATTLEMAGE,}),
        };
    }

    private ECS.Character GetTargetCharacter(ECS.Character hunter) {
        Region regionOfHunter = hunter.specificLocation.tileLocation.region;
        List<ECS.Character> possibleTargets = CharacterManager.Instance.GetCharacters(regionOfHunter, this);
        possibleTargets.Remove(hunter);
        if (possibleTargets.Count > 0) {
            return possibleTargets[Random.Range(0, possibleTargets.Count)];
        }
        return null;
    }
    //private ECS.Character GetTargetCharacter(ECS.Character hunter, ILocation location) {
    //    List<ECS.Character> possibleTargets = CharacterManager.Instance.GetCharacters(location, this);
    //    possibleTargets.Remove(hunter);
    //    if (possibleTargets.Count > 0) {
    //        return possibleTargets[Random.Range(0, possibleTargets.Count)];
    //    }
    //    return null;
    //}

    #region overrides
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
        if (_specificTarget == null) {
            _specificTarget = GetTargetCharacter(character);
        }
        _targetCharacter = (ECS.Character)_specificTarget;
        if (_targetLocation == null) {
            _targetLocation = _targetCharacter.specificLocation;
        }
        if (_targetCharacter != null) {
            _assignedCharacter.GoToLocation(_targetLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP);
        }
    }
    public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
        base.PerformTask();
        if(_targetCharacter != null) {
            if (_assignedCharacter.specificLocation.charactersAtLocation.Contains(_targetCharacter) && !_targetCharacter.isInCombat) {
                //target is at location
                InitiateCombat();
            } else {
                if (_targetCharacter.isDead) {
                    EndTask(TASK_STATUS.SUCCESS);
                }
            }
        } else {
            EndTask(TASK_STATUS.FAIL);
        }
    }
    public override bool CanBeDone(Character character, ILocation location) {
        if(location.tileLocation.landmarkOnTile != null) {
            for (int i = 0; i < location.tileLocation.landmarkOnTile.charactersAtLocation.Count; i++) {
                ECS.Character currChar = location.tileLocation.landmarkOnTile.charactersAtLocation[i].mainCharacter;
                if (CanMeetRequirements(currChar)) {
                    return true;
                }
            }
        }
        //if (GetTargetCharacter(character, location) != null) {
        //    return true;
        //}
        return base.CanBeDone(character, location);
    }
    public override bool AreConditionsMet(Character character) {
        //If there are any magic users in the region (Characters that use staves, or belong to the mage/battlemage class)
        for (int i = 0; i < character.specificLocation.tileLocation.region.charactersInRegion.Count; i++) {
            ECS.Character currChar = character.specificLocation.tileLocation.region.charactersInRegion[i];
            if (currChar.id != character.id) {
                if (CanMeetRequirements(currChar)) {
                    return true;
                }
            }
        }
        return base.AreConditionsMet(character);
    }
    public override int GetSelectionWeight(Character character) {
        return 100;
    }
    protected override WeightedDictionary<Character> GetCharacterTargetWeights(Character character) {
        WeightedDictionary<Character> characterWeights = base.GetCharacterTargetWeights(character);
        for (int i = 0; i < character.specificLocation.tileLocation.region.charactersInRegion.Count; i++) {
            ECS.Character currChar = character.specificLocation.tileLocation.region.charactersInRegion[i];
            if (currChar.id != character.id) {
                if (CanMeetRequirements(currChar)) {
                    int weight = 20; //Each magic user in region has base weight: 20
                    if (currChar.remainingHPPercent < 50) {
                        weight += 100; //If the magic user has less than 50% hp: +100
                    }
                    if (currChar.isFollower) {
                        weight += 200; //If the magic user is a follower: +200
                    }
                    characterWeights.AddElement(currChar, weight);
                }
            }
        }
        return characterWeights;
    }
    #endregion

    private void InitiateCombat() {
        _assignedCharacter.specificLocation.StartCombatBetween(_assignedCharacter, _targetCharacter);
    }

}
