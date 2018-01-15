using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SettlementInfoUI : UIMenu {

    private bool isShowing;

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TweenPosition tweenPos;
    [SerializeField] private UILabel settlementInfoLbl;

    private Settlement currentlyShowingSettlement;

    internal override void Initialize() {
        Messenger.AddListener("UpdateUI", UpdateSettlementInfo);
        tweenPos.AddOnFinished(() => UpdateSettlementInfo());
    }

    public void ShowSettlementInfo() {
        isShowing = true;
        tweenPos.PlayForward();
    }
    public void HideSettlementInfo() {
        isShowing = false;
        tweenPos.PlayReverse();
    }

    public void SetSettlementAsActive(Settlement settlement) {
        currentlyShowingSettlement = settlement;
        if (isShowing) {
            UpdateSettlementInfo();
        }
    }

    public void UpdateSettlementInfo() {
        if(currentlyShowingSettlement == null) {
            return;
        }
        string text = string.Empty;
        text += "[b]Location:[/b] " + currentlyShowingSettlement.location.name;

        if (currentlyShowingSettlement.owner != null) {
            text += "\n[b]Owner:[/b] " + currentlyShowingSettlement.owner.name + "/" + currentlyShowingSettlement.owner.race.ToString();
            text += "\n[b]Total Population: [/b] " + currentlyShowingSettlement.totalPopulation.ToString();
            text += "\n[b]Civilian Population: [/b] " + currentlyShowingSettlement.civilians.ToString();
            text += "\n[b]Population Growth: [/b] " + (currentlyShowingSettlement.totalPopulation * currentlyShowingSettlement.location.region.populationGrowth).ToString();
            text += "\n[b]Characters: [/b] ";
            if (currentlyShowingSettlement.charactersOnLandmark.Count > 0) {
                for (int i = 0; i < currentlyShowingSettlement.charactersOnLandmark.Count; i++) {
                    ECS.Character currChar = currentlyShowingSettlement.charactersOnLandmark[i];
                    text += "\n" + currChar.name + " - " + currChar.characterClass.className + "/" + currChar.role.roleType.ToString();
                    if (currChar.currentQuest != null) {
                        text += " (" + currChar.currentQuest.questType.ToString() + ")";
                    }
                }
            } else {
                text += "NONE";
            }

            //text += "\n[b]ECS.Character Caps: [/b] ";
            //for (int i = 0; i < LandmarkManager.Instance.characterProductionWeights.Count; i++) {
            //    CharacterProductionWeight currWweight = LandmarkManager.Instance.characterProductionWeights[i];
            //    bool isCapReached = false;
            //    for (int j = 0; j < currWweight.productionCaps.Count; j++) {
            //        CharacterProductionCap cap = currWweight.productionCaps[j];
            //        if (cap.IsCapReached(currWweight.role, currentlyShowingSettlement.owner)) {
            //            isCapReached = true;
            //            break;
            //        }
            //    }
            //    text += "\n" + currWweight.role.ToString() + " - " + isCapReached.ToString();
            //}

            text += "\n[b]Active Quests: [/b] ";
            if (currentlyShowingSettlement.owner.internalQuestManager.activeQuests.Count > 0) {
                for (int i = 0; i < currentlyShowingSettlement.owner.internalQuestManager.activeQuests.Count; i++) {
                    Quest currQuest = currentlyShowingSettlement.owner.internalQuestManager.activeQuests[i];
                    text += "\n" + currQuest.questType.ToString();
                    if (currQuest.questType == QUEST_TYPE.EXPLORE_REGION) {
                        text += " " + ((ExploreRegion)currQuest).regionToExplore.centerOfMass.name;
                    }
                    if (currQuest.isAccepted) {
                        text += " - A";
                    } else {
                        text += " - N";
                    }
                    
                }
            } else {
                text += "NONE";
            }
        }
        text += "\n[b]Technologies: [/b] ";
        List<TECHNOLOGY> availableTech = currentlyShowingSettlement.technologies.Where(x => x.Value == true).Select(x => x.Key).ToList();
        if (availableTech.Count > 0) {
            text += "\n";
            for (int i = 0; i < availableTech.Count; i++) {
                TECHNOLOGY currTech = availableTech[i];
                text += currTech.ToString();
                if (i + 1 != availableTech.Count) {
                    text += ", ";
                }
            }
        } else {
            text += "NONE";
        }

        settlementInfoLbl.text = text;
    }
}
