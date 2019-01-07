using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CombatAttributeItem : MonoBehaviour {
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    private Trait _combatAttribute;

    public void SetCombatAttribute(Trait combatAttribute) {
        _combatAttribute = combatAttribute;
        nameText.text = _combatAttribute.nameInUI;
        descriptionText.text = _combatAttribute.description;
    }
}
