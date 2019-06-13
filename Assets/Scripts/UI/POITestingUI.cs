using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class POITestingUI : MonoBehaviour {
    //This script is used to test characters and actions
    //Most of the functions here will only work if there is a currently clicked/active character
    public RectTransform rt;
    public IPointOfInterest poi { get; private set; }

    #region Utilities
    public void ShowUI(IPointOfInterest poi) {
        if (UIManager.Instance.characterInfoUI.activeCharacter != null) {
            this.poi = poi;
            UIManager.Instance.PositionTooltip(gameObject, rt, rt);
            gameObject.SetActive(true);
        }
    }
    public void HideUI() {
        gameObject.SetActive(false);
        this.poi = null;
    }
    #endregion

    #region Character Testing
    public void KnockoutThisCharacter() {
        if(poi is Character) {
            UIManager.Instance.characterInfoUI.activeCharacter.CreateKnockoutJob(poi as Character);
        } else {
            Debug.LogError(poi.name + " is not a character!");
        }
        HideUI();
    }
    public void ChatWithThisCharacter() {
        if (poi is Character) {
            UIManager.Instance.characterInfoUI.activeCharacter.marker.visionCollision.ForceChatHandling(poi as Character);
        } else {
            Debug.LogError(poi.name + " is not a character!");
        }
        HideUI();
    }
    public void InviteToMakeLove() {
        if (poi is Character) {
            Character target = poi as Character;
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY_FORLORN, INTERACTION_TYPE.INVITE_TO_MAKE_LOVE, target);
            job.SetCannotOverrideJob(true);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job, false);
            //if (UIManager.Instance.characterInfoUI.activeCharacter.HasRelationshipOfTypeWith(target, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR)) {
            //    GoapPlanJob job = new GoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY_FORLORN, INTERACTION_TYPE.INVITE_TO_MAKE_LOVE, target);
            //    job.SetCannotOverrideJob(true);
            //    UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job, false);
            //} else {
            //    Debug.LogError("Must be paramour or lover!");
            //}
        } else {
            Debug.LogError(poi.name + " is not a character!");
        }
        HideUI();
    }
    public void StealFromThisCharacter() {
        if (poi is Character) {
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY_FORLORN, INTERACTION_TYPE.STEAL_CHARACTER, poi);
            job.SetCannotOverrideJob(true);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job, false);
        } else {
            Debug.LogError(poi.name + " is not a character!");
        }
        HideUI();
    }
    #endregion

    #region Tile Object Testing
    public void PoisonTable() {
        if (poi is Table) {
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.UNDERMINE_ENEMY, INTERACTION_TYPE.TABLE_POISON, poi);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job, false);
        } else {
            Debug.LogError(poi.name + " is not a table!");
        }
        HideUI();
    }
    public void EatAtTable() {
        if (poi is Table) {
            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.FULLNESS_RECOVERY_STARVING, INTERACTION_TYPE.EAT_DWELLING_TABLE, poi);
            job.SetCannotOverrideJob(true);
            UIManager.Instance.characterInfoUI.activeCharacter.jobQueue.AddJobInQueue(job, false);
        } else {
            Debug.LogError(poi.name + " is not a table!");
        }
        HideUI();
    }
    #endregion
}
