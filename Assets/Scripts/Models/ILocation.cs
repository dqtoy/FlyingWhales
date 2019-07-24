using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public interface ILocation {
    int id { get; }
	LOCATION_IDENTIFIER locIdentifier { get; }
	HexTile tileLocation { get; }
    string locationName { get; }
    string thisName { get; }

    //void ScheduleCombatCheck();
    //void UnScheduleCombatCheck();
    //void CheckForCombat();
    //bool HasCombatInitializers();
    //void PairUpCombats();
    //List<Character> GetCharactersByCombatPriority();
    //void CheckAttackingGroupsCombat();
    //void CheckPatrollingGroupsCombat();
    //bool HasHostilities();
    //bool HasHostilitiesWith(Faction faction, bool withFactionOnly = false);
    //bool HasHostileCharactersWith(Character character);
    //bool IsCharacterAtLocation(Character character);
    //List<Character> GetAttackingGroups();
    //List<Character> GetPatrollingGroups();
    //List<Character> GetGroupsBasedOnStance(STANCE stance, bool notInCombatOnly, Character except = null);
    //void StartCombatBetween(Character combatant1, Character combatant2);
    //void StartCombatAtLocation();
    //bool CombatAtLocation();
    //Character GetCombatEnemy(Character combatInitializer);
    //void SetCurrentCombat(CombatPrototype combat);
    //int CharactersCount(bool includeHostile = false);
    //void ContinueDailyActions();

	//ICharacter GetCharacterAtLocationByID (int id, bool includeTraces = false);
	//Party GetPartyAtLocationByLeaderID (int id);
}
