using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Inspect : PlayerAbility {

	public Inspect(): base() {
        _name = "Inspect";
        _description = "Inspect a structure, character, or monster";
        _powerCost = 5;
        _threatGain = 2;
        _cooldown = 12;
    }

    #region Overrides
    public override void Activate(IInteractable interactable) {
        interactable.SetIsBeingInspected(true);
        if (!interactable.hasBeenInspected) {
            interactable.SetHasBeenInspected(true);
        }
        UpdateInfoUI(interactable);
        ScheduleEndInspection(interactable);

        if(PlayerManager.Instance.totalLifestonesInWorld > 0) {
            int chance = UnityEngine.Random.Range(0, 100);
            if(chance < PlayerManager.Instance.player.currentLifestoneChance) {
                PlayerManager.Instance.AdjustTotalLifestones(-1);
                PlayerManager.Instance.player.AdjustLifestone(1);
                PlayerManager.Instance.player.DecreaseLifestoneChance();
            }
        }
        base.Activate(interactable);
    }
    #endregion

    #region Utilities
    private void ScheduleEndInspection(IInteractable interactable) {
        GameDate date = GameManager.Instance.Today();
        date.AddDays(2);
        SchedulingManager.Instance.AddEntry(date, () => EndInspection(interactable));
    }
    private void EndInspection(IInteractable interactable) {
        interactable.SetIsBeingInspected(false);
    }
    private void UpdateInfoUI(IInteractable interactable) {
        if (interactable is Character) {
            UIManager.Instance.UpdateCharacterInfo();
        }else if (interactable is BaseLandmark) {
            UIManager.Instance.UpdateLandmarkInfo();
        }
    }
    #endregion
}
