using System;

namespace SphereTree {
	[Flags]
	public enum SphereTreeFlags { // TODO: Unused as of yet
		RootSphere	=	(1 << 0),
		SuperSphere	=	(1 << 1),
		LeafSphere	=	(1 << 2),
		RootNode	=	(1 << 3), 
		Recompute	=	(1 << 4), 
		Integrate	=	(1 << 5),
	}
	
	public static class SphereTreeFlagsExtensions {
		public static SphereTreeFlags Set(this SphereTreeFlags curr, SphereTreeFlags flags) => curr | flags;
		
		public static SphereTreeFlags Remove(this SphereTreeFlags curr, SphereTreeFlags flags) => curr & ~flags;
		
		public static bool Has(this SphereTreeFlags curr, SphereTreeFlags flags) => (curr & flags) > 0;
	}
}
