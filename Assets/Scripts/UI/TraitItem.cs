using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Traits;

public class TraitItem : MonoBehaviour {
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public Image iconImg;

    [SerializeField] private CharacterPortrait portrait;

    private Trait trait;

    public void SetCombatAttribute(Trait trait) {
        this.trait = trait;
        nameText.text = this.trait.name;

        if (this.trait is RelationshipTrait relTrait) {
            portrait.GeneratePortrait(relTrait.targetCharacter);
            portrait.gameObject.SetActive(true);
            iconImg.gameObject.SetActive(false);
            //portrait.SetClickButton(UnityEngine.EventSystems.PointerEventData.InputButton.Left);
        } else {
            portrait.gameObject.SetActive(false);
            Sprite icon = TraitManager.Instance.GetTraitPortrait(trait.name);
            if (icon != null) {
                iconImg.sprite = icon;
                iconImg.gameObject.SetActive(true);
            } else {
                iconImg.gameObject.SetActive(false);
            }
        }


        descriptionText.text = this.trait.description;
        this.gameObject.SetActive(true);
    }

    public void OnHover() {
        if(trait != null) {
            string summary = trait.name;
            summary += $"\n{trait.GetTestingData()}";
            if(summary != string.Empty) {
                UIManager.Instance.ShowSmallInfo(summary);
            }
        }
    }
    public void OnHoverOut() {
        if (trait != null) {
            UIManager.Instance.HideSmallInfo();
        }
    }
}
