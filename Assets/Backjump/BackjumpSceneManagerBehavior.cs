using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

namespace Backjump {
	public class BackjumpSceneManagerBehavior : MonoBehaviour {

		// Text input is not supported in this demo, but test annotations for tags can be added here.
		// If the string is empty, tag won't have annotation.
		static string[] TestAnnotations = new string[] {
			"", "", "Exposed live wire", "Sink installed to wrong room"
		};
		int TestAnnotationIndex = 0;
		
		// List view modes (which are toggled with MODE button) here, with the layers in the model that should be shown.
		// For example, line
		// 		STRUCTURAL: Laatat, Pilarit, Ikkunat
		// ...will define mode with name STRUCTURAL that contains 3 layers.
		// 		ARK: *
		// ...will define mode ARK that shows all layers.
		public static string[] ModesAndVisibility =
@"ARK: *
HVACE: IV, Vesi, 3D 2D Viivat
STRUCTURAL: Laatat, Pilarit, Ikkunat"
.Split('\n');
		
		public List<Annotation> Annotations = new List<Annotation>();
		
		public GameObject InfoText;
		public GameObject Scanned;
		public GameObject Model;
		public GameObject ScanMarker1;
		public GameObject ModelMarker1;
		public GameObject ScanMarker2;
		public GameObject ModelMarker2;
		public GameObject Canvas; 
		public GameObject AnnotationContainer;
		public GameObject ModeTitle;
		
		enum CalibrationPhase {
			Unknown,
			Point1Model,
			Point1Scan,
			Point2Model,
			Point2Scan,
			Completed
		}
		
		CalibrationPhase Phase = CalibrationPhase.Unknown;

        public TangoPointCloud m_pointCloud;
        bool usingTango;
        int captureState = 0;

        public void Awake() {
			
			GoToPhase(CalibrationPhase.Point1Model);
			
			ScanMarker1.SetActive(false);
			ModelMarker1.SetActive(false);
			ScanMarker2.SetActive(false);
			ModelMarker2.SetActive(false);
			
			ModeButtonClicked();
            usingTango = Scanned == null;

        }

        float rotation = 0;
		float height = 2f;
		float distance = 7f;
		float horizontalOffset = 0f;
		
		public void PinButtonClicked() {
			if(Phase != CalibrationPhase.Completed) {
				MarkPosition();
			} else {
				Annotate(Screen.width * .5f, Screen.height * .5f);
			}
		}
		
		int CurrentMode = ModesAndVisibility.Length - 1;
		
		// change display mode and show associated layers
		public void ModeButtonClicked() {
			CurrentMode++;
			CurrentMode %= ModesAndVisibility.Length;
			
			var data = ModesAndVisibility[CurrentMode].Split(':');
			
			ModeTitle.GetComponent<Text>().text = String.Format("{0} MODE", data.First());
			
			var layers = data.Last().Split(',').Select(d => d.Trim());
			
			bool showAll = layers.Contains("*"); 
			
			foreach(var t in Model.GetComponentsInChildren<MeshRenderer>()) {
				var show = showAll;
				var p = t.transform.parent;
				while(p != null && !show) {
					if(layers.Contains(p.name)) show = true;
					p = p.transform.parent;
				}
				t.enabled = show;
			}
		}
		

        public void Update() {

            Annotations.ForEach(a => {
                a.CanvasObject.SetActive(false);
            });
			
            var buttonBeingPressed = this.IsButtonPressed();
			
			// if button not touched, model receives the event
            if (!buttonBeingPressed) {
                if (Phase != CalibrationPhase.Completed) {
                    if (Input.GetKeyDown(KeyCode.Space)) MarkPosition();
                    if (Input.GetKeyDown(KeyCode.P)) TestAlign();

                    if (Input.GetMouseButtonDown(0)) MarkMousePosition();
                } else {
                    if (Input.GetKeyDown(KeyCode.Space)) PinButtonClicked();
                    if (Input.GetMouseButtonDown(0)) Annotate(Input.mousePosition.x, Input.mousePosition.y);
                }
            }

            if (!usingTango) ControlCamera();
			
			// project sphere to preview hit location
			Vector3 hit;
			var sphere = GameObject.Find("HitSphere");
			
			if(!buttonBeingPressed && sphere != null) {
				var mp = Input.mousePosition;
				if(usingTango || mp.x < 0 || mp.y < 0 || mp.x > Screen.width || mp.y > Screen.height) {
					if(GetScreenCenterRaycastHit(out hit)) {
						sphere.transform.position = hit;
					}
				} else {
					if(GetScreenRaycastHit(mp.x, mp.y, out hit)) {
						sphere.transform.position = hit;
					}
				}
			}
			
			Annotations.ForEach(ProjectAnnotation);
		}
		
