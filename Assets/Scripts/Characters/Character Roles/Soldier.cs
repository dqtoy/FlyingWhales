using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : CharacterRole {
    public override int reservedSupply { get { return 30; } }

    public Soldier() : base(CHARACTER_ROLE.SOLDIER, "Normal", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE, INTERACTION_CATEGORY.OFFENSE, INTERACTION_CATEGORY.DEFENSE }) {
    }

    #region Overrides
    public override void AddRoleWorkPlansToCharacterWeights(WeightedDictionary<INTERACTION_TYPE> weights) {
        base.AddRoleWorkPlansToCharacterWeights(weights);
        weights.AddElement(INTERACTION_TYPE.PATROL, 5);
    }
    public override GoapPlan PickRoleWorkPlanFromCharacterWeights(INTERACTION_TYPE pickedActionType, Character actor) {
        if(pickedActionType == INTERACTION_TYPE.PATROL) {
            return PatrolPlan(actor);
        }
        return base.PickRoleWorkPlanFromCharacterWeights(pickedActionType, actor);
    }
    #endregion

    private GoapPlan PatrolPlan(Character actor) {
        GoapAction goapAction1 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PATROL, actor, actor);
        Stroll goapAction2 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.STROLL, actor, actor) as Stroll;
        goapAction2.SetTargetStructure(actor.currentStructure);
        goapAction2.SetIsStrollFromPatrol(true);
        GoapAction goapAction3 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PATROL, actor, actor);
        Stroll goapAction4 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.STROLL, actor, actor) as Stroll;
        goapAction4.SetTargetStructure(actor.currentStructure);
        goapAction4.SetIsStrollFromPatrol(true);

        GoapNode goalNode = new GoapNode(null, goapAction4.cost, goapAction4);
        GoapNode thirdNode = new GoapNode(goalNode, goapAction3.cost, goapAction3);
        GoapNode secondNode = new GoapNode(thirdNode, goapAction2.cost, goapAction2);
        GoapNode startingNode = new GoapNode(secondNode, goapAction1.cost, goapAction1);

        GoapPlan goapPlan = new GoapPlan(startingNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.WORK);
        return goapPlan;
    }
}
