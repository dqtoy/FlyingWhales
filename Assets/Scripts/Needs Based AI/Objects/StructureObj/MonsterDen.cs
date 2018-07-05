using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDen : StructureObj {

    private const int SPAWN_COOLDOWN_TICKS = 48;

    private int spawnCooldown;
    private Monster spawn;

    public MonsterDen() : base() {
        _specificObjectType = LANDMARK_TYPE.MONSTER_DEN;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        ResetSpawnCooldown();
        //_resourceInventory[RESOURCE.IRON] = 5000;
    }

    #region Overrides
    public override IObject Clone() {
        MonsterDen clone = new MonsterDen();
        SetCommonData(clone);
        return clone;
    }
    #endregion

    public void ReduceSpawnCooldown(int amount) {
        spawnCooldown -= amount;
        if (spawnCooldown <= 0) {
            SpawnMonster();
            ResetSpawnCooldown();
        }
    }

    private void SpawnMonster() {
        Area areaOfObj = specificLocation.tileLocation.areaOfTile;
        if (areaOfObj != null) {
            AreaData areaData = LandmarkManager.Instance.GetAreaData(areaOfObj.areaType);
            if (areaData.possibleMonsterSpawns.Count > 0) {
                MonsterPartyComponent chosenMonster = areaData.possibleMonsterSpawns[Random.Range(0, areaData.possibleMonsterSpawns.Count)];
                Monster spawnedMonster = MonsterManager.Instance.SpawnMonsterOnLandmark(this.objectLocation, chosenMonster.name);
                BindMonsterToDen(spawnedMonster);
            }
        } else {
            Debug.LogWarning("Monster Den is not part of and area, so it cannot spawn a monster!" + specificLocation.tileLocation.ToString(), specificLocation.tileLocation.gameObject);
        }
    }


    private void BindMonsterToDen(Monster monster) {
        spawn = monster;
        ChangeState(GetState("Occupied"));
        Messenger.AddListener<Monster>(Signals.MONSTER_DEATH, OnMonsterDied);
    }

    private void OnMonsterDied(Monster monsterThatDied) {
        if (monsterThatDied.id == spawn.id) {
            spawn = null;
            ChangeState(GetState("Spawning"));
            Messenger.RemoveListener<Monster>(Signals.MONSTER_DEATH, OnMonsterDied);
        }
    }

    private void ResetSpawnCooldown() {
        spawnCooldown = SPAWN_COOLDOWN_TICKS;
    }
}
