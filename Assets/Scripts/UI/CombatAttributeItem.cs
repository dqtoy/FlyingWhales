using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CombatAttributeItem : MonoBehaviour {
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    private CombatAttribute _combatAttribute;

    public void SetCombatAttribute(CombatAttribute combatAttribute) {
        _combatAttribute = combatAttribute;
        nameText.text = _combatAttribute.name;
        descriptionText.text = _combatAttribute.description;
    }
}
