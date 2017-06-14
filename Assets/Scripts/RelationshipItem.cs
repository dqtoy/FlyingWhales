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
        likenessLbl.text = this.rk.like.ToString();
	}

	//void OnHover(bool isOver){
	//	if (isOver) {
	//		this.isHovering = true;
	//		UIManager.Instance.ShowSmallInfo("[b]" + rk.lordRelationship.ToString () + "[/b]\n" + rk.like.ToString ());
	//	} else {
	//		this.isHovering = false;
	//		UIManager.Instance.HideSmallInfo();
	//	}
	//}

	//void Update(){
	//	if (this.isHovering) {
	//		if (this.rk != null) {
	//			UIManager.Instance.ShowSmallInfo ("[b]" + rk.lordRelationship.ToString () + "[/b]\n" + rk.like.ToString ());
	//		}
	//	}
	//}

}
