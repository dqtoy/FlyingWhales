using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataGeneration : MapGenerationComponent {
	public override IEnumerator Execute(MapGenerationData data) {
		PlayerManager.Instance.InitializePlayer(data.portal, data.portalStructure, CursorManager.Instance.selectedArchetype);
		yield return null;
		GenerateInitialMinions();
		yield return null;
	}

	private void GenerateInitialMinions() {
        List<string> archetypeMinions = PlayerManager.Instance.player.archetype.minionClasses;
        for (int i = 0; i < archetypeMinions.Count; i++) {
            string className = archetypeMinions[i];
            Minion minion = PlayerManager.Instance.player.CreateNewMinion(className, RACE.DEMON, false);
            minion.SetCombatAbility(COMBAT_ABILITY.FLAMESTRIKE);
            PlayerManager.Instance.player.AddMinion(minion);
            PlayerManager.Instance.player.playerSettlement.region.RemoveCharacterFromLocation(minion.character);
        }
    }
}
