using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace SphereTree {
	
	public class SphereTreeBehaviour : MonoBehaviour {
		public struct SphereActor {
			public int ID { get; set; }
			public Sphere Sphere { get; set; }
			public int EntryID { get; set; }
			public int Attractor { get; set; }
			public float Speed { get; set; }
			public Color Color { get; set; }
		}
		
		[Header("Actor")]
		[SerializeField] private float _actorSpeed = 100.0f;
		[Space, Header("Sphere Tree")]
		[SerializeField] private float _maxWidth = 500.0f;
		[SerializeField] private float _maxSuperSphereRadius = 100f;
		[SerializeField] private float _gravy = 10f;
		[SerializeField] private int _maxSpheres = 4000;
		[SerializeField] private int _numAttractors = 19;

		private SphereTree sphereTree;
		private List<SphereActor> actors = new List<SphereActor>();
		private List<Vector3> attractors = new List<Vector3>();
		private Text selectedText;
		private float[] randomMoveTimes;
		private float mouseRadius = 50;
		
		private void Start() {
			// Initialize attractors
			randomMoveTimes = new float[_numAttractors];
			for (int i = 0; i < _numAttractors; i++)
				attractors.Add(Random.insideUnitCircle * _maxWidth);

			// Initialize SphereTree
			Sphere rootSphere = new Sphere(Vector3.zero, _maxWidth);
			sphereTree = SphereTree.Create(rootSphere, _maxSuperSphereRadius, _gravy);
		}

		// Debug GUI variables
		private int _numberToDisplay;
		private readonly List<string> _textStrings = new List<string>() {
				"Num [1]: Toggle render spheres",
				"Num [2]: Toggle render super spheres",
				"Num [3]: Toggle render attractors",
		};
		private void OnGUI() {
			// Define styles
			GUIStyle numberStyle = new GUIStyle(GUI.skin.label) {
					alignment = TextAnchor.UpperRight,
					fontSize = 24,
					normal = {
							textColor = Color.yellow,
					},
			};

			GUIStyle listStyle = new GUIStyle(GUI.skin.textArea) {
					alignment = TextAnchor.UpperLeft,
					fontSize = 18,
					normal = {
							textColor = Color.blue,
					},
			};

			// Display the number in the top-right corner
			Rect numberRect = new Rect(Screen.width - 200, 10, 190, 50); // Adjust for positioning and padding
			GUI.Label(numberRect, $"Selected: {_numberToDisplay}", numberStyle);

			// Display the list of strings in the top-left corner
			Rect listRect = new Rect(10, 10, 320, 75); // Adjust width and height as needed
			string concatenatedStrings = string.Join("\n", _textStrings); // Combine strings into a single string with line breaks
			GUI.Label(listRect, concatenatedStrings, listStyle);
		}
		
		private void Update() {
			// Input
			if (Input.GetKeyDown(KeyCode.Alpha1)) _drawActors = !_drawActors;
			if (Input.GetKeyDown(KeyCode.Alpha2)) _drawSuperSpheres = !_drawSuperSpheres;
			if (Input.GetKeyDown(KeyCode.Alpha3)) _drawAttractors = !_drawAttractors;
			mouseRadius += Input.mouseScrollDelta.y;

			// Move Attractors randomly
			float currTime = Time.time;
			for (int i = 0; i < randomMoveTimes.Length; ++i) {
				if (!(randomMoveTimes[i] <= currTime)) continue;
				randomMoveTimes[i] = currTime + Random.Range(2f, 5f);
				attractors[i] = Random.insideUnitCircle;
			}
			
			// Update actors
			if (actors.Count < _maxSpheres) {
				for (int i = 0; i < 100; i++) {
					var sphere = new Sphere(Random.insideUnitCircle * _maxWidth, 1.0f);
					int entryID = sphereTree.Insert(actors.Count, sphere);
					int attractor = i % attractors.Count;

					// if (actors.Count > 2000) {
					Color color = (i % 3) switch {
							0 => Color.blue,
							1 => Color.yellow,
							2 => Color.red,
							_ => Color.cyan,
					};
					// }

					actors.Add(new SphereActor {
							ID = actors.Count,
							Sphere = sphere,
							EntryID = entryID,
							Attractor = attractor,
							Speed = _actorSpeed,
							Color = color,
					});
				}
			} else {
				// Move spheres and keep them inside max width
				for (int i = 0; i < actors.Count; i++) {

					// if (actors[i].Attractor == Vector3.zero) continue;
					SphereActor actor = actors[i];
					Sphere sphere = actor.Sphere;
					Vector3 attraction = attractors[actor.Attractor];
					if (sphere.Center.magnitude > _maxWidth) 
						sphere.Center = Random.insideUnitCircle.normalized * _maxWidth;

					sphere.Center += Time.deltaTime * actor.Speed * attraction;
					sphereTree.Move(actors[i].EntryID, actors[i].Sphere);
					actor.Sphere = sphere;
					actors[i] = actor;
				}
			}

			// Integrate and recompute SphereTree
			sphereTree.Integrate();
			sphereTree.Recompute();
		}

		private void OnDrawGizmos() {
			if (!Application.isPlaying) return;
			RenderActorsAndAttractors();
		}

		private bool _drawActors = true;
		private bool _drawAttractors = true;
		private bool _drawSuperSpheres = true;
		private void RenderActorsAndAttractors() {
			/* Render Actor spheres */
			if (_drawActors)
				foreach (var actor in actors) 
					DrawCircle(actor.Sphere.Center, actor.Sphere.Radius, actor.Color);

			/* Render Attractors */
			if (true)
				foreach (var attractor in attractors) 
					DrawCircle(attractor, 4, Color.green);

			/* Render SphereTree super spheres */
			if (_drawSuperSpheres) {
				sphereTree.Walk((sphere, isSuper) => {
					if (!isSuper) return;
					DrawCircle(sphere.Center, sphere.Radius, Color.blue);
				});
			}

			/* Render mouse-interacted entries */
			Vector3 mousePosition = Input.mousePosition;
			var mouseSphere = new Sphere(mousePosition, mouseRadius);
			var entries = sphereTree.GetEntries(mouseSphere);

			foreach (var entry in entries) { DrawCircle(entry.Sphere.Center, entry.Sphere.Radius, Color.yellow); }

			DrawCircle(mousePosition, mouseRadius, Color.yellow);

			_numberToDisplay = entries.Count;
		}

		private void DrawCircle(Vector3 center, float radius, Color color) { // TODO experiment with actual prefab spheres to act as "Debug" instead of Gizmos/Handles 
			Handles.color = Gizmos.color = color;
			Gizmos3D.Sphere(center, radius); // More expensive but prettier - perspective accurate 3D sphere outline using Handles
			// Gizmos.DrawWireSphere(center, radius);
		}
	}

	public struct Sphere {
		public Vector3 Center { get; set; }
		public float Radius { get; set; }

		public Sphere(Vector3 center, float radius) {
			Center = center;
			Radius = radius;
		}

		public bool IntersectsSphere(Sphere other) =>
				Vector3.Distance(this.Center, other.Center) < this.Radius + other.Radius;

		public bool ContainsSphere(Sphere other) =>
				this.Radius >= Vector3.Distance(this.Center, other.Center) + other.Radius;
	}

	public struct SphereTree {
		public float MaxSize { get; private set; }
		public float Gravy { get; private set; }
		public FreeList<SphereEntry> Spheres;

		private Queue<int> _integrateFifo;
		private Queue<int> _recomputeFifo;

		public static SphereTree Create(Sphere rootSphere, float maxSize, float gravy) {
			var tree = new SphereTree {
					Spheres = FreeList<SphereEntry>.Create(),
					MaxSize = maxSize,
					Gravy = gravy,
					_integrateFifo = new Queue<int>(),
					_recomputeFifo = new Queue<int>(),
			};
			tree.Spheres.Insert(new SphereEntry { Sphere = rootSphere, FirstChild = -1, Next = -1 }); // TBD no ID ??? 
			return tree;
		}

		public void Move(int entryID, Sphere sphere) {
			SphereEntry entry = Spheres.Get(entryID);
			entry.Sphere = sphere;
			Spheres.Set(entryID, entry);

			int parentID = entry.Parent;
			SphereEntry parent = Spheres.Get(parentID);

			if (parent.Sphere.ContainsSphere(entry.Sphere)) return;

			RemoveChild(parentID, entryID);
			QueueIntegrate(entryID);
		}


		public void Walk(Action<Sphere, bool> f) {
			SphereEntry root = Spheres.Get(0);

			int childIndex = root.FirstChild;
			while (childIndex != -1) {
				SphereEntry child = Spheres.Get(childIndex);
				int entryIndex = child.FirstChild;
				while (entryIndex != -1) {
					SphereEntry entry = Spheres.Get(entryIndex);
					f(entry.Sphere, false);
					entryIndex = entry.Next;
				}

				childIndex = child.Next;
			}

			childIndex = root.FirstChild;
			while (childIndex != -1) {
				SphereEntry child = Spheres.Get(childIndex);
				f(child.Sphere, true);
				childIndex = child.Next;
			}
		}

		public int Insert(int id, Sphere sphere) {
			int index = Spheres.Insert(new SphereEntry { ID = id, Sphere = sphere, FirstChild = -2, Next = -1 });
			QueueIntegrate(index);
			return index;
		}

		public void Remove(int entryID) {
			SphereEntry entry = Spheres.Get(entryID);
			Spheres.Erase(entryID);
			RemoveChild(entry.Parent, entryID);
		}

		private void AddChild(int parentID, int sphereID) {
			SphereEntry parent = Spheres.Get(parentID);
			SphereEntry entry = Spheres.Get(sphereID);

			entry.Parent = parentID;
			entry.Next = parent.FirstChild;
			parent.FirstChild = sphereID;

			Spheres.Set(parentID, parent);
			Spheres.Set(sphereID, entry);

			QueueRecompute(parentID);
		}

		private void RemoveChild(int parentID, int entryID) {
			SphereEntry parent = Spheres.Get(parentID);
			SphereEntry entry = Spheres.Get(entryID);
			int childIndex = parent.FirstChild;

			if (childIndex == entryID) { parent.FirstChild = entry.Next; } else {
				while (childIndex != -1) {
					SphereEntry child = Spheres.Get(childIndex);
					if (child.Next == entryID) {
						child.Next = entry.Next;
						Spheres.Set(childIndex, child);
						break;
					}

					childIndex = child.Next;
				}
			}

			Spheres.Set(parentID, parent);
			Spheres.Set(entryID, entry);

			QueueRecompute(parentID);
		}

		private void QueueIntegrate(int entryID) {
			foreach (int id in _integrateFifo) // TODO set flags to avoid looping through whole queue
				if (id == entryID)
					return;

			_integrateFifo.Enqueue(entryID);
		}

		private void QueueRecompute(int parentID) {
			if (parentID == 0) // TODO set flags to avoid looping through whole queue
				return;

			foreach (int id in _recomputeFifo)
				if (id == parentID)
					return;

			_recomputeFifo.Enqueue(parentID);
		}

		public void Integrate() {
			while (_integrateFifo.Count > 0)
				if (_integrateFifo.TryDequeue(out int integrateCandidateID))
					Integrate(integrateCandidateID);
		}

		private void Integrate(int entryID) {
			var integrateCandidate = Spheres.Get(entryID);

			int containsUs = -1;
			int nearest = -1;
			float nearestDist = float.MaxValue;

			// look through all super spheres for candidate parent
			// look for super spheres that fully contain the candidate or find the closet one
			SphereEntry rootSphere = Spheres.Get(0);
			int superSphereIndex = rootSphere.FirstChild;
			while (superSphereIndex >= 0) {
				SphereEntry superSphere = Spheres.Get(superSphereIndex);

				if (superSphere.Sphere.ContainsSphere(integrateCandidate.Sphere)) {
					containsUs = superSphereIndex;
					break;
				}

				float dist = Vector3.Distance(superSphere.Sphere.Center, integrateCandidate.Sphere.Center) +
						integrateCandidate.Sphere.Radius - superSphere.Sphere.Radius;
				if (dist < nearestDist) {
					nearest = superSphereIndex;
					nearestDist = dist;
				}

				superSphereIndex = superSphere.Next;
			}

			// if a super sphere contains it, just insert it!
			if (containsUs != -1) {
				AddChild(containsUs, entryID);
				return;
			}

			// check to see if the nearest sphere can grow to contain us
			if (nearest != -1) {
				SphereEntry parent = Spheres.Get(nearest);
				float newSize = nearestDist + parent.Sphere.Radius;
				if (newSize <= MaxSize) {
					parent.Sphere = new Sphere(parent.Sphere.Center, newSize + Gravy);
					Spheres.Set(nearest, parent);
					AddChild(nearest, entryID);
					return;
				}
			}

			// we'll have to make a new super sphere
			var newParent = new SphereEntry {
					Sphere = new Sphere(integrateCandidate.Sphere.Center, integrateCandidate.Sphere.Radius + Gravy),
					FirstChild = -1,
					Parent = 0,
					Next = rootSphere.FirstChild,
			};

			int parentID = Spheres.Insert(newParent);
			rootSphere.FirstChild = parentID;
			Spheres.Set(0, rootSphere);

			AddChild(parentID, entryID);
		}

		public void Recompute() {
			while (_recomputeFifo.Count > 0)
				if (_recomputeFifo.TryDequeue(out int recomputeCandidateID))
					Recompute(recomputeCandidateID);
		}

		private void Recompute(int superSphereID) {
			SphereEntry superSphere = Spheres.Get(superSphereID);

			if (superSphere.FirstChild == -1) {
				RemoveChild(0, superSphereID);
				Spheres.Erase(superSphereID);
				return;
			}

			Vector3 total = Vector3.zero;
			int childCount = 0;
			int childIndex = superSphere.FirstChild;
			while (childIndex != -1) {
				childCount++;
				SphereEntry child = Spheres.Get(childIndex);
				total += child.Sphere.Center;
				childIndex = child.Next;
			}

			float reciprocal = 1.0f / childCount;
			total *= reciprocal;
			Vector3 oldCenter = superSphere.Sphere.Center;
			superSphere.Sphere = new Sphere(total, superSphere.Sphere.Radius);

			float newRadius = 0.0f;
			childIndex = superSphere.FirstChild;

			while (childIndex != -1) {
				SphereEntry child = Spheres.Get(childIndex);
				float radius = Vector3.Distance(superSphere.Sphere.Center, child.Sphere.Center) + child.Sphere.Radius;
				if (radius > newRadius) {
					newRadius = radius;
					if (newRadius + Gravy > superSphere.Sphere.Radius) {
						superSphere.Sphere = new Sphere(oldCenter, superSphere.Sphere.Radius);
						return;
					}
				}

				childIndex = child.Next;
			}

			superSphere.Sphere = new Sphere(superSphere.Sphere.Center, newRadius + Gravy);
			Spheres.Set(superSphereID, superSphere);

			SphereEntry root = Spheres.Get(0);
			int possibleParentID = root.FirstChild;

			while (possibleParentID != -1) {
				if (possibleParentID == superSphereID) {
					possibleParentID = superSphere.Next;
					continue;
				}

				SphereEntry possibleParent = Spheres.Get(possibleParentID);
				if (possibleParent.Sphere.ContainsSphere(superSphere.Sphere)) {
					childIndex = superSphere.FirstChild;

					while (childIndex != -1) {
						SphereEntry child = Spheres.Get(childIndex);
						int next = child.Next;
						child.Next = possibleParent.FirstChild;
						child.Parent = possibleParentID;
						possibleParent.FirstChild = childIndex;
						Spheres.Set(childIndex, child);

						childIndex = next;
					}

					superSphere.FirstChild = -1;
					Spheres.Set(superSphereID, superSphere);
					Spheres.Set(possibleParentID, possibleParent);
					QueueRecompute(superSphereID);
					QueueRecompute(possibleParentID);

					break;
				}

				possibleParentID = possibleParent.Next;
			}
		}

		public List<SphereEntry> GetEntries(Sphere selectionSphere) {
			var results = new List<SphereEntry>();

			SphereEntry root = Spheres.Get(0);
			int sphereIndex = root.FirstChild;

			while (sphereIndex != -1) {
				SphereEntry sphere = Spheres.Get(sphereIndex);

				if (selectionSphere.IntersectsSphere(sphere.Sphere)) {
					int childIndex = sphere.FirstChild;

					while (childIndex != -1) {
						SphereEntry child = Spheres.Get(childIndex);

						if (selectionSphere.IntersectsSphere(child.Sphere)) results.Add(child);

						childIndex = child.Next;
					}
				}

				sphereIndex = sphere.Next;
			}

			return results;
		}
	}

	public struct SphereEntry {
		public int ID { get; set; }
		public Sphere Sphere { get; set; }
		public int Parent { get; set; }
		public int FirstChild { get; set; }
		public int Next { get; set; }
	}

	// FreeList is a structure that holds any kind of object.
	// When an entry is erased, it marks that slot as free so that new entries can be filled in to
	// the existing allocated memory.
	public struct FreeList<T> {
		public int FirstFree { get; private set; }

		private List<FreeListEntry> _data;

		private struct FreeListEntry {
			public T Element;
			public int NextFree;
		}

		public static FreeList<T> Create() {
			return new FreeList<T> {
					_data = new List<FreeListEntry>(),
					FirstFree = -1,
			};
		}

		public int Insert(T element) {
			if (FirstFree != -1) {
				int index = FirstFree;
				FreeListEntry freeListEntry = _data[index];
				FirstFree = freeListEntry.NextFree;
				freeListEntry.NextFree = 0;
				freeListEntry.Element = element;
				_data[index] = freeListEntry;
				return index;
			}

			_data.Add(new FreeListEntry { Element = element, NextFree = 0 });
			return _data.Count - 1;
		}

		public void Set(int index, T element) {
			if (index < 0 || index >= _data.Count) // TODO remove
				throw new ArgumentOutOfRangeException(nameof(index), "Invalid index.");

			var entry = _data[index];
			entry.Element = element;
			_data[index] = entry;
		}

		public void Erase(int index) {
			if (index < 0 || index >= _data.Count) // TODO Remove
				throw new ArgumentOutOfRangeException(nameof(index), "Invalid index.");

			var entry = _data[index];
			entry.NextFree = FirstFree;
			_data[index] = entry;
			FirstFree = index;
		}

		public void Clear() {
			_data.Clear();
			FirstFree = -1;
		}

		public T Get(int index) {
			if (index < 0 || index >= _data.Count) // TODO Remove
				throw new ArgumentOutOfRangeException(nameof(index), "Invalid index.");

			return _data[index].Element;
		}

		public int Count => _data.Count;
	}
}
