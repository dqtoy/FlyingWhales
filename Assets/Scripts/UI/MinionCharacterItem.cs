using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinionCharacterItem : CharacterItem {

    [SerializeField] private Image combatAbilityImg;

    public override void SetCharacter(Character character) {
        base.SetCharacter(character);
        UpdateCombatAbility();
    }

    private void UpdateCombatAbility() {
        combatAbilityImg.sprite = PlayerManager.Instance.GetCombatAbilitySprite(character.minion.combatAbility.name);
    }

    public void ShowCombatAbilityTooltip() {
        string header = character.minion.combatAbility.name;
        string message = character.minion.combatAbility.description;
        UIManager.Instance.ShowSmallInfo(message, header);
    }
}
