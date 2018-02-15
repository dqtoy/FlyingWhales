using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SettlementInfoUI : UIMenu {

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private UILabel settlementInfoLbl;
	[SerializeField] private UILabel settlementHistoryInfoLbl;
	[SerializeField] private GameObject expandBtnGO;
	[SerializeField] private GameObject exploreBtnGO;
    [SerializeField] private GameObject buildStructureBtnGO;
    [SerializeField] private UIScrollView infoScrollView;
	[SerializeField] private UIScrollView historyScrollView;

    internal BaseLandmark currentlyShowingLandmark {
        get { return _data as BaseLandmark; }
    }

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener("UpdateUI", UpdateSettlementInfo);
        tweenPos.AddOnFinished(() => UpdateSettlementInfo());
    }

    public override void OpenMenu() {
        base.OpenMenu();
        UpdateSettlementInfo();
    }

    public override void ShowMenu() {
        base.ShowMenu();
        infoScrollView.ResetPosition();
    }
    public override void HideMenu() {
        base.HideMenu();
		HidePlayerActions();
    }
    public override void SetData(object data) {
        base.SetData(data);
        UIManager.Instance.hexTileInfoUI.SetData((data as BaseLandmark).location);
        ShowPlayerActions();
        if (isShowing) {
            UpdateSettlementInfo();
        }
    }

    public void UpdateSettlementInfo() {
        if(currentlyShowingLandmark == null) {
            return;
        }
        string text = string.Empty;
		if (currentlyShowingLandmark.landmarkName != string.Empty) {
			text += "[b]Name:[/b] " + currentlyShowingLandmark.landmarkName + "\n";
		}
		text += "[b]Location:[/b] " + currentlyShowingLandmark.location.urlName;
        text += "\n[b]Material:[/b] " + currentlyShowingLandmark.location.materialOnTile.ToString();
        text += "\n[b]Can Be Occupied:[/b] " + currentlyShowingLandmark.canBeOccupied.ToString();
		text += "\n[b]Is Occupied:[/b] " + currentlyShowingLandmark.isOccupied.ToString();
		text += "\n[b]Is Hidden:[/b] " + currentlyShowingLandmark.isHidden.ToString();
		text += "\n[b]Is Explored:[/b] " + currentlyShowingLandmark.isExplored.ToString();

        if (currentlyShowingLandmark.owner != null) {
            text += "\n[b]Owner:[/b] " + currentlyShowingLandmark.owner.urlName + "/" + currentlyShowingLandmark.owner.race.ToString();
            text += "\n[b]Regional Population: [/b] " + currentlyShowingLandmark.totalPopulation.ToString();
            text += "\n[b]Settlement Population: [/b] " + currentlyShowingLandmark.civilians.ToString();
            text += "\n[b]Civilians: [/b] ";
            foreach (KeyValuePair<RACE, int> kvp in currentlyShowingLandmark.civiliansByRace) {
                if (kvp.Value > 0) {
                    text += "\n" + kvp.Key.ToString() + " - " + kvp.Value.ToString();
                }
            }
            text += "\n[b]Population Growth: [/b] " + (currentlyShowingLandmark.totalPopulation * currentlyShowingLandmark.location.region.populationGrowth).ToString();

            if (currentlyShowingLandmark is Settlement) {
                text += "\n[b]Quest Board: [/b] ";
                Settlement settlement = (Settlement)currentlyShowingLandmark;
                if (settlement.questBoard.Count > 0) {
                    for (int i = 0; i < settlement.questBoard.Count; i++) {
                        Quest currQuest = settlement.questBoard[i];
                        text += "\n" + currQuest.urlName;
                        if (currQuest.questType == QUEST_TYPE.EXPLORE_REGION) {
                            text += " " + ((ExploreRegion)currQuest).regionToExplore.centerOfMass.tileName;
                        } else if (currQuest.questType == QUEST_TYPE.EXPLORE_TILE) {
                            text += " " + ((ExploreTile)currQuest).landmarkToExplore.location.tileName;
                        } else if (currQuest.questType == QUEST_TYPE.BUILD_STRUCTURE) {
                            text += " " + ((BuildStructure)currQuest).target.tileName;
						} else if (currQuest.questType == QUEST_TYPE.OBTAIN_MATERIAL) {
							text += " " + ((ObtainMaterial)currQuest).materialToObtain.ToString();
						} else if (currQuest.questType == QUEST_TYPE.SAVE_LANDMARK) {
							text += " " + ((SaveLandmark)currQuest).target.location.tileName;
						}
                        //						else {
                        //                            text += "[/url]";
                        //                        }
                        if (currQuest.isAccepted) {
                            text += " - A";
                            text += " (" + currQuest.assignedParty.name + ")";
                        } else {
                            text += " - N";
                        }
                    }
                }
            }
        }

		text += "\n[b]Characters At Landmark: [/b] ";
        if (currentlyShowingLandmark.charactersAtLocation.Count > 0) {
			for (int i = 0; i < currentlyShowingLandmark.charactersAtLocation.Count; i++) {
                object currObject = currentlyShowingLandmark.charactersAtLocation[i];
                if (currObject is ECS.Character) {
					ECS.Character currChar = (ECS.Character)currObject;
					text += "\n" + currChar.urlName + " - " + (currChar.characterClass != null ? currChar.characterClass.className : "NONE") + "/" + (currChar.role != null ? currChar.role.roleType.ToString () : "NONE");
					if (currChar.currentTask != null) {
						if (currChar.currentTask.taskType == TASK_TYPE.QUEST) {
							Quest currQuest = (Quest)currChar.currentTask;
							text += " (" + currQuest.urlName + ")";
						} else {
							text += " (" + currChar.currentTask.taskType.ToString () + ")";
						}
					}
				} else if (currObject is Party) {
					Party currParty = (Party)currObject;
					text += "\n" + currParty.urlName + " - " + (currParty.currentTask != null ? currParty.currentTask.ToString () : "NONE");
				}
			}
		} else {
			text += "NONE";
		}
        text += "\n[b]Characters At Tile: [/b] ";
        if (currentlyShowingLandmark.location.charactersAtLocation.Count > 0) {
            for (int i = 0; i < currentlyShowingLandmark.location.charactersAtLocation.Count; i++) {
                object currObject = currentlyShowingLandmark.location.charactersAtLocation[i];
                if (currObject is ECS.Character) {
                    ECS.Character currChar = (ECS.Character)currObject;
                    text += "\n" + currChar.urlName + " - " + (currChar.characterClass != null ? currChar.characterClass.className : "NONE") + "/" + (currChar.role != null ? currChar.role.roleType.ToString() : "NONE");
                    if (currChar.currentTask != null) {
                        if (currChar.currentTask.taskType == TASK_TYPE.QUEST) {
                            Quest currQuest = (Quest)currChar.currentTask;
                            text += " (" + currQuest.urlName + ")";
                        } else {
                            text += " (" + currChar.currentTask.taskType.ToString() + ")";
                        }
                    }
                } else if (currObject is Party) {
                    Party currParty = (Party)currObject;
                    text += "\n" + currParty.urlName + " - " + (currParty.currentTask != null ? currParty.currentTask.ToString() : "NONE");
                }
            }
        } else {
            text += "NONE";
        }

		text += "\n[b]Prisoners:[/b] ";
		if(currentlyShowingLandmark.prisoners.Count > 0){
			for (int i = 0; i < currentlyShowingLandmark.prisoners.Count; i++) {
				ECS.Character prisoner = currentlyShowingLandmark.prisoners [i];
				text += "\n" + prisoner.prisonerName;
			}
		} else {
			text += "NONE";
		}

        text += "\n[b]Materials: [/b] ";
        Dictionary<MATERIAL, MaterialValues> materials = currentlyShowingLandmark.materialsInventory;
        if (materials.Sum(x => x.Value.totalCount) > 0) {
            foreach (KeyValuePair<MATERIAL, MaterialValues> kvp in materials) {
                if (kvp.Value.totalCount > 0) {
                    text += "\n" + kvp.Key.ToString() + " - " + kvp.Value.totalCount;
                }
            }
        } else {
            text += "NONE";
        }
        text += "\n[b]Technologies: [/b] ";
        List<TECHNOLOGY> availableTech = currentlyShowingLandmark.technologies.Where(x => x.Value == true).Select(x => x.Key).ToList();
        if (availableTech.Count > 0) {
            for (int i = 0; i < availableTech.Count; i++) {
                TECHNOLOGY currTech = availableTech[i];
                text += "\n" + currTech.ToString();
                //if (i + 1 != availableTech.Count) {
                //    text += ", ";
                //}
            }
        } else {
            text += "NONE";
        }
        if (currentlyShowingLandmark.landmarkEncounterable != null){
			text += "\n[b]Encounterable: [/b]" + currentlyShowingLandmark.landmarkEncounterable.encounterName;
		}
		//if(currentlyShowingLandmark is Settlement && currentlyShowingLandmark.specificLandmarkType == LANDMARK_TYPE.CITY && currentlyShowingLandmark.owner != null){
		//	text += "\n[b]Parties: [/b] ";
		//	if (PartyManager.Instance.allParties.Count > 0) {
		//		for (int i = 0; i < PartyManager.Instance.allParties.Count; i++) {
		//			Party currParty = PartyManager.Instance.allParties[i];
		//			text += "\n" + currParty.urlName + " O: " + currParty.isOpen + " F: " + currParty.isFull;
		//			if(currParty.currentTask != null) {
  //                      if (currParty.currentTask.taskType == TASK_TYPE.QUEST) {
  //                          Quest currQuest = (Quest)currParty.currentTask;
  //                          text += " (" + currQuest.urlName + ")";
  //                          if (currQuest.isDone) {
  //                              text += "(Done)";
  //                          } else {
  //                              if (currParty.isOpen || currQuest.isWaiting) {
  //                                  text += "(Forming Party)";
  //                              } else {
  //                                  text += "(In Progress)";
  //                                  if (currQuest.currentAction != null) {
  //                                      text += "(" + currQuest.currentAction.GetType().ToString() + ")";
  //                                  }
  //                              }
  //                          }
  //                      } else {
  //                          text += " (" + currParty.currentTask.taskType.ToString() + ")";
  //                          if (currParty.currentTask.isDone) {
  //                              text += "(Done)";
  //                          } else {
  //                              text += "(In Progress)";
  //                          }
  //                      }
		//			}
		//			text += "\n     Leader: " + currParty.partyLeader.urlName;
  //                  if(currParty.partyMembers.Count > 2) {
  //                      text += "\n          Members:";
  //                  }
		//			for (int j = 0; j < currParty.partyMembers.Count; j++) {
		//				ECS.Character currMember = currParty.partyMembers[j];
		//				if(currMember.id != currParty.partyLeader.id) {
		//					text += "\n          " + currMember.urlName;
		//				}
		//			}
  //                  text += "\n";
		//		}
		//	} else {
		//		text += "NONE";
		//	}
		//}
       
        settlementInfoLbl.text = text;
        infoScrollView.UpdatePosition();

		UpdateHistoryInfo ();
    }
	private void UpdateHistoryInfo(){
		string text = string.Empty;
		if (currentlyShowingLandmark.history.Count > 0) {
			for (int i = 0; i < currentlyShowingLandmark.history.Count; i++) {
				if(i > 0){
					text += "\n";
				}
				text += currentlyShowingLandmark.history[i];
			}
		} else {
			text += "NONE";
		}

		settlementHistoryInfoLbl.text = text;
		historyScrollView.UpdatePosition();
	}
	public void OnClickCloseBtn(){
//		UIManager.Instance.playerActionsUI.HidePlayerActionsUI ();
		HideMenu ();
	}

	public void OnClickExpandBtn(){
		currentlyShowingLandmark.owner.internalQuestManager.CreateExpandQuest(currentlyShowingLandmark);
		expandBtnGO.SetActive (false);
	}
	public void OnClickExploreRegionBtn(){
		currentlyShowingLandmark.location.region.centerOfMass
            .landmarkOnTile.owner.internalQuestManager.CreateExploreTileQuest(currentlyShowingLandmark);
        exploreBtnGO.SetActive(false);
    }
    //public void OnClickBuildStructureBtn() {
    //    currentlyShowingLandmark.location.region.centerOfMass
    //        .landmarkOnTile.owner.internalQuestManager.CreateBuildStructureQuest(currentlyShowingLandmark);
    //    buildStructureBtnGO.SetActive(false);
    //}
    private void ShowPlayerActions(){
		expandBtnGO.SetActive (CanExpand());
		exploreBtnGO.SetActive (CanExploreTile ());
        //buildStructureBtnGO.SetActive(CanBuildStructure());
    }
	private void HidePlayerActions(){
		expandBtnGO.SetActive (false);
		exploreBtnGO.SetActive (false);
	}
	private bool CanExpand(){
		if(isShowing && currentlyShowingLandmark != null && currentlyShowingLandmark is Settlement){
			Settlement settlement = (Settlement)currentlyShowingLandmark;
			if(settlement.owner != null && settlement.owner.factionType == FACTION_TYPE.MAJOR){
                Construction constructionData = ProductionManager.Instance.GetConstructionDataForCity();
                if (settlement.CanAffordConstruction(constructionData) && settlement.HasAdjacentUnoccupiedTile() && !settlement.owner.internalQuestManager.AlreadyHasQuestOfType(QUEST_TYPE.EXPAND, settlement)){
					return true;
				}
			}
		}
		return false;
	}

	private bool CanExploreTile(){
		if(isShowing && currentlyShowingLandmark != null && !currentlyShowingLandmark.isExplored
			&& currentlyShowingLandmark.owner == null && currentlyShowingLandmark.location.region.centerOfMass.isOccupied 
            && !currentlyShowingLandmark.location.region.centerOfMass
            .landmarkOnTile.owner.internalQuestManager.AlreadyHasQuestOfType(QUEST_TYPE.EXPLORE_TILE, currentlyShowingLandmark)) {
			return true;
		}
		return false;
	}

    //private bool CanBuildStructure() {
    //    if (isShowing && currentlyShowingLandmark != null && currentlyShowingLandmark.location.region.owner != null 
    //        && currentlyShowingLandmark.owner == null && !currentlyShowingLandmark.location.HasStructure()
    //        && !currentlyShowingLandmark.location.region.owner.internalQuestManager.AlreadyHasQuestOfType(QUEST_TYPE.BUILD_STRUCTURE, currentlyShowingLandmark)
    //        && currentlyShowingLandmark is ResourceLandmark) {
    //        Settlement settlement = currentlyShowingLandmark.location.region.centerOfMass.landmarkOnTile as Settlement;
    //        ResourceLandmark resourceLandmark = currentlyShowingLandmark as ResourceLandmark;
    //        Construction constructionData = ProductionManager.Instance.GetConstruction(resourceLandmark.materialData.structure.name);
    //        if (settlement.CanAffordConstruction(constructionData) && settlement.HasTechnology(Utilities.GetNeededTechnologyForMaterial(resourceLandmark.materialOnLandmark))) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
}
