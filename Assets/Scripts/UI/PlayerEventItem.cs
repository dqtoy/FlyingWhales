using UnityEngine;
using System.Collections;

public class PlayerEventItem : MonoBehaviour {

    [SerializeField] private EVENT_TYPES eventType;

    [SerializeField] private UILabel eventBtnLbl;
    [SerializeField] private UILabel eventDescriptionLbl;

    public void SetEvent(EVENT_TYPES eventType) {
        this.eventType = eventType;
        string[] words = eventType.ToString().Split('_');
        string fileName = string.Empty;
        for (int i = 0; i < words.Length; i++) {
            fileName += Utilities.FirstLetterToUpperCase(words[i].ToLower());
        }
        this.eventBtnLbl.text = Utilities.FirstLetterToUpperCase(eventType.ToString().Replace('_', ' ').ToLower());
        this.eventDescriptionLbl.text = LocalizationManager.Instance
            .GetLocalizedValue("Events", fileName, "player_event_description");
    }

    public void OnClickEventItem() {
        UIManager.Instance.ShowInterveneEvent(this.eventType);
    }
}
