using UnityEngine;
using System.Collections;

public class PlayerEventItem : MonoBehaviour {

    [SerializeField] private EVENT_TYPES eventType;

    [SerializeField] private UILabel eventBtnLbl;
    [SerializeField] private UILabel eventDescriptionLbl;

    public void SetEvent(EVENT_TYPES eventType) {
        this.eventType = eventType;
        string fileName = Utilities.FirstLetterToUpperCase(eventType.ToString().Replace("_", string.Empty).ToLower());
        this.eventBtnLbl.text = Utilities.FirstLetterToUpperCase(eventType.ToString().Replace('_', ' ').ToLower());
        this.eventDescriptionLbl.text = LocalizationManager.Instance
            .GetLocalizedValue("Events", fileName, "player_event_description");
    }

    public void OnClickEventItem() {
        //TODO: Put Action to perform on click event item.
    }
}
