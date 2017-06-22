using UnityEngine;
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
            int modifierFromWar = rk.GetLikenessModifiersFromEvent(EVENT_TYPES.KINGDOM_WAR);
            int modifierFromOtherEvents = rk.eventLikenessModifier - modifierFromWar;
            string relationshipSummary = string.Empty;
            relationshipSummary += rk.baseLike.ToString() + " Base Value";
            if(modifierFromWar != 0) {
                relationshipSummary += modifierFromWar.ToString() + " Recent War";
            }
            if(rk.likeFromMutualRelationships != 0) {
                relationshipSummary += rk.likeFromMutualRelationships.ToString() + " Circle of Friends";
            }
            if (modifierFromOtherEvents != 0) {
                relationshipSummary += modifierFromOtherEvents.ToString() + " Ideologies";
            }

            UIManager.Instance.ShowRelationshipSummary(rk.sourceKing, relationshipSummary);
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
