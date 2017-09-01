using UnityEngine;
using System.Collections;

public class RelationshipItem : MonoBehaviour {

	[SerializeField] private UI2DSprite relationshipSprite;
    [SerializeField] private UILabel likenessLbl;
	private KingdomRelationship rk;

	private bool isHovering = false;

	public void SetRelationship(KingdomRelationship rk){
		this.rk = rk;
		this.relationshipSprite.color = Utilities.GetColorForRelationship (rk.relationshipStatus);
        likenessLbl.text = Mathf.Clamp(this.rk.totalLike, -100, 100).ToString();
	}

    void OnHover(bool isOver) {
        if (isOver) {
            string summary = string.Empty;
            summary += rk.relationshipSummary + "\n";
            if(rk.likeFromMutualRelationships != 0) {
                if (rk.likeFromMutualRelationships > 0) {
                    summary += "+ ";
                }
                summary += rk.likeFromMutualRelationships.ToString() + " Mutual Relationships\n";
            }

            if(rk.eventLikenessModifier != 0) {
                if (rk.eventLikenessModifier > 0) {
                    summary += "+ ";
                }
                summary += rk.eventLikenessModifier.ToString() + " Opinions";
            }
            
			UIManager.Instance.ShowRelationshipSummary(this.rk.targetKingdom.king, summary);
        } else {
            UIManager.Instance.HideRelationshipSummary();
        }
    }

}
