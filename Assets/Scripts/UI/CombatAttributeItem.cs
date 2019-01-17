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
        descriptionText.text = _combatAttribute.description;
    }

    public void OnHover() {
        if(_combatAttribute != null) {
            UIManager.Instance.ShowSmallInfo(_combatAttribute.nameInUI);
        }
    }
    public void OnHoverOut() {
        if (_combatAttribute != null) {
            UIManager.Instance.HideSmallInfo();
        }
    }
}
