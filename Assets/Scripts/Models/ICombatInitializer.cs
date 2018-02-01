using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ICombatInitializer {
	bool isDefeated { get;}
	bool InitializeCombat();
	bool CanBattleThis (ICombatInitializer combatInitializer);
	void ReturnCombatResults(ECS.CombatPrototype combat);
	void SetIsDefeated (bool state);
}
