using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ILocation {
	LOCATION_IDENTIFIER locIdentifier { get; }
	HexTile tileLocation { get; }
    string locationName { get; }
    List<ICombatInitializer> charactersAtLocation { get; }

    void AddCharacterToLocation(ICombatInitializer character, bool startCombat = true);
	void RemoveCharacterFromLocation(ICombatInitializer character, bool forLeaving = true);

    void ScheduleCombatCheck();
    void UnScheduleCombatCheck();
    void CheckForCombat();
    void CheckAttackingGroupsCombat();
    void CheckPatrollingGroupsCombat();
    bool HasHostilities();
    bool HasHostilitiesWith(Faction faction, bool withFactionOnly = false);
    bool HasHostilitiesWith(ECS.Character character);
    List<ICombatInitializer> GetAttackingGroups();
    List<ICombatInitializer> GetPatrollingGroups();
    List<ICombatInitializer> GetGroupsBasedOnStance(STANCE stance, bool notInCombatOnly, ICombatInitializer except = null);
    void StartCombatBetween(ICombatInitializer combatant1, ICombatInitializer combatant2);
    //void StartCombatAtLocation();
    //bool CombatAtLocation();
    //ICombatInitializer GetCombatEnemy(ICombatInitializer combatInitializer);
    //void SetCurrentCombat(ECS.CombatPrototype combat);
    int CharactersCount(bool includeHostile = false);
    void ContinueDailyActions();

    ECS.Character GetCharacterAtLocationByID (int id);
	Party GetPartyAtLocationByLeaderID (int id);
}