		// project annotations to UI layer
		void ProjectAnnotation(Annotation annotation) {
			annotation.CanvasObject.SetActive(true);
			
			var canvasRect = Canvas.GetComponent<RectTransform>();

			var cam = Camera.main;
			var viewportPos = cam.WorldToViewportPoint(annotation.Position);
			var WorldObject_ScreenPosition=new Vector2(
				((viewportPos.x * canvasRect.sizeDelta.x)-(canvasRect.sizeDelta.x*0.5f)),
				((viewportPos.y * canvasRect.sizeDelta.y)-(canvasRect.sizeDelta.y*0.5f))
			);
			
			if(viewportPos.z < 0) return;
			
			var dist = Vector3.Distance(cam.transform.position, annotation.Position);
			var aRect = annotation.CanvasObject.GetComponent<RectTransform>();
			aRect.anchoredPosition = WorldObject_ScreenPosition;
			aRect.localScale = Vector3.one * Mathf.Max(.2f, 1f - dist * .1f);
		}
		
		void ControlCamera() {
			var cam = Camera.main;

			var rotate = 0f;	
			var dy = 0f;
			
			var forward = 0f;
			var sideways = 0f;

			if (Input.GetKey(KeyCode.UpArrow)) dy += .03f;
			if (Input.GetKey(KeyCode.DownArrow)) dy -= .03f;
			if (Input.GetKey(KeyCode.LeftArrow)) rotate -= 1f;
			if (Input.GetKey(KeyCode.RightArrow)) rotate += 1f;
			if (Input.GetKey(KeyCode.W)) forward += .1f;
			if (Input.GetKey(KeyCode.S)) forward -= .1f;
			if (Input.GetKey(KeyCode.A)) sideways -= .1f;
			if (Input.GetKey(KeyCode.D)) sideways += .1f;
			
			var ct = cam.transform;
			
			var ang = ct.rotation.eulerAngles.y;
			
			var dx = Mathf.Cos(ang) * forward + Mathf.Sin(ang) * sideways;
			var dz = Mathf.Sin(ang) * forward + Mathf.Cos(ang) * sideways;

			ct.Rotate(0, rotate, 0, Space.Self);
			ct.Translate(sideways, dy, forward, Space.Self);
		}
		
		void Annotate(float screenX, float screenY) {
			
			if(this.IsTooFastTap()) return;
			
			// find nearest annotation that already exists
			var ray = Camera.main.ScreenPointToRay(new Vector3(screenX, screenY, 0));
			
			var minDistance = float.MaxValue;
			Annotation nearest = null;
			Annotations.ForEach(a => {
				var distance = Vector3.Cross(ray.direction, a.Position - ray.origin).magnitude;
				if(distance < minDistance && distance < .05f) {
					nearest = a;
					minDistance = distance;
				}
			});
			
			Annotations.Where(a => a.Opened && a != nearest).ToList().ForEach(a => {
				a.ToggleOpen();
				UpdateCanvasAnnotation(a);
			});
			
			// annotation found near raycast, open/close
			if(nearest != null) {
				nearest.ToggleOpen();
				UpdateCanvasAnnotation(nearest);
				return;
			}
			
			RaycastHit hit;
			if(!Physics.Raycast(ray, out hit)) return; // model not hit by raycast
			
			// add new annotation
			TestAnnotationIndex %= TestAnnotations.Length;
			var txt = TestAnnotations[TestAnnotationIndex++];
			
			var annotation = new Annotation {
				Position = hit.point,
				Text = txt
			};
			
			Annotations.Add(annotation);
			UpdateCanvasAnnotation(annotation);
		}
		
		// recreate annotation gameobject
		void UpdateCanvasAnnotation(Annotation annotation) {
			if(annotation.CanvasObject != null) {
				Debug.Log("Destroying old canvas object");
				Destroy(annotation.CanvasObject);
				annotation.CanvasObject = null;
			}
			
			var go = Instantiate(Resources.Load(annotation.GetPrefabName())) as GameObject;
			go.transform.SetParent(AnnotationContainer.transform, false);
			go.name = Guid.NewGuid().ToString().Substring(0, 8);
			
			var t = go.GetComponentInChildren<Text>();
			if(t != null) t.text = annotation.Text;
			
			annotation.CanvasObject = go;
		}
		
		bool GetScreenCenterRaycastHit(out Vector3 hit) {
			return GetScreenRaycastHit(Screen.width * .5f, Screen.height * .5f, out hit);
		}
		
		bool GetScreenRaycastHit(float x, float y, out Vector3 hitPosition) {
			
            bool ok;
            if (usingTango && (Phase == CalibrationPhase.Point1Scan || Phase== CalibrationPhase.Point2Scan))
            {
                var screenPos = new Vector2(x, y);
                Plane plane;
                ok = m_pointCloud.FindPlane(Camera.main, screenPos, out hitPosition, out plane);
            }
            else
            {
                var ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
                RaycastHit hit;
                ok = Physics.Raycast(ray, out hit);
                hitPosition = hit.point;
            }
			
            return ok;
        }


