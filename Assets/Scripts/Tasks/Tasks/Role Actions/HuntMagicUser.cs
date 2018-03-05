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
        _filters = new QuestFilter[] {
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
    private ECS.Character GetTargetCharacter(ECS.Character hunter, ILocation location) {
        List<ECS.Character> possibleTargets = CharacterManager.Instance.GetCharacters(location, this);
        possibleTargets.Remove(hunter);
        if (possibleTargets.Count > 0) {
            return possibleTargets[Random.Range(0, possibleTargets.Count)];
        }
        return null;
    }

    #region overrides
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
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
            EndTask(TASK_STATUS.SUCCESS);
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
    #endregion

    private void InitiateCombat() {
        _assignedCharacter.specificLocation.StartCombatBetween(_assignedCharacter, _targetCharacter);
    }

}
