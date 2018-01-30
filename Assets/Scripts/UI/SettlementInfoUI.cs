using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SettlementInfoUI : UIMenu {

    internal bool isShowing;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private UILabel settlementInfoLbl;
	[SerializeField] private GameObject expandBtnGO;
	[SerializeField] private GameObject exploreBtnGO;
    [SerializeField] private UIScrollView infoScrollView;

    internal BaseLandmark currentlyShowingSettlement;

    internal override void Initialize() {
        Messenger.AddListener("UpdateUI", UpdateSettlementInfo);
        tweenPos.AddOnFinished(() => UpdateSettlementInfo());
    }

    public void ShowSettlementInfo() {
        isShowing = true;
		this.gameObject.SetActive (true);
        infoScrollView.ResetPosition();
        //      tweenPos.PlayForward();
    }
    public void HideSettlementInfo() {
        isShowing = false;
		this.gameObject.SetActive (false);
		HidePlayerActions ();
//      tweenPos.PlayReverse();
    }

    public void SetSettlementAsActive(BaseLandmark settlement) {
        currentlyShowingSettlement = settlement;
		UIManager.Instance.hexTileInfoUI.SetHexTileAsActive (settlement.location);
		ShowPlayerActions ();
        if (isShowing) {
            UpdateSettlementInfo();
        }
    }

    public void UpdateSettlementInfo() {
        if(currentlyShowingSettlement == null) {
            return;
        }
        string text = string.Empty;
		text += "[b]Location:[/b] " + "[url=" + currentlyShowingSettlement.location.id + "_hextile]" + currentlyShowingSettlement.location.tileName + "[/url]";
		text += "\n[b]Can Be Occupied:[/b] " + currentlyShowingSettlement.canBeOccupied.ToString();
		text += "\n[b]Is Occupied:[/b] " + currentlyShowingSettlement.isOccupied.ToString();
		text += "\n[b]Is Hidden:[/b] " + currentlyShowingSettlement.isHidden.ToString();
		text += "\n[b]Is Explored:[/b] " + currentlyShowingSettlement.isExplored.ToString();

        if (currentlyShowingSettlement.owner != null) {
            text += "\n[b]Owner:[/b] " + "[url=" + currentlyShowingSettlement.owner.id + "_faction]" + currentlyShowingSettlement.owner.name + "[/url]" + "/" + currentlyShowingSettlement.owner.race.ToString();
            text += "\n[b]Total Population: [/b] " + currentlyShowingSettlement.totalPopulation.ToString();
            text += "\n[b]Civilian Population: [/b] " + currentlyShowingSettlement.civiliansWithReserved.ToString();
            text += "\n[b]Population Growth: [/b] " + (currentlyShowingSettlement.totalPopulation * currentlyShowingSettlement.location.region.populationGrowth).ToString();

            if (currentlyShowingSettlement is Settlement) {
                text += "\n[b]Quest Board: [/b] ";
                Settlement settlement = (Settlement)currentlyShowingSettlement;
                if (settlement.questBoard.Count > 0) {
                    for (int i = 0; i < settlement.questBoard.Count; i++) {
                        Quest currQuest = settlement.questBoard[i];
                        text += "\n" + "[url=" + currQuest.id + "_quest]" + currQuest.questType.ToString();
                        if (currQuest.questType == QUEST_TYPE.EXPLORE_REGION) {
                            text += " " + ((ExploreRegion)currQuest).regionToExplore.centerOfMass.name + "[/url]";
                        } else if (currQuest.questType == QUEST_TYPE.EXPLORE_TILE) {
                            text += " " + ((ExploreTile)currQuest).landmarkToExplore.location.name + "[/url]";
                        } else {
                            text += "[/url]";
                        }
                        if (currQuest.isAccepted) {
                            text += " - A";
                            text += " (" + currQuest.assignedParty.name + ")";
                        } else {
                            text += " - N";
                        }

                    }
                } else {
                    text += "NONE";
                }
            }
        }

        //    text += "\n[b]Active Quests: [/b] ";
        //    if (currentlyShowingSettlement.owner.activeQuests.Count > 0) {
        //        for (int i = 0; i < currentlyShowingSettlement.owner.activeQuests.Count; i++) {
        //            Quest currQuest = currentlyShowingSettlement.owner.activeQuests[i];
        //            text += "\n" + "[url=" + currQuest.id + "_quest]" + currQuest.questType.ToString();
        //            if (currQuest.questType == QUEST_TYPE.EXPLORE_REGION) {
        //                text += " " + ((ExploreRegion)currQuest).regionToExplore.centerOfMass.name + "[/url]";
        //            } else if (currQuest.questType == QUEST_TYPE.EXPLORE_TILE) {
        //                text += " " + ((ExploreTile)currQuest).landmarkToExplore.location.name + "[/url]";
        //            } else {
        //                text += "[/url]";
        //            }
        //            if (currQuest.isAccepted) {
        //                text += " - A";
        //                text += " (" + currQuest.assignedParty.name + ")";
        //            } else {
        //                text += " - N";
        //            }
                    
        //        }
        //    } else {
        //        text += "NONE";
        //    }
        //}

		text += "\n[b]Characters: [/b] ";
		if (currentlyShowingSettlement.location.charactersOnTile.Count > 0) {
			for (int i = 0; i < currentlyShowingSettlement.location.charactersOnTile.Count; i++) {
				ECS.Character currChar = currentlyShowingSettlement.location.charactersOnTile[i];
				text += "\n" + "[url=" + currChar.id + "_character]" + currChar.name  + "[/url]" + " - " + (currChar.characterClass != null ? currChar.characterClass.className : "NONE") + "/" + (currChar.role != null ? currChar.role.roleType.ToString() : "NONE");
				if (currChar.currentTask != null) {
                    if (currChar.currentTask.taskType == TASK_TYPE.QUEST) {
                        Quest currQuest = (Quest)currChar.currentTask;
                        text += " ([url=" + currQuest.id + "_quest]" + currQuest.questType.ToString() + "[/url])";
                    } else {
                        text += " (" + currChar.currentTask.taskType.ToString() + ")";
                    }
                }
			}
		} else {
			text += "NONE";
		}

        text += "\n[b]Technologies: [/b] ";
        List<TECHNOLOGY> availableTech = currentlyShowingSettlement.technologies.Where(x => x.Value == true).Select(x => x.Key).ToList();
        if (availableTech.Count > 0) {
            text += "\n";
            for (int i = 0; i < availableTech.Count; i++) {
                TECHNOLOGY currTech = availableTech[i];
                text += currTech.ToString();
                if (i + 1 != availableTech.Count) {
                    text += ", ";
                }
            }
        } else {
            text += "NONE";
        }
		if(currentlyShowingSettlement.landmarkEncounterable != null){
			text += "\n[b]Encounterable: [/b]" + currentlyShowingSettlement.landmarkEncounterable.encounterName;
		}
		if(currentlyShowingSettlement is Settlement && currentlyShowingSettlement.specificLandmarkType == LANDMARK_TYPE.CITY && currentlyShowingSettlement.owner != null){
			text += "\n[b]Parties: [/b] ";
			if (PartyManager.Instance.allParties.Count > 0) {
				for (int i = 0; i < PartyManager.Instance.allParties.Count; i++) {
					Party currParty = PartyManager.Instance.allParties[i];
					text += "\n" + currParty.name + " O: " + currParty.isOpen + " F: " + currParty.isFull;
					if(currParty.currentTask != null) {
                        if (currParty.currentTask.taskType == TASK_TYPE.QUEST) {
                            Quest currQuest = (Quest)currParty.currentTask;
                            text += " ([url=" + currQuest.id + "_quest]" + currQuest.questType.ToString() + "[/url])";
                            if (currQuest.isDone) {
                                text += "(Done)";
                            } else {
                                if (currParty.isOpen || currQuest.isWaiting) {
                                    text += "(Forming Party)";
                                } else {
                                    text += "(In Progress)";
                                    if (currQuest.currentAction != null) {
                                        text += "(" + currQuest.currentAction.GetType().ToString() + ")";
                                    }

                                }
                            }
                        } else {
                            text += " (" + currParty.currentTask.taskType.ToString() + ")";
                            if (currParty.currentTask.isDone) {
                                text += "(Done)";
                            } else {
                                text += "(In Progress)";
                            }
                        }
					}
					text += "\n     Leader: [url=" + currParty.partyLeader.id + "_character]" + currParty.partyLeader.name + "[/url]";
                    if(currParty.partyMembers.Count > 2) {
                        text += "\n          Members:";
                    }
					for (int j = 0; j < currParty.partyMembers.Count; j++) {
						ECS.Character currMember = currParty.partyMembers[j];
						if(currMember.id != currParty.partyLeader.id) {
							text += "\n          [url=" + currMember.id + "_character]" + currMember.name + "[/url]";
						}
					}
                    text += "\n";
				}
			} else {
				text += "NONE";
			}
		}
       
        settlementInfoLbl.text = text;
        infoScrollView.UpdatePosition();
    }

	public void OnClickCloseBtn(){
//		UIManager.Instance.playerActionsUI.HidePlayerActionsUI ();
		HideSettlementInfo ();
	}

	public void OnClickExpandBtn(){
		currentlyShowingSettlement.owner.internalQuestManager.CreateExpandQuest(currentlyShowingSettlement);
		expandBtnGO.SetActive (false);
	}
	public void OnClickExploreRegionBtn(){
		currentlyShowingSettlement.location.region.centerOfMass
            .landmarkOnTile.owner.internalQuestManager.CreateExploreTileQuest(currentlyShowingSettlement);
        exploreBtnGO.SetActive(false);
    }
	private void ShowPlayerActions(){
		expandBtnGO.SetActive (CanExpand());
		exploreBtnGO.SetActive (CanExploreRegion ());
	}
	private void HidePlayerActions(){
		expandBtnGO.SetActive (false);
		exploreBtnGO.SetActive (false);
	}
	private bool CanExpand(){
		if(isShowing && currentlyShowingSettlement != null && currentlyShowingSettlement is Settlement){
			Settlement settlement = (Settlement)currentlyShowingSettlement;
			if(settlement.owner != null && settlement.owner.factionType == FACTION_TYPE.MAJOR){
				if(settlement.civilians > 20 && settlement.HasAdjacentUnoccupiedTile() && !settlement.owner.internalQuestManager.AlreadyHasQuestOfType(QUEST_TYPE.EXPAND, settlement)){
					return true;
				}
			}
		}
		return false;
	}

	private bool CanExploreRegion(){
		if(isShowing && currentlyShowingSettlement != null && !currentlyShowingSettlement.isExplored
			&& currentlyShowingSettlement.owner == null && currentlyShowingSettlement.location.region.centerOfMass.isOccupied 
            && !currentlyShowingSettlement.location.region.centerOfMass
            .landmarkOnTile.owner.internalQuestManager.AlreadyHasQuestOfType(QUEST_TYPE.EXPLORE_TILE, currentlyShowingSettlement)) {
			return true;
		}
		return false;
	}
}
