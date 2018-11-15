using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class ResearchScrollDesire : HiddenDesire {

    public SurrenderItemsQuest surrenderScrollsQuest { get; private set; }

    public ResearchScrollDesire(Character host) : base(HIDDEN_DESIRE.RESEARCH_SCROLL, host) {
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        _description = "Craves forbidden knowledge.";
    }
    public override void Awaken() {
        base.Awaken();
        //when awakened, create a new quest to obtain scrolls,
        surrenderScrollsQuest = new SurrenderItemsQuest(_host, "Scroll");
        List<BaseLandmark> questBoards = LandmarkManager.Instance.GetAllLandmarksWithQuestBoard();
        for (int i = 0; i < questBoards.Count; i++) {
            questBoards[i].questBoard.PostQuest(surrenderScrollsQuest);
        }
        //also check if character already has scrolls in inventory
        if (_host.HasItemLike("Scroll", 1)) {
            //if they do, schedule reseach event
            //GameEvent researchScrolls = EventManager.Instance.AddNewEvent(GAME_EVENT.RESEARCH_SCROLLS);
            //researchScrolls.Initialize(new List<Character>() { _host });
        }
        //also activate a listener for when the character obtains a new scroll
        Messenger.AddListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
    }
    public override void OnHostDied() {
        base.OnHostDied();
        if (_isAwakened) {
            Messenger.RemoveListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
        }
    }
    #endregion

    private void OnItemObtained(Item obtainedItem, Character character) {
        if (_host.id == character.id && obtainedItem.itemName.Contains("Scroll")) {
            //the priest obtained a scroll!
            if (!_host.HasEventScheduled(GAME_EVENT.RESEARCH_SCROLLS)) { //check if the character already has an event to research his/her scrolls
                //schedule a research scrolls event
                //GameEvent researchScrolls = EventManager.Instance.AddNewEvent(GAME_EVENT.RESEARCH_SCROLLS);
                //researchScrolls.Initialize(new List<Character>() { _host });
            }
        }
    }
}
