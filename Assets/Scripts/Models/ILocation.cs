using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ILocation {

    List<ICombatInitializer> charactersAtLocation { get; }

    void AddCharacterToLocation(ICombatInitializer character, bool startCombat = true);
    void RemoveCharacterFromLocation(ICombatInitializer character);

    void StartCombatAtLocation();
    bool CombatAtLocation();
    ICombatInitializer GetCombatEnemy(ICombatInitializer combatInitializer);
    void SetCurrentCombat(ECS.CombatPrototype combat);
}
