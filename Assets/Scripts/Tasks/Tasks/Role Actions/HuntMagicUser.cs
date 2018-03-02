using UnityEngine;
using System.Collections;
using ECS;
using System.Collections.Generic;

public class HuntMagicUser : CharacterTask {

    private ECS.Character _target;

    private CharacterFilter _taskFilter = new CharacterFilter(null, null, new List<CHARACTER_CLASS>() {
        CHARACTER_CLASS.ARCANIST,
        CHARACTER_CLASS.MAGE,
        CHARACTER_CLASS.BATTLEMAGE,
    });

    public HuntMagicUser(TaskCreator createdBy) : base(createdBy, TASK_TYPE.HUNT_MAGIC_USER) {
        SetStance(STANCE.STEALTHY);
    }

    private ECS.Character GetTargetCharacter(ECS.Character hunter) {
        Region regionOfHunter = hunter.specificLocation.tileLocation.region;
        List<ECS.Character> possibleTargets = CharacterManager.Instance.GetCharacters(regionOfHunter, _taskFilter);
        possibleTargets.Remove(hunter);
        if (possibleTargets.Count > 0) {
            return possibleTargets[Random.Range(0, possibleTargets.Count)];
        }
        return null;
    }

    #region overrides
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
        _target = GetTargetCharacter(character);
        if (_target != null) {
            _assignedCharacter.GoToLocation(_target.specificLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP);
        }
    }
    public override void PerformTask() {
        base.PerformTask();
        if(_target != null) {
            if (_assignedCharacter.specificLocation.charactersAtLocation.Contains(_target) && !_target.isInCombat) {
                //target is at location
                InitiateCombat();
            } else {
                if (_target.isDead) {
                    EndTask(TASK_STATUS.SUCCESS);
                }
            }
        } else {
            EndTask(TASK_STATUS.SUCCESS);
        }
    }
    public override bool CanBeDone(Character character, ILocation location) {
        if (GetTargetCharacter(character) != null) {
            return true;
        }
        return base.CanBeDone(character, location);
    }
    #endregion

    private void InitiateCombat() {
        _assignedCharacter.specificLocation.StartCombatBetween(_assignedCharacter, _target);
    }

}
