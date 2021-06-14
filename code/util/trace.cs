using Sandbox;
using System;

namespace Core
{
	public struct TraceUtil
	{

		public static TraceResult PlayerBBox(Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, Entity pawn, float lift_feet = 0.0f)
		{
			if (lift_feet > 0)
			{
				start += Vector3.Up * lift_feet;
				maxs = maxs.WithZ(maxs.z - lift_feet);
			}

			var tr = Trace.Ray(start, end)
						.Size(mins, maxs)
						.HitLayer(CollisionLayer.All, false)
						.HitLayer(CollisionLayer.Solid, true)
						.HitLayer(CollisionLayer.GRATE, true)
						.HitLayer(CollisionLayer.PLAYER_CLIP, true)
						.Ignore(pawn)
						.Run();

			return tr;
		}

		public static Trace CreateBBox(Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, Entity pawn, float lift_feet = 0.0f)
		{
			if (lift_feet > 0)
			{
				start += Vector3.Up * lift_feet;
				maxs = maxs.WithZ(maxs.z - lift_feet);
			}

			var tr = Trace.Ray(start, end)
						.Size(mins, maxs)
						.Ignore(pawn);

			return tr;
		}
	}
}
