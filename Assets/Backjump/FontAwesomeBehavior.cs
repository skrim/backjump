using System;
using UnityEngine;
using UnityEngine.UI;

namespace Backjump {
	public class FontAwesomeBehavior : MonoBehaviour {
		
		[Tooltip("Enter as hex string such as f0e7 for bolt - see codes here: http://fortawesome.github.io/Font-Awesome/cheatsheet/")]
		public string Symbol;
		
		public void Awake() {
			var text = GetComponent<Text>();
			if (text != null) {
				text.text = string.Format("{0}", Char.ConvertFromUtf32(Convert.ToInt32(Symbol, 16)));
			}
		}
		
		public void Update() {
		}
		
	}
}