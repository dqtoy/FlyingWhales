﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class LandmarkInfoUI : UIMenu {

    private const int MAX_HISTORY_LOGS = 20;

    public bool isWaitingForAttackTarget;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI structureTypeLbl;
    [SerializeField] private Image structureIcon;
    [SerializeField] private Image areaIcon;
    [SerializeField] private Image factionIcon;
    [SerializeField] private Slider healthProgressBar;

    [Space(10)]
    [Header("Characters")]
    [SerializeField] private GameObject charactersGO;
    [SerializeField] private GameObject landmarkCharacterPrefab;
    [SerializeField] private ScrollRect charactersScrollView;

    [Space(10)]
    [Header("Items")]
    [SerializeField] private GameObject itemsGO;
    private ItemContainer[] itemContainers;

    [Space(10)]
    [Header("Events")]
    [SerializeField] private GameObject eventsGO;
    [SerializeField] private ScrollRect eventsScrollView;

    [Space(10)]
    [Header("Logs")]
    [SerializeField] private GameObject logHistoryPrefab;
    [SerializeField] private ScrollRect historyScrollView;
    [SerializeField] private Color evenLogColor;
    [SerializeField] private Color oddLogColor;

    [Space(10)]
    [Header("Settlement")]
    [SerializeField] private GameObject attackButtonGO;
    [SerializeField] private Toggle attackBtnToggle;

    private LogHistoryItem[] logHistoryItems;

    internal BaseLandmark currentlyShowingLandmark {
        get { return _data as BaseLandmark; }
    }

    internal override void Initialize() {
        base.Initialize();
        SetWaitingForAttackState(false);
        //Messenger.AddListener("UpdateUI", UpdateLandmarkInfo);
        Messenger.AddListener<object>(Signals.HISTORY_ADDED, UpdateHistory);

        logHistoryItems = new LogHistoryItem[MAX_HISTORY_LOGS];
        //populate history logs table
        for (int i = 0; i < MAX_HISTORY_LOGS; i++) {
            GameObject newLogItem = ObjectPoolManager.Instance.InstantiateObjectFromPool(logHistoryPrefab.name, Vector3.zero, Quaternion.identity, historyScrollView.content);
            logHistoryItems[i] = newLogItem.GetComponent<LogHistoryItem>();
            newLogItem.transform.localScale = Vector3.one;
            newLogItem.SetActive(true);
        }
        for (int i = 0; i < logHistoryItems.Length; i++) {
            logHistoryItems[i].gameObject.SetActive(false);
        }

        Messenger.AddListener<StructureObj, ObjectState>(Signals.STRUCTURE_STATE_CHANGED, OnStructureChangedState);
    }
    public override void ShowMenu() {
        base.ShowMenu();
        UpdateLandmarkInfo();
        UpdateAllHistoryInfo();
        ShowAttackButton();
    }
    public override void SetData(object data) {
        base.SetData(data);
        UIManager.Instance.hexTileInfoUI.SetData((data as BaseLandmark).tileLocation);
        if (isShowing) {
            UpdateLandmarkInfo();
        }
    }

    public void UpdateLandmarkInfo() {
  //      if(currentlyShowingLandmark == null) {
  //          return;
  //      }
  //      string text = string.Empty;
		//if (currentlyShowingLandmark.landmarkName != string.Empty) {
		//	text += "<b>Name:</b> " + currentlyShowingLandmark.landmarkName + "\n";
		//}
		//text += "<b>Location:</b> " + currentlyShowingLandmark.tileLocation.urlName;
  //      if (currentlyShowingLandmark.tileLocation.areaOfTile != null) {
  //          text += " <b>Area:</b> " + currentlyShowingLandmark.tileLocation.areaOfTile.name;
  //          if (currentlyShowingLandmark.tileLocation.areaOfTile.owner != null) {
  //              text += " (Owner: " + currentlyShowingLandmark.tileLocation.areaOfTile.owner.name + ")"; 
  //          }
  //      }
  //      text += "\n<b>Landmark Type:</b> " + currentlyShowingLandmark.landmarkObj.objectName + " (" + currentlyShowingLandmark.landmarkObj.currentState.stateName + ")";
  //      text += "\n<b>HP:</b> " + currentlyShowingLandmark.landmarkObj.currentHP.ToString() + "/" + currentlyShowingLandmark.landmarkObj.maxHP.ToString();
  //      text += "\n<b>Durability:</b> " + currentlyShowingLandmark.currDurability.ToString() + "/" + currentlyShowingLandmark.totalDurability.ToString();
  //      text += "\n<b>Can Be Occupied:</b> " + currentlyShowingLandmark.canBeOccupied.ToString();
		//text += "\n<b>Is Occupied:</b> " + currentlyShowingLandmark.isOccupied.ToString();

  //      if (currentlyShowingLandmark.owner != null) {
  //          text += "\n<b>Owner:</b> " + currentlyShowingLandmark.owner.urlName;
  //          text += "\n<b>Settlement Population: </b> " + currentlyShowingLandmark.civilianCount.ToString();
  //      }

  //      text += "\n<b>Connections: </b> ";
  //      if (currentlyShowingLandmark.connections.Count > 0) {
  //          for (int i = 0; i < currentlyShowingLandmark.connections.Count; i++) {
  //              BaseLandmark connection = currentlyShowingLandmark.connections[i];
  //              text += "\n" + connection.urlName;
  //          }
  //      } else {
  //          text += "NONE";
  //      }

  //      text += "\n<b>Parties At Landmark: </b> ";
  //      if (currentlyShowingLandmark.charactersAtLocation.Count > 0) {
		//	for (int i = 0; i < currentlyShowingLandmark.charactersAtLocation.Count; i++) {
  //              NewParty currObject = currentlyShowingLandmark.charactersAtLocation[i];
  //              if (currObject is CharacterParty) {
  //                  CharacterParty currChar = currObject as CharacterParty;
  //                  text += "\n" + currChar.urlName; // + " - " + (currChar.characterClass != null ? currChar.characterClass.className : "NONE") + "/" + (currChar.role != null ? currChar.role.roleType.ToString() : "NONE");
  //              } else if (currObject is MonsterParty) {
  //                  //MonsterParty monster = currObject as MonsterParty;
  //                  text += "\n" + currObject.name;
  //              }
  //          }
		//} else {
		//	text += "NONE";
		//}
  //      text += "\n<b>Characters At Tile: </b> ";
		//if (currentlyShowingLandmark.tileLocation.charactersAtLocation.Count > 0) {
		//	for (int i = 0; i < currentlyShowingLandmark.tileLocation.charactersAtLocation.Count; i++) {
		//		object currObject = currentlyShowingLandmark.tileLocation.charactersAtLocation[i];
  //              if (currObject is ECS.Character) {
  //                  ECS.Character currChar = (ECS.Character)currObject;
  //                  text += "\n" + currChar.urlName + " - " + (currChar.characterClass != null ? currChar.characterClass.className : "NONE") + "/" + (currChar.role != null ? currChar.role.roleType.ToString() : "NONE");
  //                  if (currChar.party.actionData.currentAction != null) {
  //                      text += " (" + currChar.party.actionData.currentAction.actionData.actionName + ")";
  //                  }
  //              } 
  //              //else if (currObject is Party) {
  //              //    Party currParty = (Party)currObject;
  //              //    text += "\n" + currParty.urlName + " - " + (currParty.currentAction != null ? currParty.currentAction.ToString() : "NONE");
  //              //}
  //          }
  //      } else {
  //          text += "NONE";
  //      }

		//text += "\n<b>Traces:</b> ";
		//if(currentlyShowingLandmark.characterTraces.Count > 0){
		//	foreach (ECS.Character character in currentlyShowingLandmark.characterTraces.Keys) {
		//		text += "\n" + character.urlName + ", " + currentlyShowingLandmark.characterTraces[character].ToStringDate();
		//	}
		//} else {
		//	text += "NONE";
		//}

		//text += "\n<b>Items In Landmark: </b> ";
		//if (currentlyShowingLandmark.itemsInLandmark.Count > 0) {
		//	for (int i = 0; i < currentlyShowingLandmark.itemsInLandmark.Count; i++) {
		//		ECS.Item item = currentlyShowingLandmark.itemsInLandmark[i];
		//		text += "\n" + item.itemName + " (" + ((item.owner == null ? "NONE" : item.owner.name)) + ")";
		//	}
		//} else {
		//	text += "NONE";
		//}

  //      if (currentlyShowingLandmark.specificLandmarkType == LANDMARK_TYPE.SHOP) {
  //          text += "\n<b>Items In Shop: </b> ";
  //          Shop shop = currentlyShowingLandmark.landmarkObj as Shop;
  //          if (shop.availableItems.Count > 0) {
  //              for (int i = 0; i < shop.availableItems.Count; i++) {
  //                  text += "\n" + shop.availableItems[i];
  //              }
  //          } else {
  //              text += "NONE";
  //          }
            
  //      }
       
  //      landmarkInfoLbl.text = text;
    }

    #region Log History
    private void UpdateHistory(object obj) {
        if (obj is BaseLandmark && currentlyShowingLandmark != null && (obj as BaseLandmark).id == currentlyShowingLandmark.id) {
            UpdateAllHistoryInfo();
        }
    }
    private void UpdateAllHistoryInfo() {
        List<Log> landmarkHistory = new List<Log>(currentlyShowingLandmark.history.OrderByDescending(x => x.id));
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            Log currLog = landmarkHistory.ElementAtOrDefault(i);
            if (currLog != null) {
                currItem.gameObject.SetActive(true);
                currItem.SetLog(currLog);
                if (Utilities.IsEven(i)) {
                    currItem.SetLogColor(evenLogColor);
                } else {
                    currItem.SetLogColor(oddLogColor);
                }
            } else {
                currItem.gameObject.SetActive(false);
            }
        }
        //if (this.gameObject.activeInHierarchy) {
        //    StartCoroutine(UIManager.Instance.RepositionTable(logHistoryTable));
        //    StartCoroutine(UIManager.Instance.RepositionScrollView(historyScrollView));
        //}
    }
    private bool IsLogAlreadyShown(Log log) {
        for (int i = 0; i < logHistoryItems.Length; i++) {
            LogHistoryItem currItem = logHistoryItems[i];
            if (currItem.log != null) {
                if (currItem.log.id == log.id) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    public void OnClickCloseBtn(){
		HideMenu ();
	}
    public void CenterOnLandmark() {
        currentlyShowingLandmark.CenterOnLandmark();
    }

    #region Attack Landmark
    private void ShowAttackButton() {
        BaseLandmark landmark = currentlyShowingLandmark;
        if (!landmark.isAttackingAnotherLandmark) {
            if ((landmark.landmarkObj.specificObjectType == LANDMARK_TYPE.GARRISON || landmark.landmarkObj.specificObjectType == LANDMARK_TYPE.DEMONIC_PORTAL) && landmark.landmarkObj.currentState.stateName == "Ready") {
                attackButtonGO.SetActive(true);
                attackBtnToggle.isOn = false;
                SetWaitingForAttackState(false);
            } else {
                attackButtonGO.SetActive(false);
            }
        } else {
            attackButtonGO.SetActive(false);
        }
        //SetAttackButtonState(false);
    }
    //public void ToggleAttack() {
    //    SetWaitingForAttackState(!isWaitingForAttackTarget);
    //}
    public void SetWaitingForAttackState(bool state) {
        attackBtnToggle.isOn = state;
    }
    public void OnSetAttackState(bool state) {
        isWaitingForAttackTarget = state;
        if (isWaitingForAttackTarget) {
            GameManager.Instance.SetCursorToTarget();
            OnStartWaitingForAttack();
        } else {
            GameManager.Instance.SetCursorToDefault();
            OnEndWaitingForAttack();
        }
    }
    //private void NotWaitingForAttackState() {
    //    attackBtnToggle.isOn = false;
    //    isWaitingForAttackTarget = false;
    //    GameManager.Instance.SetCursorToDefault();
    //}
    public void SetActiveAttackButtonGO(bool state) {
        attackButtonGO.SetActive(state);
        if (state) {
            SetWaitingForAttackState(false);
        }
    }
    #endregion

    private void OnStructureChangedState(StructureObj obj, ObjectState newState) {
        if (currentlyShowingLandmark == null) {
            return;
        }
        if (obj.objectLocation == null) {
            return;
        }
        if (obj.objectLocation.id == currentlyShowingLandmark.id) {
            if (newState.stateName.Equals("Ready")) {
                SetActiveAttackButtonGO(true);
            } else {
                SetActiveAttackButtonGO(false);
            }
        }
    }

    private void OnStartWaitingForAttack() {
        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OVER, TileHoverOver);
        Messenger.AddListener<HexTile>(Signals.TILE_HOVERED_OUT, TileHoverOut);
        Messenger.AddListener<HexTile>(Signals.TILE_RIGHT_CLICKED, TileRightClicked);
        Messenger.AddListener<BaseLandmark>(Signals.LANDMARK_ATTACK_TARGET_SELECTED, OnAttackTargetSelected);
    }
    private void TileHoverOver(HexTile tile) {
        if (tile.landmarkOnTile != null) {
            currentlyShowingLandmark.landmarkVisual.DrawPathTo(tile.landmarkOnTile);
        }
    }
    private void TileHoverOut(HexTile tile) {
        currentlyShowingLandmark.landmarkVisual.HidePathVisual();
    }
    private void OnAttackTargetSelected(BaseLandmark target) {
        Debug.Log(currentlyShowingLandmark.landmarkName + " will attack " + target.landmarkName);
        currentlyShowingLandmark.landmarkObj.AttackLandmark(target);
        SetWaitingForAttackState(false);
        SetActiveAttackButtonGO(false);
        Messenger.Broadcast(Signals.HIDE_POPUP_MESSAGE);
        //OnEndWaitingForAttack();
        //currentlyShowingLandmark.landmarkVisual.HidePathVisual();
        //SetWaitingForAttackState(false);
        //NotWaitingForAttackState();
    }
    private void TileRightClicked(HexTile tile) {
        SetWaitingForAttackState(false);
        Messenger.Broadcast(Signals.HIDE_POPUP_MESSAGE);
        //NotWaitingForAttackState();
    }
    private void OnEndWaitingForAttack() {
        if (this.gameObject.activeSelf) {
            currentlyShowingLandmark.landmarkVisual.HidePathVisual();
            Messenger.RemoveListener<HexTile>(Signals.TILE_HOVERED_OVER, TileHoverOver);
            Messenger.RemoveListener<HexTile>(Signals.TILE_HOVERED_OUT, TileHoverOut);
            Messenger.RemoveListener<HexTile>(Signals.TILE_RIGHT_CLICKED, TileRightClicked);
            Messenger.RemoveListener<BaseLandmark>(Signals.LANDMARK_ATTACK_TARGET_SELECTED, OnAttackTargetSelected);
        }
    }

    
}
