using UnityEngine;

public class TheProfane : BaseLandmark {
    public int currentCooldownTick { get; private set; }
    public int cooldownDuration { get; private set; }
    public bool isInCooldown {
        get { return currentCooldownTick < cooldownDuration; }
    }

    public TheProfane(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        cooldownDuration = GameManager.Instance.GetTicksBasedOnHour(8);
        currentCooldownTick = cooldownDuration;
    }

    public TheProfane(HexTile location, SaveDataLandmark data) : base(location, data) {
        cooldownDuration = GameManager.Instance.GetTicksBasedOnHour(8);
    }

    public void LoadSavedData(SaveDataTheProfane data) {
        if (data.currentCooldownTick < cooldownDuration) {
            StartCooldown();
        }
        currentCooldownTick = data.currentCooldownTick;
    }

    public void DoAction(Character targetCharacter, string action) {
        if (action == "Convert to cultist") {
            PlayerManager.Instance.player.AdjustMana(-GetConvertToCultistCost(targetCharacter));
            targetCharacter.traitContainer.AddTrait(targetCharacter,"Cultist");
        } else if (action == "Corrupt") {
            if (!targetCharacter.jobQueue.HasJob(JOB_TYPE.CORRUPT_CULTIST)) {
                CharacterStateJob job = new CharacterStateJob(JOB_TYPE.CORRUPT_CULTIST, CHARACTER_STATE.MOVE_OUT, targetCharacter);
                targetCharacter.jobQueue.AddJobInQueue(job);
            }
        } else if (action == "Sabotage Faction Quest") {
            (targetCharacter.faction.activeQuest as DivineInterventionQuest).CreateSabotageFactionnJob();
        } else if (action == "Destroy Supply") {
            if (!targetCharacter.jobQueue.HasJob(JOB_TYPE.DESTROY_SUPPLY)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DESTROY_SUPPLY, INTERACTION_TYPE.DESTROY_RESOURCE, targetCharacter.specificLocation.supplyPile, targetCharacter);
                job.SetIsStealth(true);
                targetCharacter.jobQueue.AddJobInQueue(job);
            }
        } else if (action == "Destroy Food") {
            if (!targetCharacter.jobQueue.HasJob(JOB_TYPE.DESTROY_FOOD)) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.DESTROY_FOOD, INTERACTION_TYPE.DESTROY_RESOURCE, targetCharacter.specificLocation.foodPile, targetCharacter);
                job.SetIsStealth(true);
                targetCharacter.jobQueue.AddJobInQueue(job);
            }
        }
        StartCooldown();
        Messenger.Broadcast(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    }

    #region Cooldown
    private void StartCooldown() {
        currentCooldownTick = 0;
        Messenger.AddListener(Signals.TICK_ENDED, PerTickCooldown);
    }
    private void PerTickCooldown() {
        currentCooldownTick++;
        if (currentCooldownTick == cooldownDuration) {
            //coodlown done
            StopCooldown();
        }
    }
    private void StopCooldown() {
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTickCooldown);
        Messenger.Broadcast(Signals.AREA_INFO_UI_UPDATE_APPROPRIATE_CONTENT, tileLocation.region);
    }
    #endregion

    public override void DestroyLandmark() {
        StopCooldown();
        base.DestroyLandmark();
    }
    public int GetConvertToCultistCost(Character character) {
        float manaCost = 0;
        if (character.traitContainer.GetNormalTrait("Evil") != null) {
            manaCost = 200;
        } else if (character.traitContainer.GetNormalTrait("Disillusioned") != null) {
            manaCost = 300;
        } else if (character.traitContainer.GetNormalTrait("Treacherous") != null) {
            manaCost = 300;
        }

        if (character.currentMoodType == CHARACTER_MOOD.GREAT || character.currentMoodType == CHARACTER_MOOD.GOOD) {
            manaCost *= 1.5f;
        } else if (character.currentMoodType == CHARACTER_MOOD.BAD) {
            manaCost *= 1f;
        } else if (character.currentMoodType == CHARACTER_MOOD.DARK) {
            manaCost *= 0.75f;
        }

        return Mathf.FloorToInt(manaCost);
    }
}


public class SaveDataTheProfane : SaveDataLandmark {
    public int currentCooldownTick;

    public override void Save(BaseLandmark landmark) {
        base.Save(landmark);
        TheProfane profane = landmark as TheProfane;
        currentCooldownTick = profane.currentCooldownTick;
    }
    public override void LoadSpecificLandmarkData(BaseLandmark landmark) {
        base.LoadSpecificLandmarkData(landmark);
        TheProfane profane = landmark as TheProfane;
        profane.LoadSavedData(this);
    }
}
