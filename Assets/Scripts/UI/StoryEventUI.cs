using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class StoryEventUI : MonoBehaviour {

    private StoryEvent currentEvent;

    [SerializeField] private TextMeshProUGUI eventTitleLbl;
    [SerializeField] private TextMeshProUGUI eventTextLbl;
    [SerializeField] private GameObject choicesParent;
    private EventChoiceItem[] choices;

    public void Initialize() {
        choices = Utilities.GetComponentsInDirectChildren<EventChoiceItem>(choicesParent);
    }

    public void ShowEvent(StoryEvent storyEvent) {
        UIManager.Instance.Pause();
        //I used flow similar to FTL: https://ftlwiki.com/wiki/Events_file_structure
        //1. All the effects of the event are executed
        //2. The text is displayed
        //3. If there are choices, they are displayed (otherwise just “Continue …”)
        //4. If the player made a choice in (3) that had an event attached: go to (1) for that event
        string additionalText = string.Empty;
        if (storyEvent.effects != null) {
            ExecuteEffects(storyEvent, out additionalText);
        }
        currentEvent = storyEvent;
        eventTitleLbl.text = storyEvent.name;
        eventTextLbl.text = storyEvent.text + additionalText;
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
        UIManager.Instance.Unpause();
        this.gameObject.SetActive(false);
    }

    private void OnChooseChoice(StoryEventChoice choice) {
        if (choice == StoryEventsManager.continueChoice) {
            CloseMenu();
        } else {
            if (choice.eventToExecute != null) {
                ShowEvent(choice.eventToExecute);
            } else {
                CloseMenu();
            }
        }
    }

    private void ExecuteEffects(StoryEvent storyEvent, out string additionalText) {
        additionalText = string.Empty;
        WeightedDictionary<StoryEventEffect> pooledEffects = new WeightedDictionary<StoryEventEffect>();
        for (int i = 0; i < storyEvent.effects.Length; i++) {
            StoryEventEffect currEFfect = storyEvent.effects[i];
            if (currEFfect.effectChance == 100) {
                //execute effect immediately.
                string text;
                StoryEventsManager.Instance.ExecuteEffect(currEFfect, out text);
                additionalText += " " + text;
            } else {
                pooledEffects.AddElement(currEFfect, currEFfect.effectChance);
            }
        }
        //if any pooled effects. Give one.
        if (pooledEffects.GetTotalOfWeights() > 0) {
            string text;
            StoryEventsManager.Instance.ExecuteEffect(pooledEffects.PickRandomElementGivenWeights(), out text);
            additionalText += " " + text;
        }
    }
}
