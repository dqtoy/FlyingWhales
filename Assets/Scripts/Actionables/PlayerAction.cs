using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actionables {
	public class PlayerAction {
		
		public string actionName { get; private set; }
		public System.Func<bool> isActionValidChecker { get; private set; }
		public System.Action action { get; private set; }

		public PlayerAction(string _name, System.Func<bool> _isActionValidChecker, System.Action _action) {
			actionName = _name;
			isActionValidChecker = _isActionValidChecker;
			action = _action;
		}
		
	}	
}
