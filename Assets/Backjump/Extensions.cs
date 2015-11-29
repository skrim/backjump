using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Backjump {
	
	public static class Extensions {
		
		public static void ScaleAround(this Transform transform, Vector3 point, float newScale) {
			transform.localScale = transform.localScale * newScale;
			transform.position = ((transform.position - point) * newScale) + point;
		}
		
		
		static DateTime lastButtonClick = DateTime.MinValue;
		
		public static bool IsTooFastTap(this MonoBehaviour mb) {
			if(DateTime.Now.Subtract(lastButtonClick).TotalMilliseconds < 500) {
				Debug.Log("Ignoring successive marking less than 500 ms from previous");
				return true;
			}
			
			lastButtonClick = DateTime.Now;
			return false;
		}
		
		public static bool IsButtonPressed(this MonoBehaviour mb) {
			// find button that is being clicked/touched to prevent event from leaking to model 
			
			var result = false;
            var es = EventSystem.current;
			
            if (Input.GetMouseButtonDown(0) && es.IsPointerOverGameObject()) {
                var go = es.currentSelectedGameObject;
                result = go != null && go.GetComponent<Button>() != null;

                if (result) Debug.Log("Hitting " + go.name);
            }
			
			if(Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
				var go = es.currentSelectedGameObject;
				result = go != null && go.GetComponent<Button>() != null;
				
				if(result) Debug.Log("Touching " + go.name);
			}
			
			return result;
		}
	}
}
