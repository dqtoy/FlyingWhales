using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		PlayerManager.Instance.InitializePlayer(data.portal, data.portalStructure);
		yield return null;
		GenerateInitialMinions();
		yield return null;
	}

	private void GenerateInitialMinions() {
		for (int i = 0; i < CharacterManager.sevenDeadlySinsClassNames.Length; i++) {
			string className = CharacterManager.sevenDeadlySinsClassNames[i];
			Minion minion = PlayerManager.Instance.player.CreateNewMinion(className, RACE.DEMON, false);
			minion.SetCombatAbility(COMBAT_ABILITY.FLAMESTRIKE);
			minion.SetRandomResearchInterventionAbilities(new List<SPELL_TYPE>());
			PlayerManager.Instance.player.AddMinion(minion);
		}
	}
}
