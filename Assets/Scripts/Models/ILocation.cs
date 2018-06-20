using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ILocation {
    int id { get; }
	LOCATION_IDENTIFIER locIdentifier { get; }
	HexTile tileLocation { get; }
    string locationName { get; }
    List<ICombatInitializer> charactersAtLocation { get; }

    void AddCharacterToLocation(ICombatInitializer character);
	void RemoveCharacterFromLocation(ICombatInitializer character);
    void ReplaceCharacterAtLocation(ICombatInitializer characterToReplace, ICombatInitializer characterToAdd);

    void ScheduleCombatCheck();
    void UnScheduleCombatCheck();
    void CheckForCombat();
    bool HasCombatInitializers();
    void PairUpCombats();
    List<ICombatInitializer> GetCharactersByCombatPriority();
    //void CheckAttackingGroupsCombat();
    //void CheckPatrollingGroupsCombat();
    bool HasHostilities();
    bool HasHostilitiesWith(Faction faction, bool withFactionOnly = false);
    bool HasHostileCharactersWith(ECS.Character character);
    //List<ICombatInitializer> GetAttackingGroups();
    //List<ICombatInitializer> GetPatrollingGroups();
    List<ICombatInitializer> GetGroupsBasedOnStance(STANCE stance, bool notInCombatOnly, ICombatInitializer except = null);
    void StartCombatBetween(ICombatInitializer combatant1, ICombatInitializer combatant2);
    //void StartCombatAtLocation();
    //bool CombatAtLocation();
    //ICombatInitializer GetCombatEnemy(ICombatInitializer combatInitializer);
    //void SetCurrentCombat(ECS.CombatPrototype combat);
    int CharactersCount(bool includeHostile = false);
    void ContinueDailyActions();

	ECS.Character GetCharacterAtLocationByID (int id, bool includeTraces = false);
	Party GetPartyAtLocationByLeaderID (int id);
}
