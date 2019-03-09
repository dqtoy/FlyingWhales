using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatAttributeItem : MonoBehaviour {
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image iconImg;

    private Trait _combatAttribute;

    public void SetCombatAttribute(Trait combatAttribute) {
        _combatAttribute = combatAttribute;
        nameText.text = _combatAttribute.nameInUI;
        Sprite icon = AttributeManager.Instance.GetTraitIcon(combatAttribute.name);
        if (icon != null) {
            iconImg.sprite = icon;
        }
        descriptionText.text = _combatAttribute.description;
    }

    public void OnHover() {
        if(_combatAttribute != null) {
            string summary = _combatAttribute.nameInUI;
            if (_combatAttribute is RelationshipTrait) {
                RelationshipTrait t = _combatAttribute as RelationshipTrait;
                CharacterRelationshipData rel = UIManager.Instance.characterInfoUI.activeCharacter.GetCharacterRelationshipData(t.targetCharacter);
                summary += "\n" + rel.GetSummary();
            }
            UIManager.Instance.ShowSmallInfo(summary);
        }
    }
    public void OnHoverOut() {
        if (_combatAttribute != null) {
            UIManager.Instance.HideSmallInfo();
        }
    }
}
