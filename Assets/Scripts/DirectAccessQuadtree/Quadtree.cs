using System;
using UnityEngine;

namespace DirectAccessQuadtree {
	public class Quadtree {
		private const int DefaultCapacity = 100;
		public const int Depth = 8; // 9
		public const int Width = 1 << 8; // 256

		public Rect[] Rects = new Rect[DefaultCapacity];
		public Rect[][] QuadLevels = new Rect[Depth][];

		public Quadtree(int capacity = DefaultCapacity) {
			// Initialize quadtree
			for (int i = 0; i < Depth; ++i) {
				int len = Math.Max(1, Mathf.RoundToInt(Mathf.Pow(4, i)));
				QuadLevels[i] = new Rect[len];
			}

			var rand = new Unity.Mathematics.Random(0x6E624EB7u);
			var minWidth = 1.0f;
			var maxWidth = 10.0f;
			Debug.Log(Width);
			for (ushort index = 0; index < capacity; ++index) {
				float rectWidth = rand.NextFloat(minWidth, maxWidth);
				float rectHeight = rand.NextFloat(minWidth, maxWidth);
				float left		= rand.NextFloat(0f, Width - rectWidth);
				float bottom	= rand.NextFloat(0f, Width - rectHeight);
				float top		= bottom + rectHeight;
				float right		= left + rectWidth;
				Rects[index] = new Rect {
						left = left,
						right = right,
						top = top,
						bottom = bottom,
						Count = 1,
						FirstChild = index,
				};
				int next = InsertRect(Rects[index], index, left, top, right, bottom);
				// if (next == index) continue;
				//
				// do {
				// 	
				// } while (Rects[Rects[next].FirstChild].Count != -1);
			}

			// Debug.Log($"{empty}/{occupied}");
		}

		public static (int level, int index) GetLevelIndex(Rect rect) =>
				GetLevelIndex(rect.left, rect.top, rect.right, rect.bottom);

		public static (int level, int index) GetLevelIndex(float left, float top, float right, float bottom) {
			int x1 = (int)left;
			int y1 = (int)top;
			int xResult = x1 ^ (int)right;
			int yResult = y1 ^ (int)bottom;

			int nodeLevel = Depth;
			int shiftCount = 0;

			int escape = 0;
			while (xResult + yResult > 0) {
				xResult >>= 1;
				yResult >>= 1;
				--nodeLevel;
				++shiftCount;
			}

			if (escape == 1000) {
				Debug.LogError("overflow");
				return (0, 0);
			}

			x1 >>= shiftCount;
			y1 >>= shiftCount;

			int index = (y1 << nodeLevel - 1) + x1;
			// if (nodeLevel <= 0) return (0, 0);
			return (nodeLevel, index);
		}

		public Rect GetNodeContaining(float left, float top, float right, float bottom) {
			(int level, int index) = GetLevelIndex(left, top, right, bottom);
			return QuadLevels[level][index];
		}

		public void InsertRect(Rect rect) {
			(int level, int index) = GetLevelIndex(rect.left, rect.top, rect.right, rect.bottom);
			QuadLevels[level][index] = rect;
		}

		private int empty = 0;
		private int occupied = 0;
		public int InsertRect(Rect rect, ushort referenceIndex, float left, float top, float right, float bottom) {
			(int level, int index) = GetLevelIndex(left, top, right, bottom);

			int next;
			if (QuadLevels[level][index].Count == 0) {		// Insert rect in empty node
				++empty;
				rect.FirstChild = referenceIndex;			
				rect.Count = 1;
				QuadLevels[level][index] = rect;			
				next = referenceIndex;
			} else {										// Leave node, increment count
				++occupied;
				++QuadLevels[level][index].Count;
				next = QuadLevels[level][index].FirstChild;
			}

			// TBD: Send back the first index and have the calling function chase the indexes until it's -1 ??? 
			return next;
		}
	}

	public struct Rect {
		public Int16 Count;
		public UInt16 FirstChild;
		public float left, right, top, bottom;
	}
}
