using System;
using UnityEditor;
using UnityEngine;

namespace DirectAccessQuadtree {
	public class QuadTreeMonoBehaviour : MonoBehaviour {
		private Quadtree _qt;
		private Vector2 _currMouseWorldPos;
		private float _selectionLeft;
		private float _selectionRight;
		private float _selectionTop;
		private float _selectionBottom;
		private float _currentScrollAmount = 10f;
		private int _level;
		private int _index;
		private int _rectLeft;
		private int _rectRight;
		private int _rectTop;
		private int _rectBottom;
		
		private void Awake() => _qt = new Quadtree();

		private int lastWidth;
		private void Update() {
			Vector3 currMousePos = Input.mousePosition;
			if (PanAndZoom(currMousePos) || Input.GetKey(KeyCode.LeftShift)) { } 
			else
				SetSelectionRect(currMousePos);
			
			(_level, _index) = GetSelectionRect();
			// Debug.Log();
			int width = (int)(Quadtree.Width / Mathf.Pow(2, _level - 1));
			int mod = (int)Mathf.Pow(2, Mathf.Max(1, _level - 1));

			int x = _index % mod * width;
			int y = _index / mod * width;
			_rectLeft = x;
			_rectRight = x + width;
			_rectBottom = y;
			_rectTop = y + width;
		}

		private void OnGUI() {
			GUILayout.Label($" {_level}, {_index}\n\n {_qt.QuadLevels[0].Length}\n {_qt.QuadLevels[1].Length}\n {_qt.QuadLevels[2].Length}\n {_qt.QuadLevels[3].Length}");
		}

		[SerializeField] private Camera _cam;
		private Vector3 _prevMousePos;
		private bool PanAndZoom(Vector3 currMousePos) {
			bool panning = false;
			bool rgtMouseClick = Input.GetMouseButtonDown(1);
			bool rgtMouseHold = Input.GetMouseButton(1);

			if (Input.GetKey(KeyCode.LeftShift) || rgtMouseHold) {
				float max = 300f;
				float min = 10f;
				float magnitude = Mathf.InverseLerp(max, min, _cam.orthographicSize);
				float scrollMagnitude = Mathf.Lerp(5f, 1f, magnitude * magnitude * magnitude);
				_cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize + Input.mouseScrollDelta.y * scrollMagnitude, 10f, 300f);
			}

			if (rgtMouseClick) {
				_prevMousePos = currMousePos;
			} else if (rgtMouseHold) {
				panning = true;
				Vector3 diff = GetMouseWorldPos(_prevMousePos) - GetMouseWorldPos(currMousePos);
				_cam.transform.position += diff;
			}
			
			_prevMousePos = currMousePos;
			return panning;
		}

		private void SetSelectionRect(Vector3 currMousePos) {
			_currMouseWorldPos = GetMouseWorldPos(currMousePos);
			float max = 300f;
			float min = 10f;
			float magnitude = Mathf.InverseLerp(max, min, _currentScrollAmount);
			float scrollMagnitude = Mathf.Lerp(10f, 1f, magnitude * magnitude * magnitude);
			_currentScrollAmount = Mathf.Max(0.25f, _currentScrollAmount + -Input.mouseScrollDelta.y * scrollMagnitude);
			_currMouseWorldPos.x = Mathf.Clamp(_currMouseWorldPos.x, _currentScrollAmount, Helpers.Math.BitDecrement(Quadtree.Width - _currentScrollAmount));
			_currMouseWorldPos.y = Mathf.Clamp(_currMouseWorldPos.y, _currentScrollAmount, Helpers.Math.BitDecrement(Quadtree.Width - _currentScrollAmount));
			_selectionLeft = _currMouseWorldPos.x - _currentScrollAmount;
			_selectionRight = _currMouseWorldPos.x + _currentScrollAmount;
			_selectionBottom = _currMouseWorldPos.y - _currentScrollAmount;
			_selectionTop = _currMouseWorldPos.y + _currentScrollAmount;
		}

		private (int, int) GetSelectionRect() => Quadtree.GetLevelIndex(_selectionLeft, _selectionTop, _selectionRight, _selectionBottom);

		private Vector3 GetMouseWorldPos(Vector3 mousePosition) {
			Plane plane = new Plane(Vector3.forward, Vector3.zero);
			Ray ray = Camera.main.ScreenPointToRay(mousePosition);
			if (plane.Raycast(ray, out float enter)) 
				return ray.GetPoint(enter);
			return Vector3.zero;
		}

