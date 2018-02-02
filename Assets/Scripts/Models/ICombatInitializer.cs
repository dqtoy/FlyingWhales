using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface ICombatInitializer {
	bool isDefeated { get;}
	int civilians { get;}
	Faction faction { get;}
	CharacterTask currentTask { get;}

	bool InitializeCombat();
	bool CanBattleThis (ICombatInitializer combatInitializer);
	void ReturnCombatResults(ECS.CombatPrototype combat);
	void SetIsDefeated (bool state);
	void SetCivilians (int amount);
	void AdjustCivilians (int amount);
}
