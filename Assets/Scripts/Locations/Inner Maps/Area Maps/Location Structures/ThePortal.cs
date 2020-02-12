using System.Collections.Generic;
using System.Linq;
using Actionables;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class ThePortal : LocationStructure {
        public override Vector2 selectableSize { get; }
        public ThePortal(ILocation location) : base(STRUCTURE_TYPE.THE_PORTAL, location){
            selectableSize = new Vector2(10f, 10f);
        }
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            PlayerAction summonMinion = new PlayerAction("Summon Minion", 
                () => PlayerManager.Instance.player.mana >= EditableValuesManager.Instance.summonMinionManaCost, 
                SummonMinion);
            AddPlayerAction(summonMinion);
        }
        private void SummonMinion() {
            List<Character> validMinions = new List<Character>(PlayerManager.Instance.player.minions
                .Where(x => x.character.currentHP >= x.character.maxHP && x.character.gridTileLocation == null)
                .Select(x => x.character));
            UIManager.Instance.ShowClickableObjectPicker(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(),
                OnSelectMinion, null, CanSummonMinion, 
                "Choose Minion to Summon", showCover: true);
        }
        private bool CanSummonMinion(Character character) {
            return character.currentHP >= character.maxHP && character.gridTileLocation == null;
        }
        private void OnSelectMinion(object obj) {
            Character character = obj as Character;
            character.minion.Summon(this);
            UIManager.Instance.HideObjectPicker();
        }
    }
}