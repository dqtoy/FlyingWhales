using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StorylineElement : MonoBehaviour {

    [SerializeField] private UILabel elementLbl;
    [SerializeField] private UIEventTrigger elementEventTrigger;

    private object element;
    private List<Log> logs;

    public void SetElement(object element, List<Log> log) {
        this.element = element;
        this.logs = log;
        if (element is ECS.Character) {
            elementLbl.text = (element as ECS.Character).name;
        } else if (element is ECS.Item) {
            elementLbl.text = (element as ECS.Item).itemName;
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
        if (element is ECS.Character) {
            UIManager.Instance.ShowCharacterInfo((element as ECS.Character));
        } else if (element is BaseLandmark) {
            UIManager.Instance.ShowLandmarkInfo((element as BaseLandmark));
        }
    }
}