        void GoToPhase(CalibrationPhase newPhase) {
			Phase = newPhase;
			
			var txt = "";
			
			switch(Phase) {
				case CalibrationPhase.Point1Model :
                    if (Scanned != null) Scanned.SetActive(false);
					Model.SetActive(true);
					txt = "Aim and press select to locate position 1 in model";
					break;
				case CalibrationPhase.Point1Scan :
					if (Scanned!=null) Scanned.SetActive(true);
					Model.SetActive(false);
					txt = "Then locate position 1 in real world";
					break;
				case CalibrationPhase.Point2Model :
                    if (Scanned != null) Scanned.SetActive(false);
					Model.SetActive(true);
					txt = "Locate position 2 in model";
					break;
				case CalibrationPhase.Point2Scan :
                    if (Scanned != null) Scanned.SetActive(true);
					Model.SetActive(false);
					txt = "And finally position 2 in real world";
					break;
				case CalibrationPhase.Completed :
					AlignModelToScan();
                    if (Scanned != null) Scanned.SetActive(true);
					Model.SetActive(true);
                    GameObject.Find("HitSphere").SetActive(false);
					break;
			}
			
			var c = Phase == CalibrationPhase.Completed;
			InfoText.GetComponent<Text>().text = txt;
			
			foreach(var s in "Pin,Mode,Configuration".Split(',')) GameObject.Find("Button" + s).GetComponent<Image>().enabled = c;
			foreach(var s in "Mode,Configuration".Split(',')) GameObject.Find("Text" + s).GetComponent<Text>().enabled = c;
		}
		
		void TestAlign() {
			ScanMarker1.transform.position = new Vector3(1.670736f, 2.428516f, -0.8953011f);
			ModelMarker1.transform.position = new Vector3(-6.418804f, 2.039249f, 1.533271f);
			ScanMarker2.transform.position = new Vector3(-4.297872f, 2.763121f, 4.238774f);
			ModelMarker2.transform.position = new Vector3(-8.734095f, 2.324718f, 6.720103f);
			
			ScanMarker1.SetActive(true);
			ModelMarker1.SetActive(true);
			ScanMarker2.SetActive(true);
			ModelMarker2.SetActive(true);
			
			GoToPhase(CalibrationPhase.Completed);
		}
		
		void AlignModelToScan() {
			var s1 = ScanMarker1.transform.position;
			var m1 = ModelMarker1.transform.position;
			var s2 = ScanMarker2.transform.position;
			var m2 = ModelMarker2.transform.position;
			
			var sv = s2 - s1;
			var mv = m2 - m1;
			
			sv = new Vector3(sv.x, 0f, sv.z);
			mv = new Vector3(mv.x, 0f, mv.z);
			
			var scale = sv.magnitude / mv.magnitude;

			Debug.LogFormat("Vector sizes: source {0:f2}, model {1:f2}, scale factor {2:f2}",
			  sv.magnitude, mv.magnitude, scale);
						
			var angle = Vector3.Angle(sv, mv);
			var cross = Vector3.Cross(sv, mv);
			
			Debug.LogFormat("Angle {0}, cross product {1}", angle, cross);
			
			Model.transform.RotateAround(m1, cross, -angle);
			Model.transform.Translate(s1 - m1, Space.World);
			Model.transform.ScaleAround(s1, scale);

            if (Scanned != null) Scanned.SetActive(true);
			Model.SetActive(true);
		}
		
		
		void MarkPosition() {
			Vector3 hit;
			if(!GetScreenCenterRaycastHit(out hit)) return;
			MarkPosition(hit);
		}
		
		void MarkMousePosition() {
            Vector3 hit;
			if(!GetScreenRaycastHit(Input.mousePosition.x, Input.mousePosition.y, out hit)) return;
			MarkPosition(hit);
		}

        void MarkTouchPosition() {
            Vector3 hit;
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                if (!GetScreenRaycastHit(t.position.x, t.position.y, out hit)) return;
                MarkPosition(hit);
            }
        }


        void MarkPosition(Vector3 hitPosition) {
			
			if(this.IsTooFastTap()) return;

			Debug.LogFormat("Hit for phase {0} is {1}", Phase, hitPosition);
			
			Debug.LogFormat("new Vector3({0}f, {1}f, {2}f)", hitPosition.x, hitPosition.y, hitPosition.z);
			
			Action<GameObject, CalibrationPhase> advance = (g, p) => {
				g.SetActive(true);
				g.transform.position = hitPosition;
				GoToPhase(p);
			};
			
			switch(Phase) {
				case CalibrationPhase.Point1Model : advance(ModelMarker1, CalibrationPhase.Point1Scan); break;
				case CalibrationPhase.Point1Scan : advance(ScanMarker1, CalibrationPhase.Point2Model); break;
				case CalibrationPhase.Point2Model : advance(ModelMarker2, CalibrationPhase.Point2Scan); break;
				case CalibrationPhase.Point2Scan : advance(ScanMarker2, CalibrationPhase.Completed); break;
			}
		}
		
	}
}
