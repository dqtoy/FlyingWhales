using UnityEngine;
using System.Collections;
using System;

public class TraitObject : MonoBehaviour {

	public BEHAVIOR_TRAIT behaviourTrait = BEHAVIOR_TRAIT.NONE;
	public SKILL_TRAIT skillTrait = SKILL_TRAIT.NONE;
	public MISC_TRAIT miscTrait = MISC_TRAIT.NONE;

	public UI2DSprite innerSprite;
	private bool isHovering = false;

	public void SetTrait(BEHAVIOR_TRAIT bTrait, SKILL_TRAIT sTrait, MISC_TRAIT mTrait){
		behaviourTrait = bTrait;
		skillTrait = sTrait;
		miscTrait = mTrait;
		innerSprite.color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
	}

	void OnHover(bool isOver){
		if (isOver) {
			this.isHovering = true;
			if (behaviourTrait != BEHAVIOR_TRAIT.NONE) {
				UIManager.Instance.ShowSmallInfo ("[b]" + behaviourTrait.ToString() + "[/b]", this.transform);
			} else if (skillTrait != SKILL_TRAIT.NONE) {
				UIManager.Instance.ShowSmallInfo ("[b]" + skillTrait.ToString() + "[/b]", this.transform);
			} else if (miscTrait != MISC_TRAIT.NONE) {
				UIManager.Instance.ShowSmallInfo ("[b]" + miscTrait.ToString() + "[/b]", this.transform);
			}
		} else {
			this.isHovering = false;
			UIManager.Instance.HideSmallInfo ();
		}
	}

	void Update(){
		if (this.isHovering) {
			if (behaviourTrait != BEHAVIOR_TRAIT.NONE) {
				UIManager.Instance.ShowSmallInfo ("[b]" + behaviourTrait.ToString() + "[/b]", this.transform);
			} else if (skillTrait != SKILL_TRAIT.NONE) {
				UIManager.Instance.ShowSmallInfo ("[b]" + skillTrait.ToString() + "[/b]", this.transform);
			} else if (miscTrait != MISC_TRAIT.NONE) {
				UIManager.Instance.ShowSmallInfo ("[b]" + miscTrait.ToString() + "[/b]", this.transform);
			}
		}
	}
}
