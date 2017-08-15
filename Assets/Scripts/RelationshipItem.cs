﻿using UnityEngine;
using System.Collections;

public class RelationshipItem : MonoBehaviour {

	[SerializeField] private UI2DSprite relationshipSprite;
    [SerializeField] private UILabel likenessLbl;
	private RelationshipKings rk;

	private bool isHovering = false;

	public void SetRelationship(RelationshipKings rk){
		this.rk = rk;
		this.relationshipSprite.color = Utilities.GetColorForRelationship (rk.lordRelationship);
        likenessLbl.text = Mathf.Clamp(this.rk.totalLike, -100, 100).ToString();
	}



    void OnHover(bool isOver) {
        if (isOver) {
            //this.isHovering = true;
            //UIManager.Instance.ShowSmallInfo("[b]" + rk.lordRelationship.ToString() + "[/b]\n" + rk.like.ToString());
//            int modifierFromWar = rk.GetLikenessModifiersFromEvent(EVENT_TYPES.KINGDOM_WAR);
//            int modifierFromOtherEvents = rk.eventLikenessModifier - modifierFromWar;
//            string relationshipSummary = string.Empty;
//            if(rk.baseLike >= 0) {
//                relationshipSummary += "+";
//            }
//            relationshipSummary += rk.baseLike.ToString() + "   Base Value";
//            if(modifierFromWar != 0) {
//                relationshipSummary += "\n";
//                if (modifierFromWar > 0) {
//                    relationshipSummary += "+";
//                }
//                relationshipSummary += modifierFromWar.ToString() + "   Recent War";
//            }
//            if(rk.likeFromMutualRelationships != 0) {
//                relationshipSummary += "\n";
//                if (rk.likeFromMutualRelationships > 0) {
//                    relationshipSummary += "+";
//                }
//                relationshipSummary += rk.likeFromMutualRelationships.ToString() + "    Circle of Friends";
//            }
//            if (modifierFromOtherEvents != 0) {
//                relationshipSummary += "\n";
//                if (modifierFromOtherEvents > 0) {
//                    relationshipSummary += "+";
//                }
//                relationshipSummary += modifierFromOtherEvents.ToString() + "   Ideologies";
//            }
			this.rk._relationshipEventsSummary = string.Empty;
			for (int i = 0; i < this.rk.eventModifiers.Count; i++) {
				this.rk._relationshipEventsSummary += "\n" + this.rk.eventModifiers [i].summary;
			}
			UIManager.Instance.ShowRelationshipSummary(this.rk.sourceKing, this.rk.relationshipSummary);
        } else {
            //this.isHovering = false;
            //UIManager.Instance.HideSmallInfo();
            UIManager.Instance.HideRelationshipSummary();
        }
    }

    //void Update(){
    //	if (this.isHovering) {
    //		if (this.rk != null) {
    //			UIManager.Instance.ShowSmallInfo ("[b]" + rk.lordRelationship.ToString () + "[/b]\n" + rk.like.ToString ());
    //		}
    //	}
    //}

}
