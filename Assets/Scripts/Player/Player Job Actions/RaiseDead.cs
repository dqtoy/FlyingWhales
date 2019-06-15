using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaiseDead : PlayerJobAction {

    public RaiseDead() {
        name = "Raise Dead";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        Character target;
        if (targetPOI is Character) {
            target = targetPOI as Character;
        } else {
            return;
        }
        base.ActivateAction(assignedCharacter, target);
        GameManager.Instance.StartCoroutine(Raise(target));
    }

    private IEnumerator Raise(Character target) {
        target.marker.Play("Raise Dead");
        yield return new WaitForSeconds(0.7f);
        target.ReturnToLife();
        UIManager.Instance.Unpause();
        yield return null;
    }

    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        return targetCharacter.isDead;
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return false;
        }
        Character targetCharacter = targetPOI as Character;
        return targetCharacter.isDead;
    }
}
