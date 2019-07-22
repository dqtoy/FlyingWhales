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
            ExecuteEffects(storyEvent, out additionalText, out collectedEffects);
        }
        currentEvent = storyEvent;
        if (isRootEvent) {
            rootEvent = storyEvent;
        }
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

    private void ExecuteEffects(StoryEvent storyEvent, out string additionalText, out List<StoryEventEffect> chosenEffects) {
        additionalText = string.Empty;
        chosenEffects = new List<StoryEventEffect>();
        WeightedDictionary<StoryEventEffect> pooledEffects = new WeightedDictionary<StoryEventEffect>();
        for (int i = 0; i < storyEvent.effects.Length; i++) {
            StoryEventEffect currEFfect = storyEvent.effects[i];
            if (currEFfect.effectChance == 100) {
                //add effect to chosen effects. These will be executed once the player has clicked a choice
                additionalText += " " + currEFfect.additionalText;
                chosenEffects.Add(currEFfect);
            } else {
                pooledEffects.AddElement(currEFfect, currEFfect.effectChance);
            }
        }
        //if any pooled effects. Give one.
        if (pooledEffects.GetTotalOfWeights() > 0) {
            StoryEventEffect currEffect = pooledEffects.PickRandomElementGivenWeights();
            additionalText += " " + currEffect.additionalText;
            chosenEffects.Add(currEffect);
        }
    }
}