		[Range(0, 5)] public int depth = 3;
		private void OnDrawGizmos() {
			if (!Application.isPlaying) return;

			float[] cs = {
					0.5f,
					0.3f,
					0.15f,
					0.05f,
					0.01f,
					0.005f,
					0.0025f,
					0.00125f,
					0.0006f,
					0.00003f,
			};
			DrawQuadTree();
			Gizmos.color = Color.white;
			DrawSelectionRect();
			DrawQuadNodes();
			DrawSelectedQuad();
			DrawQuadTreeOccupancy();

			void DrawQuadTree() {
				for (int i = depth; i >= 0; --i) {
					float c = cs[i];
					Gizmos.color = new Color(c, c, c, 1f);
					float width = Quadtree.Width / Mathf.Pow(4, i);
					for (float x = 0; x < Quadtree.Width; x += width) {
						for (float y = 0; y < Quadtree.Width; y += width) {
							Gizmos.DrawLine(
									new Vector3(x, y + width),
									new Vector3(x + width, y + width)
							);
							Gizmos.DrawLine(
									new Vector3(x + width, y + width),
									new Vector3(x + width, y)
							);
							Gizmos.DrawLine(
									new Vector3(x + width, y),
									new Vector3(x, y)
							);
							Gizmos.DrawLine(
									new Vector3(x, y),
									new Vector3(x, y + width)
							);
						}
					}
				}
			}

			void DrawSelectionRect() {
				Debug.DrawLine(
						new Vector3(_selectionLeft, _selectionTop),
						new Vector3(_selectionRight, _selectionTop)
				);
				Debug.DrawLine(
						new Vector3(_selectionRight, _selectionTop),
						new Vector3(_selectionRight, _selectionBottom)
				);
				Debug.DrawLine(
						new Vector3(_selectionRight, _selectionBottom),
						new Vector3(_selectionLeft, _selectionBottom)
				);
				Debug.DrawLine(
						new Vector3(_selectionLeft, _selectionBottom),
						new Vector3(_selectionLeft, _selectionTop)
				);
			}

			void DrawQuadNodes() {
				foreach (Rect quadNode in _qt.Rects) {
					if (quadNode is { left: 0f, top: 0f, right: 0f, bottom: 0f }) continue;
					Gizmos.DrawLine(
							new Vector3(quadNode.left, quadNode.top),
							new Vector3(quadNode.right, quadNode.top)
					);
					Gizmos.DrawLine(
							new Vector3(quadNode.right, quadNode.top),
							new Vector3(quadNode.right, quadNode.bottom)
					);
					Gizmos.DrawLine(
							new Vector3(quadNode.right, quadNode.bottom),
							new Vector3(quadNode.left, quadNode.bottom)
					);
					Gizmos.DrawLine(
							new Vector3(quadNode.left, quadNode.bottom),
							new Vector3(quadNode.left, quadNode.top)
					);
				}
			}

			void DrawSelectedQuad() {
				Debug.DrawLine(
						new Vector3(_rectLeft, _rectTop),
						new Vector3(_rectRight, _rectTop)
				);
				Debug.DrawLine(
						new Vector3(_rectRight, _rectTop),
						new Vector3(_rectRight, _rectBottom)
				);
				Debug.DrawLine(
						new Vector3(_rectRight, _rectBottom),
						new Vector3(_rectLeft, _rectBottom)
				);
				Debug.DrawLine(
						new Vector3(_rectLeft, _rectBottom),
						new Vector3(_rectLeft, _rectTop)
				);
			}

			void DrawQuadTreeOccupancy() {
				int i = 0;
				int occupied = 0;
				int tot = 87_381;
				int sqr = (int)Mathf.Sqrt(tot);
				int origo = -(sqr - Quadtree.Width) / 2;
				
				for (int level = 0; level < _qt.QuadLevels.Length; ++level) {
					var lvl = _qt.QuadLevels[level];
					for (int index = 0; index < lvl.Length; ++index) {
						Gizmos.color = lvl[index].Count > 0 ? Color.white : Color.red;
						Gizmos.DrawCube(new Vector3(origo + i % sqr, -5 - i / sqr), Vector3.one);
						++i;
						if (lvl[index].Count == 0) continue;
						// Debug.Log($"{lvl[index].Count} {lvl[index].FirstChild}");
						++occupied;
					}

				}

				GUIStyle whiteText = new GUIStyle {
						fontSize = 12,
						normal = new GUIStyleState { textColor = Color.white }, // Text color
						// alignment = TextAnchor.MiddleCenter,                 // Align text to center
						// fontStyle = FontStyle.Bold                           // Bold font style
				};
				GUIStyle blackText = new GUIStyle {
						fontSize = 12,
						normal = new GUIStyleState { textColor = Color.black }, // Text color
						// alignment = TextAnchor.MiddleCenter,                 // Align text to center
						// fontStyle = FontStyle.Bold                           // Bold font style
				};
				var textString = $"{(float)occupied / i}% {occupied}/{i}";
				Handles.color = Color.black;
				Handles.Label(new Vector3(origo, 6f, -2f), textString, blackText);
				Handles.color = Color.white;
				Handles.Label(new Vector3(origo + 0.2f, 6f - 0.2f, -2f), textString, whiteText);
			}
		}
	}
}
