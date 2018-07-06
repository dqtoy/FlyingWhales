using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDen : StructureObj {

    private const int SPAWN_COOLDOWN_TICKS = 48;

    private int spawnCooldown;
    private MonsterParty spawnedParty;

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
                MonsterParty spawnedMonsterParty = MonsterManager.Instance.SpawnMonsterPartyOnLandmark(this.objectLocation, chosenMonster);
                BindMonsterPartyToDen(spawnedMonsterParty);
            }
        } else {
            Debug.LogWarning("Monster Den is not part of and area, so it cannot spawn a monster!" + specificLocation.tileLocation.ToString(), specificLocation.tileLocation.gameObject);
        }
    }


    private void BindMonsterPartyToDen(MonsterParty party) {
        spawnedParty = party;
        ChangeState(GetState("Occupied"));
        Messenger.AddListener<MonsterParty>(Signals.MONSTER_PARTY_DIED, OnMonsterPartyDied);
    }

    private void OnMonsterPartyDied(MonsterParty partyThatDied) {
        if (partyThatDied.id == spawnedParty.id) {
            spawnedParty = null;
            ChangeState(GetState("Spawning"));
            Messenger.RemoveListener<MonsterParty>(Signals.MONSTER_PARTY_DIED, OnMonsterPartyDied);
        }
    }

    private void ResetSpawnCooldown() {
        spawnCooldown = SPAWN_COOLDOWN_TICKS;
    }
}
