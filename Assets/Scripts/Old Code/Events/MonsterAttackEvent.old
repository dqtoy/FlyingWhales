using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttackEvent : GameEvent {

    private BaseLandmark _targetLandmark;
    private MonsterParty _monsterPartySpawned;
    private AttackLandmarkAction _attackLandmarkAction;

    public MonsterAttackEvent() : base(GAME_EVENT.MONSTER_ATTACK) {
        Messenger.AddListener<MonsterParty>(Signals.MONSTER_PARTY_DIED, MonsterPartyDied);
    }

    public void Initialize(BaseLandmark target) {
        _targetLandmark = target;
        MonsterPartyComponent chosenParty = MonsterManager.Instance.monsterAttackParties[UnityEngine.Random.Range(0, MonsterManager.Instance.monsterAttackParties.Count)];
        _monsterPartySpawned = MonsterManager.Instance.SpawnMonsterPartyOnLandmark(_targetLandmark, chosenParty, false);
        _monsterPartySpawned.SetIsAttacking(true);
        if (_monsterPartySpawned.specificLocation.tileLocation.landmarkOnTile != null) {
            Messenger.Broadcast(Signals.LANDMARK_UNDER_ATTACK, _monsterPartySpawned.GetBase(), this.GetBase());
        }
        _attackLandmarkAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK_LANDMARK) as AttackLandmarkAction;
        _attackLandmarkAction.SetOnEndAction(() => EndTerrorization());
        _monsterPartySpawned.actionData.AssignAction(_attackLandmarkAction, target.landmarkObj);
    }
    private void MonsterPartyDied(MonsterParty monsterParty) {
        if(_monsterPartySpawned.id == monsterParty.id) {
            EndTerrorization();
        }
    }
    private void EndTerrorization() {
        if (_isDone) {
            return;
        }
        //if(_monsterPartySpawned.currentCombat != null) {
        //    _monsterPartySpawned.currentCombat.AddAfterCombatAction(() => WaitForCombatToFinish());
        //    return;
        //}
        _monsterPartySpawned.SetIsAttacking(false);
        if (!_monsterPartySpawned.isDead) {
            _targetLandmark.DestroyLandmark();
        }
        EndEvent();
    }
    private void WaitForCombatToFinish() {
        EndTerrorization();
    }

    #region Overrides
    public override void EndEvent() {
        base.EndEvent();
        Messenger.RemoveListener<MonsterParty>(Signals.MONSTER_PARTY_DIED, MonsterPartyDied);
    }
    #endregion
}
