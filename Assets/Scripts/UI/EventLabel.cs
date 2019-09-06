using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using TMPro;

using UnityEngine.Events;

public class EventLabel : MonoBehaviour, IPointerClickHandler{

    [SerializeField] private LogItem logItem;
	[SerializeField] private TextMeshProUGUI text;
    [SerializeField] private bool allowClickAction = true;
    [SerializeField] private EventLabelHoverAction hoverAction;
    [SerializeField] private UnityEvent hoverOutAction;

    private Log log;


    private void Awake() {
        if (text == null) {
            text = gameObject.GetComponent<TextMeshProUGUI>();
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (!allowClickAction) {
            return;
        }
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
        if (linkIndex != -1) {
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
            if (logItem == null) {
                string linkText = linkInfo.GetLinkID();
                string id = linkText.Substring(0, linkText.IndexOf('_'));
                int idToUse = int.Parse(id);
                if (linkText.Contains("_faction")) {
                    Faction faction = UIManager.Instance.characterInfoUI.activeCharacter.faction;
                    if (faction != null) {
                        UIManager.Instance.ShowFactionInfo(faction);
                    }
                } else if (linkText.Contains("_character")) {
                    Character character = CharacterManager.Instance.GetCharacterByID(idToUse);
                    if (character != null) {
                        UIManager.Instance.ShowCharacterInfo(character);
                    }
                } else if (linkText.Contains("_combat")) {
                    if (UIManager.Instance.characterInfoUI.activeCharacter != null) {
                        if (UIManager.Instance.characterInfoUI.activeCharacter.combatHistory.ContainsKey(idToUse)) {
                            UIManager.Instance.ShowCombatLog(UIManager.Instance.characterInfoUI.activeCharacter.combatHistory[idToUse]);
                        }
                    }
                }
            } else if (logItem.log != null) {
                int indexToUse = int.Parse(linkInfo.GetLinkID());
                LogFiller lf = logItem.log.fillers[indexToUse];
                if (lf.obj != null) {
                    if (lf.obj is Character) {
                        UIManager.Instance.ShowCharacterInfo(lf.obj as Character);
                    } else if (lf.obj is Area) {
                        UIManager.Instance.ShowHextileInfo((lf.obj as Area).coreTile);
                    } else if (lf.obj is Faction) {
                        UIManager.Instance.ShowFactionInfo((lf.obj as Faction));
                    } else if (lf.obj is Minion) {
                        UIManager.Instance.ShowCharacterInfo((lf.obj as Minion).character);
                    } else if (lf.obj is Combat) {
                        UIManager.Instance.ShowCombatLog(lf.obj as Combat);
                    } else if (lf.obj is Party) {
                        Party party = lf.obj as Party;
                        UIManager.Instance.ShowCharacterInfo(party.mainCharacter);
                    } else if (lf.obj is IPointOfInterest) {
                        IPointOfInterest poi = lf.obj as IPointOfInterest;
                        if(poi is Character) {
                            UIManager.Instance.ShowCharacterInfo(poi as Character);
                        }else if (poi is TileObject) {
                            UIManager.Instance.ShowTileObjectInfo(poi as TileObject);
                        }
                    } else if (lf.obj is Region) {
                        Region region = lf.obj as Region;
                        UIManager.Instance.ShowHextileInfo(region.mainLandmark.tileLocation);
                    }
                }
            }
            
        }
    }

    public void SetLog(Log log) {
        this.log = log;
    }

    public void HoveringAction() {
        if (hoverAction == null) {
            return;
        }
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
        if (linkIndex != -1) {
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
            string linkText = linkInfo.GetLinkID();
            object obj = null;
            if (log == null) {
                int idToUse;
                if (!int.TryParse(linkText, out idToUse)) {
                    string id = linkText.Substring(0, linkText.IndexOf('_'));
                    idToUse = int.Parse(id);
                }
                if (linkText.Contains("_faction")) {
                    obj = UIManager.Instance.characterInfoUI.activeCharacter.faction;
                } else if (linkText.Contains("_landmark")) {
                    obj = LandmarkManager.Instance.GetLandmarkByID(idToUse);
                } else if (linkText.Contains("_character")) {
                    obj = CharacterManager.Instance.GetCharacterByID(idToUse);
                } else {
                    obj = idToUse;
                }
            } else {
                int idToUse = int.Parse(linkText);
                obj = log.fillers[idToUse].obj;
            }
            if (obj != null) {
                hoverAction.Invoke(obj);
                return;
            }
        }
        if (hoverOutAction != null) {
            hoverOutAction.Invoke();
        }
    }

    public void HoverOutAction() {
        if (hoverOutAction == null) {
            return;
        }
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
        if (linkIndex == -1) {
            hoverOutAction.Invoke();
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
    //               if (lf.obj is Character) {
    //                   UIManager.Instance.ShowCharacterInfo(lf.obj as Character);
    //               } else if (lf.obj is Party) {
    //                   UIManager.Instance.ShowCharacterInfo((lf.obj as Party).partyLeader);
    //               } else if (lf.obj is BaseLandmark) {
    //                   UIManager.Instance.ShowLandmarkInfo(lf.obj as BaseLandmark);
    //               } else if (lf.obj is Combat) {
    //                   UIManager.Instance.ShowCombatLog(lf.obj as Combat);
    //               }
    //           }
    //	}
    //}
}

[System.Serializable]
public class EventLabelHoverAction : UnityEvent<object> { }
