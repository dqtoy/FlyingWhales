using UnityEngine;
using System.Collections;

public class TraitIcon : MonoBehaviour {

    private TRAIT trait;

    [SerializeField] private UI2DSprite innerSprite;
    private bool isHovering = false;

    public void SetTrait(TRAIT trait) {
        this.trait = trait;
        innerSprite.color = Utilities.GetColorForTrait(trait);
    }

    private void OnHover(bool isOver) {
        if (isOver) {
            this.isHovering = true;
            UIManager.Instance.ShowSmallInfo("[b]" + this.trait.ToString() + "[/b]");
        } else {
            this.isHovering = false;
            UIManager.Instance.HideSmallInfo();
        }
    }
}
