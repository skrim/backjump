using UnityEngine;

namespace Backjump {
	public class Annotation {
		
		public Vector3 Position { get; set; }
		
		public string Text { get; set; }
		
		public GameObject CanvasObject { get; set; }
		
		public bool Opened { get; private set; }
		
		public string GetPrefabName() {
			if(string.IsNullOrEmpty(Text)) return "MarkerEmpty";
			return Opened ? "MarkerMaximized" : "MarkerMinimized";
		}
		
		public bool ToggleOpen() {
			if(string.IsNullOrEmpty(Text)) return false;
			Opened = !Opened;
			return Opened;
		}
		
	}
}
