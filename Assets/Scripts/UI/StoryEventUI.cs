using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class StoryEventUI : MonoBehaviour {

    private StoryEvent rootEvent;
    private StoryEvent currentEvent;

    [SerializeField] private TextMeshProUGUI eventTitleLbl;
    [SerializeField] private TextMeshProUGUI eventTextLbl;
    [SerializeField] private GameObject choicesParent;
    private EventChoiceItem[] choices;
    private List<StoryEventEffect> collectedEffects;
    public void Initialize() {
        choices = Utilities.GetComponentsInDirectChildren<EventChoiceItem>(choicesParent);
        collectedEffects = new List<StoryEventEffect>();
    }

    public void ShowEvent(StoryEvent storyEvent, bool isRootEvent = false) {
        UIManager.Instance.Pause();
        //I used flow similar to FTL: https://ftlwiki.com/wiki/Events_file_structure
        //1. All the effects of the event are collected.
        //2. The text is displayed
        //3. If there are choices, they are displayed (otherwise just “Continue …”)
        //4. Once the player clicks any choice. The collected effects are now executed, since almost all effects would not make sense if the player had not seen the event text beforehand.
        //5. If the player made a choice in (3) that had an event attached: go to (1) for that event
        string additionalText = string.Empty;
        collectedEffects.Clear();
        if (storyEvent.effects != null) {
            StoryEventsManager.Instance.CollectEffects(storyEvent, out additionalText, out collectedEffects);
        }
        currentEvent = storyEvent;
        if (isRootEvent) { rootEvent = storyEvent; }
        eventTitleLbl.text = storyEvent.name;
        Log textLog = CreateTextLog(storyEvent, additionalText);
        eventTextLbl.text = Utilities.LogReplacer(textLog);
        bool hasChoice = false;
        if (storyEvent.choices != null) {
            for (int i = 0; i < choices.Length; i++) {
                EventChoiceItem item = choices[i];
                StoryEventChoice currChoice = storyEvent.choices.ElementAtOrDefault(i);
                if (currChoice != null && StoryEventsManager.Instance.DoesPlayerMeetChoiceRequirement(currChoice)) {
                    hasChoice = true;
                    item.SetChoice(currChoice, OnChooseChoice);
                    item.gameObject.SetActive(true);
                } else {
                    item.gameObject.SetActive(false);
                }
            }
        } else {
            for (int i = 0; i < choices.Length; i++) {
                EventChoiceItem item = choices[i];
                item.gameObject.SetActive(false);
            }
        }
        if (!hasChoice) {
            //show continue, which will just close the UI
            choices[0].SetChoice(StoryEventsManager.continueChoice, OnChooseChoice);
            choices[0].gameObject.SetActive(true);
        }
        this.gameObject.SetActive(true);
    }
    public void CloseMenu() {
        if (rootEvent.trigger != STORY_EVENT_TRIGGER.END) {
            UIManager.Instance.Unpause();
        }
        this.gameObject.SetActive(false);
    }

    private void OnChooseChoice(StoryEventChoice choice) {
        if (choice == StoryEventsManager.continueChoice) {
            CloseMenu();
            ExecuteCollectedEffects();
        } else {
            if (choice.eventToExecute != null) {
                ExecuteCollectedEffects();
                ShowEvent(choice.eventToExecute);
            } else {
                CloseMenu();
                ExecuteCollectedEffects();
            }
        }
    }

    private void ExecuteCollectedEffects() {
        for (int i = 0; i < collectedEffects.Count; i++) {
            StoryEventEffect currEffect = collectedEffects[i];
            StoryEventsManager.Instance.ExecuteEffect(currEffect);
        }
    }

    private Log CreateTextLog(StoryEvent storyEvent, string additionalText) {
        string message = storyEvent.text + additionalText;
        message = message.Replace("[Lead]", "%00@");
        Log textLog = new Log(GameManager.Instance.Today(), message);
        textLog.AddToFillers(PlayerManager.Instance.player.currentMinionLeader, PlayerManager.Instance.player.currentMinionLeader.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        return textLog;
    }
   
}

