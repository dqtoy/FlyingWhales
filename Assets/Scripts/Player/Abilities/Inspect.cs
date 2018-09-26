using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Inspect : PlayerAbility {

	public Inspect(): base(ABILITY_TYPE.ALL) {
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

        if(interactable is BaseLandmark) {
            if (PlayerManager.Instance.totalLifestonesInWorld > 0) {
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < PlayerManager.Instance.player.currentLifestoneChance) {
                    PlayerManager.Instance.AdjustTotalLifestones(-1);
                    PlayerManager.Instance.player.AdjustLifestone(1);
                    PlayerManager.Instance.player.DecreaseLifestoneChance();
                }
            }
        }
        for (int i = 0; i < PlayerManager.Instance.player.allAbilities.Count; i++) {
            PlayerManager.Instance.player.allAbilities[i].playerAbilityButton.UpdateThis(interactable);
        }
        base.Activate(interactable);
    }
    public override bool CanBeDone(IInteractable interactable) {
        if (base.CanBeDone(interactable)) {
            if (!interactable.isBeingInspected) {
                return true;
            }
        }
        return false;
    }
    public override bool CanBeActivated(IInteractable interactable) {
        int magicUsed = 0;
        if (interactable is Character) {
            magicUsed = PlayerManager.Instance.player.blueMagic;
        } else if (interactable is BaseLandmark) {
            magicUsed = PlayerManager.Instance.player.greenMagic;
        } else if (interactable is Monster) {
            magicUsed = PlayerManager.Instance.player.redMagic;
        }
        if (magicUsed >= _powerCost) {
            return true;
        }
        return false;
    }
    #endregion

    #region Utilities
    private void ScheduleEndInspection(IInteractable interactable) {
        GameDate date = GameManager.Instance.Today();
        date.AddDays(1);
        SchedulingManager.Instance.AddEntry(date, () => EndInspection(interactable));
    }
    private void EndInspection(IInteractable interactable) {
        interactable.SetIsBeingInspected(false);
        _playerAbilityButton.UpdateThis(interactable);
        interactable.EndedInspection();
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
