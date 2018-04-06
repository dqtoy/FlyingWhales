using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class StorylineElement : MonoBehaviour {

    [SerializeField] private UILabel elementLbl;
    [SerializeField] private UIEventTrigger elementEventTrigger;

    private object element;
    private List<Log> logs;

    public void SetElement(object element, List<Log> log) {
        this.element = element;
        this.logs = log;
        if (element is Character) {
            elementLbl.text = (element as Character).name;
        } else if (element is Item) {
            elementLbl.text = (element as Item).itemName;
        } else if (element is BaseLandmark) {
            elementLbl.text = (element as BaseLandmark).landmarkName;
		} else if (element is string) {
			elementLbl.text = (element as string);
		}
        EventDelegate.Set(elementEventTrigger.onClick, OnClickElement);
        EventDelegate.Set(elementEventTrigger.onHoverOver, ShowInfo);
        EventDelegate.Set(elementEventTrigger.onHoverOut, UIManager.Instance.storylinesSummaryMenu.HideElementInfo);
    }

    private void ShowInfo() {
        string text = string.Empty;
        for (int i = 0; i < logs.Count; i++) {
            Log currLog = logs[i];
            text += Utilities.LogReplacer(currLog) + " ";
        }
        UIManager.Instance.storylinesSummaryMenu.ShowElementInfo(text);
    }
    
    private void OnClickElement() {
        if (element is Character) {
            UIManager.Instance.ShowCharacterInfo((element as Character));
        } else if (element is BaseLandmark) {
            UIManager.Instance.ShowLandmarkInfo((element as BaseLandmark));
		} else if (element is Item) {
			Item item = element as Item;
			if(item.possessor != null){
				if(item.possessor is Character){
					UIManager.Instance.ShowCharacterInfo((item.possessor as Character));
				}else{
					UIManager.Instance.ShowLandmarkInfo((item.possessor as BaseLandmark));
				}
			}
		}
    }
}
