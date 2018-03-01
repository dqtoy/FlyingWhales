using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public interface ICombatInitializer {
    CharacterAvatar avatar { get; }
	bool isDefeated { get;}
	int civilians { get;}
	Faction faction { get;}
	CharacterTask currentTask { get;}
	bool isInCombat { get; }
	Action currentFunction { get; }
	ECS.Character mainCharacter { get; }

	//bool InitializeCombat();
	bool IsHostileWith (ICombatInitializer combatInitializer);
	void ReturnCombatResults(ECS.CombatPrototype combat);
	void SetIsDefeated (bool state);
	//void SetCivilians (int amount);
	//void AdjustCivilians (int amount);
	void SetIsInCombat (bool state);
	void SetCurrentFunction (Action function);
    void ContinueDailyAction();

    STANCE GetCurrentStance();
}
