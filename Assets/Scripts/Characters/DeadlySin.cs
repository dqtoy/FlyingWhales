using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadlySin {
    //public DEADLY_SIN_ACTION[] assignments { get; protected set; }

    #region Virtuals
    public virtual INTERVENTION_ABILITY_CATEGORY GetInterventionAbilityCategory() { return INTERVENTION_ABILITY_CATEGORY.NONE; }
    public virtual bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) { return false; }
    #endregion
}

public class Envy : DeadlySin {
    #region Overrides
    public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
        return sinAction == DEADLY_SIN_ACTION.RESEARCH_SPELL 
            || sinAction == DEADLY_SIN_ACTION.SPAWN_EVENT 
            || sinAction == DEADLY_SIN_ACTION.CONSTRUCT;
    }
    public override INTERVENTION_ABILITY_CATEGORY GetInterventionAbilityCategory() {
        return INTERVENTION_ABILITY_CATEGORY.SABOTAGE;
    }
    #endregion
}

public class Greed : DeadlySin {
    #region Overrides
    public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
        return sinAction == DEADLY_SIN_ACTION.INTERFERE
            || sinAction == DEADLY_SIN_ACTION.INVADE
            || sinAction == DEADLY_SIN_ACTION.FIGHT;
    }
    #endregion
}

public class Pride : DeadlySin {
    #region Overrides
    public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
        return sinAction == DEADLY_SIN_ACTION.SPAWN_EVENT
            || sinAction == DEADLY_SIN_ACTION.INVADE
            || sinAction == DEADLY_SIN_ACTION.FIGHT;
    }
    #endregion
}

public class Lust : DeadlySin {
    #region Overrides
    public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
        return sinAction == DEADLY_SIN_ACTION.RESEARCH_SPELL
            || sinAction == DEADLY_SIN_ACTION.INTERFERE
            || sinAction == DEADLY_SIN_ACTION.UPGRADE;
    }
    public override INTERVENTION_ABILITY_CATEGORY GetInterventionAbilityCategory() {
        return INTERVENTION_ABILITY_CATEGORY.HEX;
    }
    #endregion
}

public class Gluttony : DeadlySin {
    #region Overrides
    public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
        return sinAction == DEADLY_SIN_ACTION.INVADE
            || sinAction == DEADLY_SIN_ACTION.INTERFERE
            || sinAction == DEADLY_SIN_ACTION.CONSTRUCT;
    }
    #endregion
}

public class Wrath : DeadlySin {
    #region Overrides
    public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
        return sinAction == DEADLY_SIN_ACTION.RESEARCH_SPELL
            || sinAction == DEADLY_SIN_ACTION.INVADE
            || sinAction == DEADLY_SIN_ACTION.FIGHT;
    }
    public override INTERVENTION_ABILITY_CATEGORY GetInterventionAbilityCategory() {
        return INTERVENTION_ABILITY_CATEGORY.DEVASTATION;
    }
    #endregion
}

public class Sloth : DeadlySin {
    #region Overrides
    public override bool CanDoDeadlySinAction(DEADLY_SIN_ACTION sinAction) {
        return sinAction == DEADLY_SIN_ACTION.RESEARCH_SPELL
            || sinAction == DEADLY_SIN_ACTION.UPGRADE
            || sinAction == DEADLY_SIN_ACTION.CONSTRUCT;
    }
    public override INTERVENTION_ABILITY_CATEGORY GetInterventionAbilityCategory() {
        return INTERVENTION_ABILITY_CATEGORY.MONSTER;
    }
    #endregion
}
