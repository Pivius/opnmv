using Sandbox;
using System;
using OMMovement;

namespace Core
{
	public struct TraceUtil
	{
		private static void DebugHull(float duration, Vector3 start, Vector3 mins, Vector3 maxs, Color color)
		{
			if (Host.IsClient)
				DebugOverlay.Box(duration, start, mins, maxs, color);
		}

		public static TraceResult Hull(Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, Entity pawn, float lift_feet = 0.0f)
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

		public static TraceResult PlayerBBox(Vector3 start, Vector3 end, MovementController controller, float lift_feet = 0.0f)
		{
			Vector3 maxs = controller.GetPlayerMaxs();
			Vector3 mins = controller.GetPlayerMins();

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
						.Ignore(controller.Pawn)
						.Run();

			return tr;
		}

		public static Trace NewHull(Vector3 start, Vector3 end, Vector3 mins, Vector3 maxs, float lift_feet = 0.0f)
		{
			if (lift_feet > 0)
			{
				start += Vector3.Up * lift_feet;
				maxs = maxs.WithZ(maxs.z - lift_feet);
			}

			var tr = Trace.Ray(start, end)
						.Size(mins, maxs);

			return tr;
		}

		public static TraceResult PlayerLine(Vector3 start, Vector3 end, Entity pawn)
		{
			var tr = Trace.Ray(start, end)
						.HitLayer(CollisionLayer.All, false)
						.HitLayer(CollisionLayer.Solid, true)
						.HitLayer(CollisionLayer.GRATE, true)
						.HitLayer(CollisionLayer.PLAYER_CLIP, true)
						.Ignore(pawn)
						.Run();

			return tr;
		}

		public static Trace NewLine(Vector3 start, Vector3 end)
		{
			var tr = Trace.Ray(start, end)
						.HitLayer(CollisionLayer.All, false)
						.HitLayer(CollisionLayer.Solid, true)
						.HitLayer(CollisionLayer.GRATE, true)
						.HitLayer(CollisionLayer.PLAYER_CLIP, true);

			return tr;
		}
	}
}
