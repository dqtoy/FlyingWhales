using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    [System.Serializable]
    public class CharacterClass : EntityComponent {
        public string className;
        public List<AttackSkill> attackSkills;
        public List<HealSkill> healSkills;
        public List<FleeSkill> fleeSkills;
        public List<MoveSkill> moveSkills;
        public List<ObtainSkill> obtainSkills;
        public int actRate;
        public int strGain;
        public int intGain;
        public int agiGain;
        public int hpGain;
        public int dodgeRate;
        public int parryRate;
        public int blockRate;

        private List<Skill> _skills;

        #region getters/setters
        public List<Skill> skills {
            get { return _skills; }
        }
        #endregion

        public CharacterClass() {
            attackSkills = new List<AttackSkill>();
            healSkills = new List<HealSkill>();
            fleeSkills = new List<FleeSkill>();
            moveSkills = new List<MoveSkill>();
            obtainSkills = new List<ObtainSkill>();
        }

        public void AddSkillOfType(SKILL_TYPE skillType, Skill skillToAdd) {
            switch (skillType) {
                case SKILL_TYPE.ATTACK:
                    attackSkills.Add((AttackSkill)skillToAdd);
                    break;
                case SKILL_TYPE.HEAL:
                    healSkills.Add((HealSkill)skillToAdd);
                    break;
                case SKILL_TYPE.OBTAIN_ITEM:
                    obtainSkills.Add((ObtainSkill)skillToAdd);
                    break;
                case SKILL_TYPE.FLEE:
                    fleeSkills.Add((FleeSkill)skillToAdd);
                    break;
                case SKILL_TYPE.MOVE:
                    moveSkills.Add((MoveSkill)skillToAdd);
                    break;
            }
        }

        public void ConstructAllSkillsList() {
            _skills = new List<Skill>();
            for (int i = 0; i < attackSkills.Count; i++) {
                _skills.Add(attackSkills[i]);
            }
            for (int i = 0; i < healSkills.Count; i++) {
                _skills.Add(healSkills[i]);
            }
            for (int i = 0; i < obtainSkills.Count; i++) {
                _skills.Add(obtainSkills[i]);
            }
            for (int i = 0; i < fleeSkills.Count; i++) {
                _skills.Add(fleeSkills[i]);
            }
            for (int i = 0; i < moveSkills.Count; i++) {
                _skills.Add(moveSkills[i]);
            }
        }
    }
}

