using UnityEngine;
using System.Collections;

public interface ICombatInitializer {
	bool InitializeCombat();
	bool CanBattleThis (ICombatInitializer combatInitializer);
	void ReturnCombatResults(ECS.CombatPrototype combat);
}
