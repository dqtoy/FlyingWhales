using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : CharacterRole {
    public override int reservedSupply { get { return 30; } }

    public Soldier() : base(CHARACTER_ROLE.SOLDIER, "Normal", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE, INTERACTION_CATEGORY.OFFENSE, INTERACTION_CATEGORY.DEFENSE }) {
        allowedInteractions = new INTERACTION_TYPE[] {
            INTERACTION_TYPE.PATROL,
            INTERACTION_TYPE.PATROL_ROAM,
            INTERACTION_TYPE.GET_SUPPLY,
        };
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
        GoapAction goapAction2 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PATROL_ROAM, actor, actor);
        //GoapAction goapAction3 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PATROL, actor, actor);
        //GoapAction goapAction4 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.PATROL_ROAM, actor, actor);

        GoapNode goalNode = new GoapNode(null, goapAction2.cost, goapAction2);
        //GoapNode thirdNode = new GoapNode(goalNode, goapAction3.cost, goapAction3);
        //GoapNode secondNode = new GoapNode(thirdNode, goapAction2.cost, goapAction2);
        GoapNode startingNode = new GoapNode(goalNode, goapAction1.cost, goapAction1);

        GoapPlan goapPlan = new GoapPlan(startingNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.WORK);
        goapPlan.ConstructAllNodes();
        return goapPlan;
    }
}
