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
            targetCharacter.AddTrait("Cultist");
        } else if (action == "Corrupt") {
            
        } else if (action == "Sabotage Faction Quest") {

        } else if (action == "Destroy Supply") {

        } else if (action == "Destroy Food") {

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
        base.DestroyLandmark();
    }
    public int GetConvertToCultistCost(Character character) {
        float manaCost = 0;
        if (character.GetNormalTrait("Evil") != null) {
            manaCost = 200;
        } else if (character.GetNormalTrait("Disillusioned") != null) {
            manaCost = 300;
        } else if (character.GetNormalTrait("Treacherous") != null) {
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
