using UnityEngine;
using System.Collections;

public class PlayerActionsUI : MonoBehaviour {
	public UIButton expandBtn;
	public UIButton exploreBtn;

	public UIGrid grid;
	public UILabel openCloseLbl;
	public TweenPosition tweenPos;

	public bool isShowing;

	void OnEnable(){
		DisableAllActions ();
	}
	private void DisableAllActions (){
		expandBtn.isEnabled = false;
		exploreBtn.isEnabled = false;
	}
	public void ShowPlayerActionsUI(){
		this.gameObject.SetActive (true);
		UpdatePlayerActionsUI ();
	}
	public void HidePlayerActionsUI(){
		this.gameObject.SetActive (false);	
	}
	public void OpenClosePlayerActionsUI(){
		if(isShowing){
			DisableAllActions ();
			tweenPos.PlayReverse ();
		}else{
			tweenPos.PlayForward ();
		}
	}
	public void OnFinishAnimation(){
		if(isShowing){
			isShowing = false;
			openCloseLbl.text = "[b]<[/b]";
		}else{
			isShowing = true;
			openCloseLbl.text = "[b]>[/b]";
			UpdatePlayerActionsUI ();
		}
	}
	public void OnClickExpandBtn(){
		UIManager.Instance.settlementInfoUI.currentlyShowingSettlement.owner.internalQuestManager.CreateExpandQuest(UIManager.Instance.settlementInfoUI.currentlyShowingSettlement);
	}
	public void OnClickExploreRegionBtn(){
		//UIManager.Instance.settlementInfoUI.currentlyShowingSettlement.owner.internalQuestManager.CreateExploreRegionQuest();
	}

	public void UpdatePlayerActionsUI(){
		if(isShowing){
			ShowExpand ();
			ShowExploreRegion ();
		}
	}

	private void ShowExpand(){
		if(UIManager.Instance.settlementInfoUI.isShowing && UIManager.Instance.settlementInfoUI.currentlyShowingSettlement != null && UIManager.Instance.settlementInfoUI.currentlyShowingSettlement is Settlement){
			Settlement settlement = (Settlement)UIManager.Instance.settlementInfoUI.currentlyShowingSettlement;
			if(settlement.owner != null && settlement.owner.factionType == FACTION_TYPE.MAJOR){
				if(settlement.civilians > 20 && settlement.HasAdjacentUnoccupiedTile()){
					expandBtn.isEnabled = true;
					return;
				}
			}
		}
		expandBtn.isEnabled = false;
	}

	private void ShowExploreRegion(){
		if(UIManager.Instance.settlementInfoUI.isShowing && UIManager.Instance.settlementInfoUI.currentlyShowingSettlement != null && UIManager.Instance.settlementInfoUI.currentlyShowingSettlement.isHidden && !UIManager.Instance.settlementInfoUI.currentlyShowingSettlement.isExplored
			&& UIManager.Instance.settlementInfoUI.currentlyShowingSettlement.owner == null && UIManager.Instance.settlementInfoUI.currentlyShowingSettlement.location.region.centerOfMass.isOccupied){
			exploreBtn.isEnabled = true;
			return;
		}
		exploreBtn.isEnabled = false;
	}
}
