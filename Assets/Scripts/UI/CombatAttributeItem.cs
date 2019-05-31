using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CombatAttributeItem : MonoBehaviour {
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image iconImg;

    [SerializeField] private CharacterPortrait portrait;

    private Trait _combatAttribute;

    public void SetCombatAttribute(Trait combatAttribute) {
        _combatAttribute = combatAttribute;
        nameText.text = _combatAttribute.nameInUI;

        if (_combatAttribute is RelationshipTrait) {
            RelationshipTrait relTrait = _combatAttribute as RelationshipTrait;
            portrait.GeneratePortrait(relTrait.targetCharacter);
            portrait.gameObject.SetActive(true);
            iconImg.gameObject.SetActive(false);
            portrait.SetClickButton(UnityEngine.EventSystems.PointerEventData.InputButton.Left);
        } else {
            portrait.gameObject.SetActive(false);
            Sprite icon = AttributeManager.Instance.GetTraitIcon(combatAttribute.name);
            if (icon != null) {
                iconImg.sprite = icon;
                iconImg.gameObject.SetActive(true);
            } else {
                iconImg.gameObject.SetActive(false);
            }
        }

        
        descriptionText.text = _combatAttribute.description;
        this.gameObject.SetActive(true);
    }

    public void OnHover() {
        if(_combatAttribute != null) {
            string summary = _combatAttribute.nameInUI;
            if (_combatAttribute is RelationshipTrait) {
                RelationshipTrait t = _combatAttribute as RelationshipTrait;
                for (int i = 0; i < t.targetCharacter.alterEgos.Values.Count; i++) {
                    AlterEgoData currAlterEgo = t.targetCharacter.alterEgos.Values.ElementAt(i);
                    if (UIManager.Instance.characterInfoUI.activeCharacter.HasRelationshipWith(currAlterEgo, true)) {
                        CharacterRelationshipData rel = UIManager.Instance.characterInfoUI.activeCharacter.relationships[currAlterEgo];
                        summary += "\n" + rel.GetSummary();
                        break;
                    }
                }
                //if (UIManager.Instance.characterInfoUI.activeCharacter.HasRelationshipWith(t.targetCharacter, true)) {
                //CharacterRelationshipData rel = UIManager.Instance.characterInfoUI.activeCharacter.relationships[t.targetCharacter.currentAlterEgo];
                //summary += "\n" + rel.GetSummary();
                //} else {
                //    summary = string.Empty;
                //}
            } else {
                summary += "\n" + _combatAttribute.GetTestingData();
            }
            if(summary != string.Empty) {
                UIManager.Instance.ShowSmallInfo(summary);
            }
        }
    }
    public void OnHoverOut() {
        if (_combatAttribute != null) {
            UIManager.Instance.HideSmallInfo();
        }
    }
}
