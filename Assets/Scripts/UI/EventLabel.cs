using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;
using ECS;

public class EventLabel : MonoBehaviour, IPointerClickHandler {

    [SerializeField] private LogItem logItem;
	[SerializeField] private TextMeshProUGUI text;

    private void Awake() {
        if (text == null) {
            text = gameObject.GetComponent<TextMeshProUGUI>();
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        //Debug.Log("OnPointerEnter()");
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
        if (linkIndex != -1) {
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
            if (logItem == null) {
                string linkText = linkInfo.GetLinkID();
                string id = linkText.Substring(0, linkText.IndexOf('_'));
                int idToUse = int.Parse(id);
                if (linkText.Contains("_faction")) {
                    Faction faction = UIManager.Instance.characterInfoUI.currentlyShowingCharacter.faction;
                    if (faction != null) {
                        UIManager.Instance.ShowFactionInfo(faction);
                    }
                } else if (linkText.Contains("_landmark")) {
                    BaseLandmark landmark = LandmarkManager.Instance.GetLandmarkByID(idToUse);
                    UIManager.Instance.ShowLandmarkInfo(landmark);
                    //if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.home.id == idToUse) {
                    //    UIManager.Instance.ShowLandmarkInfo(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.home);
                    //}
                } else if (linkText.Contains("_party")) {
                    CharacterParty party = UIManager.Instance.characterInfoUI.currentlyShowingCharacter.party;
                    UIManager.Instance.ShowPartyInfo(party);
                } else if (linkText.Contains("_character")) {
                    Character character = CharacterManager.Instance.GetCharacterByID(idToUse);
                    if (character != null) {
                        UIManager.Instance.ShowCharacterInfo(character);
                    }
                } else if (linkText.Contains("_combat")) {
                    if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null) {
                        if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter.combatHistory.ContainsKey(idToUse)) {
                            UIManager.Instance.ShowCombatLog(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.combatHistory[idToUse]);
                        }
                    }
                } 
                //else if (linkText.Contains("_monster")) {
                //        if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter.combatHistory.ContainsKey(idToUse)) {
                //            UIManager.Instance.ShowCombatLog(UIManager.Instance.characterInfoUI.currentlyShowingCharacter.combatHistory[idToUse]);
                //        }
                //    }
                //}
            } else {
                int indexToUse = int.Parse(linkInfo.GetLinkID());
                LogFiller lf = logItem.log.fillers[indexToUse];

                if (lf.obj != null) {
                    if (lf.obj is Character) {
                        UIManager.Instance.ShowCharacterInfo(lf.obj as Character);
                    } else if (lf.obj is Party) {
                        UIManager.Instance.ShowCharacterInfo((lf.obj as Party).partyLeader);
                    } else if (lf.obj is BaseLandmark) {
                        UIManager.Instance.ShowLandmarkInfo(lf.obj as BaseLandmark);
                    } else if (lf.obj is Combat) {
                        UIManager.Instance.ShowCombatLog(lf.obj as Combat);
                    } else if (lf.obj is NewParty) {
                        NewParty party = lf.obj as NewParty;
                        if(party.icharacters.Count > 1) {
                            UIManager.Instance.ShowPartyInfo(party);
                        } else if (party.icharacters.Count == 1) {
                            if(party.icharacters[0] is Character) {
                                UIManager.Instance.ShowCharacterInfo(party.icharacters[0] as Character);
                            }
                        }
                    }
                }
            }
            
        }
    }

    //public void OnPointerClick(PointerEventData data) {
        
    //}

	//void OnClick(){
	//	UILabel lbl = GetComponent<UILabel>();
	//	string url = lbl.GetUrlAtPosition(UICamera.lastWorldPosition);

	//	if (!string.IsNullOrEmpty (url)) {
	//		int indexToUse = int.Parse (url);
 //           LogFiller lf = new LogFiller();
 //           if(logItem.GetComponent<NotificationItem>() != null) {
 //               lf = logItem.GetComponent<NotificationItem>().thisLog.fillers[indexToUse];
 //           } else if (logItem.GetComponent<LogHistoryItem>() != null) {
 //               lf = logItem.GetComponent<LogHistoryItem>().thisLog.fillers[indexToUse];
 //           }

 //           if (lf.obj != null) {
 //               if (lf.obj is ECS.Character) {
 //                   UIManager.Instance.ShowCharacterInfo(lf.obj as ECS.Character);
 //               } else if (lf.obj is Party) {
 //                   UIManager.Instance.ShowCharacterInfo((lf.obj as Party).partyLeader);
 //               } else if (lf.obj is BaseLandmark) {
 //                   UIManager.Instance.ShowLandmarkInfo(lf.obj as BaseLandmark);
 //               } else if (lf.obj is ECS.Combat) {
 //                   UIManager.Instance.ShowCombatLog(lf.obj as ECS.Combat);
 //               }
 //           }
	//	}
	//}
}
