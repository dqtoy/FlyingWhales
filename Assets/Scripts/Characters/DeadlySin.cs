using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DeadlySin {
    public DEADLY_SIN_ACTION[] assignments { get; protected set; }

    public DeadlySin() { }

    public bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
        return assignments.Contains(sinAction);
    }

    #region Virtuals
    public virtual INTERVENTION_ABILITY_CATEGORY GetInterventionAbilityCategory() { return INTERVENTION_ABILITY_CATEGORY.NONE; }
    #endregion
}

public class Envy : DeadlySin {
    public Envy() : base() {
        assignments = new DEADLY_SIN_ACTION[] { DEADLY_SIN_ACTION.RESEARCH_SPELL, DEADLY_SIN_ACTION.SPAWN_EVENT, DEADLY_SIN_ACTION.CONSTRUCT };
    }
    #region Overrides
    //public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
    //    return sinAction == DEADLY_SIN_ACTION.RESEARCH_SPELL 
    //        || sinAction == DEADLY_SIN_ACTION.SPAWN_EVENT 
    //        || sinAction == DEADLY_SIN_ACTION.CONSTRUCT;
    //}
    public override INTERVENTION_ABILITY_CATEGORY GetInterventionAbilityCategory() {
        return INTERVENTION_ABILITY_CATEGORY.SABOTAGE;
    }
    #endregion
}

public class Greed : DeadlySin {
    public Greed() : base() {
        assignments = new DEADLY_SIN_ACTION[] { DEADLY_SIN_ACTION.INTERFERE, DEADLY_SIN_ACTION.INVADE, DEADLY_SIN_ACTION.FIGHT };
    }
    //#region Overrides
    //public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
    //    return sinAction == DEADLY_SIN_ACTION.INTERFERE
    //        || sinAction == DEADLY_SIN_ACTION.INVADE
    //        || sinAction == DEADLY_SIN_ACTION.FIGHT;
    //}
    //#endregion
}

public class Pride : DeadlySin {
    public Pride() : base() {
        assignments = new DEADLY_SIN_ACTION[] { DEADLY_SIN_ACTION.SPAWN_EVENT, DEADLY_SIN_ACTION.UPGRADE, DEADLY_SIN_ACTION.FIGHT };
    }
    //#region Overrides
    //public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
    //    return sinAction == DEADLY_SIN_ACTION.SPAWN_EVENT
    //        || sinAction == DEADLY_SIN_ACTION.INVADE
    //        || sinAction == DEADLY_SIN_ACTION.FIGHT;
    //}
    //#endregion
}

public class Lust : DeadlySin {
    public Lust() : base() {
        assignments = new DEADLY_SIN_ACTION[] { DEADLY_SIN_ACTION.RESEARCH_SPELL, DEADLY_SIN_ACTION.INTERFERE, DEADLY_SIN_ACTION.UPGRADE };
    }
    #region Overrides
    //public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
    //    return sinAction == DEADLY_SIN_ACTION.RESEARCH_SPELL
    //        || sinAction == DEADLY_SIN_ACTION.INTERFERE
    //        || sinAction == DEADLY_SIN_ACTION.UPGRADE;
    //}
    public override INTERVENTION_ABILITY_CATEGORY GetInterventionAbilityCategory() {
        return INTERVENTION_ABILITY_CATEGORY.HEX;
    }
    #endregion
}

public class Gluttony : DeadlySin {
    public Gluttony() : base() {
        assignments = new DEADLY_SIN_ACTION[] { DEADLY_SIN_ACTION.INVADE, DEADLY_SIN_ACTION.INTERFERE, DEADLY_SIN_ACTION.CONSTRUCT };
    }
    //#region Overrides
    //public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
    //    return sinAction == DEADLY_SIN_ACTION.INVADE
    //        || sinAction == DEADLY_SIN_ACTION.INTERFERE
    //        || sinAction == DEADLY_SIN_ACTION.CONSTRUCT;
    //}
    //#endregion
}

public class Wrath : DeadlySin {
    public Wrath() : base() {
        assignments = new DEADLY_SIN_ACTION[] { DEADLY_SIN_ACTION.RESEARCH_SPELL, DEADLY_SIN_ACTION.INVADE, DEADLY_SIN_ACTION.FIGHT };
    }
    #region Overrides
    //public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
    //    return sinAction == DEADLY_SIN_ACTION.RESEARCH_SPELL
    //        || sinAction == DEADLY_SIN_ACTION.INVADE
    //        || sinAction == DEADLY_SIN_ACTION.FIGHT;
    //}
    public override INTERVENTION_ABILITY_CATEGORY GetInterventionAbilityCategory() {
        return INTERVENTION_ABILITY_CATEGORY.DEVASTATION;
    }
    #endregion
}

public class Sloth : DeadlySin {
    public Sloth() : base() {
        assignments = new DEADLY_SIN_ACTION[] { DEADLY_SIN_ACTION.RESEARCH_SPELL, DEADLY_SIN_ACTION.UPGRADE, DEADLY_SIN_ACTION.CONSTRUCT };
    }
    #region Overrides
    //public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
    //    return sinAction == DEADLY_SIN_ACTION.RESEARCH_SPELL
    //        || sinAction == DEADLY_SIN_ACTION.UPGRADE
    //        || sinAction == DEADLY_SIN_ACTION.CONSTRUCT;
    //}
    public override INTERVENTION_ABILITY_CATEGORY GetInterventionAbilityCategory() {
        return INTERVENTION_ABILITY_CATEGORY.MONSTER;
    }
    #endregion
}
