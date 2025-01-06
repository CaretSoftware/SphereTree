
using System.Runtime.CompilerServices;

namespace Helpers {
	public static class Math {
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float BitIncrement(float value) {
			int bits = System.BitConverter.SingleToInt32Bits(value);
			bits += value >= 0 ? 1 : -1;
			return System.BitConverter.Int32BitsToSingle(bits);
		}
		
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static float BitDecrement(float value) {
			int bits = System.BitConverter.SingleToInt32Bits(value);
			bits -= value >= 0 ? 1 : -1;
			return System.BitConverter.Int32BitsToSingle(bits);
		}
	}
}

