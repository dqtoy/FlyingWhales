using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    [System.Serializable]
    public class CharacterClass : EntityComponent {
        public string className;
        public List<string> attackSkills;
        public List<string> healSkills;
        public List<string> fleeSkills;
        public List<string> moveSkills;
        public List<string> obtainSkills;
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
            attackSkills = new List<string>();
            healSkills = new List<string>();
            fleeSkills = new List<string>();
            moveSkills = new List<string>();
            obtainSkills = new List<string>();
        }

        public void AddSkillOfType(SKILL_TYPE skillType, Skill skillToAdd) {
            switch (skillType) {
                case SKILL_TYPE.ATTACK:
                    attackSkills.Add(skillToAdd.skillName);
                    break;
                case SKILL_TYPE.HEAL:
                    healSkills.Add(skillToAdd.skillName);
                    break;
                case SKILL_TYPE.OBTAIN_ITEM:
                    obtainSkills.Add(skillToAdd.skillName);
                    break;
                case SKILL_TYPE.FLEE:
                    fleeSkills.Add(skillToAdd.skillName);
                    break;
                case SKILL_TYPE.MOVE:
                    moveSkills.Add(skillToAdd.skillName);
                    break;
            }
        }

        public void ConstructAllSkillsList() {
            _skills = new List<Skill>();
            for (int i = 0; i < attackSkills.Count; i++) {
                string skillName = attackSkills[i];
                string path = "Assets/CombatPrototype/Data/Skills/ATTACK/" + skillName + ".json";
                AttackSkill currSkill = JsonUtility.FromJson<AttackSkill>(System.IO.File.ReadAllText(path));
                _skills.Add(currSkill);
            }
            for (int i = 0; i < healSkills.Count; i++) {
                string skillName = healSkills[i];
                string path = "Assets/CombatPrototype/Data/Skills/HEAL/" + skillName + ".json";
                HealSkill currSkill = JsonUtility.FromJson<HealSkill>(System.IO.File.ReadAllText(path));
                _skills.Add(currSkill);
            }
            for (int i = 0; i < obtainSkills.Count; i++) {
                string skillName = obtainSkills[i];
                string path = "Assets/CombatPrototype/Data/Skills/OBTAIN_ITEM/" + skillName + ".json";
                ObtainSkill currSkill = JsonUtility.FromJson<ObtainSkill>(System.IO.File.ReadAllText(path));
                _skills.Add(currSkill);
            }
            for (int i = 0; i < fleeSkills.Count; i++) {
                string skillName = fleeSkills[i];
                string path = "Assets/CombatPrototype/Data/Skills/FLEE/" + skillName + ".json";
                FleeSkill currSkill = JsonUtility.FromJson<FleeSkill>(System.IO.File.ReadAllText(path));
                _skills.Add(currSkill);
            }
            for (int i = 0; i < moveSkills.Count; i++) {
                string skillName = moveSkills[i];
                string path = "Assets/CombatPrototype/Data/Skills/MOVE/" + skillName + ".json";
                MoveSkill currSkill = JsonUtility.FromJson<MoveSkill>(System.IO.File.ReadAllText(path));
                _skills.Add(currSkill);
            }
        }
    }
}

