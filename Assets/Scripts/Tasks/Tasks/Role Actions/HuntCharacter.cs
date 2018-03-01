using UnityEngine;
using System.Collections;

public class HuntCharacter : CharacterTask {

    private ECS.Character target;

    public HuntCharacter(TaskCreator createdBy, int defaultDaysLeft = -1) : base(createdBy, TASK_TYPE.HUNT_CHARACTER, defaultDaysLeft) {

    }

    #region overrides

    #endregion
}
