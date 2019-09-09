using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheSpire : BaseLandmark {
    public INTERVENTION_ABILITY interventionAbilityToResearch { get; private set; }
    public int currentResearchTick { get; private set; }
    public int researchDuration { get; private set; }

    public TheSpire(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) { }

    public TheSpire(HexTile location, SaveDataLandmark data) : base(location, data) { }

    public void SetInterventionAbilityToResearch(INTERVENTION_ABILITY interventionAbility) {
        interventionAbilityToResearch = interventionAbility;
        if(interventionAbilityToResearch != INTERVENTION_ABILITY.NONE) {
            researchDuration = PlayerManager.Instance.allInterventionAbilitiesData[interventionAbilityToResearch].durationInTicks;
        } else {
            researchDuration = 0;
        }
    }
    public void StartResearchNewInterventionAbility() {
        currentResearchTick = 0;
        TimerHubUI.Instance.AddItem("Research for " + Utilities.NormalizeStringUpperCaseFirstLetters(interventionAbilityToResearch.ToString()), researchDuration - currentResearchTick, null);
        Messenger.AddListener(Signals.TICK_STARTED, PerTickInterventionAbility);
    }
    private void PerTickInterventionAbility() {
        currentResearchTick++;
        if (currentResearchTick >= researchDuration) {
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickInterventionAbility);
            PlayerManager.Instance.player.GainNewInterventionAbility(interventionAbilityToResearch, true);
        }
    }
}

public class SaveDataTheSpire: SaveDataLandmark {
    public INTERVENTION_ABILITY interventionAbilityToResearch;
    public int currentResearchTick;

    public override void Save(BaseLandmark landmark) {
        base.Save(landmark);
        TheSpire spire = landmark as TheSpire;
        interventionAbilityToResearch = spire.interventionAbilityToResearch;
        currentResearchTick = spire.currentResearchTick;
    }
}
