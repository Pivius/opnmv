using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;


namespace OMHUD
{
	public partial class HUDModule : Panel
	{
		public float Distance {get; set;} = 3.0f;
		public Angles PrevViewDelta {get; set;}
		public Vector3 PrevPos {get; set;}
		public Dictionary<string, Elements> Elements = new Dictionary<string, Elements>();
		public Dictionary<string, Container> Containers = new Dictionary<string, Container>();

		// Offset Module on moving around
		public Vector2 DefaultOffset {get; set;}
		public Vector2 Offset {get; set;}
		public float OffsetAimRate{get; set;} = 25.0f;
		public float OffsetMoveRate{get; set;} = 10.0f;
		public float OffsetResetRate{get; set;} = 0.5f;

		// Rotate module on looking around
		public Angles DefaultRotation {get; set;}
		public Angles Rotation {get; set;}
		public float RotationRate {get; set;} = 45.0f;
		public float RotationResetRate {get; set;} = 0.5f;

		public HUDModule()
		{

			StyleSheet.Load( "/ui/hud/module.scss" );
			InitContainers();
						GetContainer("BottomLeft").NewElement<Velocity>("Velocity");
		}

		public void NewElement<T>(string identifier = null) where T : Elements, new()
		{
			if (identifier == null)
				identifier = typeof(T).ToString().Split(".")[1];

			if (!Elements.ContainsKey(identifier))
			{
				var child = AddChild<T>();
				Elements.Add(identifier, child);
			}
		}

		public Elements GetElement(string type)
		{
			Elements child = null;

			if (Elements.ContainsKey(type))
				child = Elements[type];

			return child;
		}

		public void DoRotation()
		{
			var player = Local.Pawn;
			var rotation_delta = player.EyeRot.Angles() - PrevViewDelta;

			if (!(rotation_delta.yaw > 90 || rotation_delta.yaw < -90))
				Rotation = Rotation.WithYaw(Rotation.yaw + ((rotation_delta.yaw - Rotation.yaw * RotationResetRate/10) * (RotationRate/100)));

			if (!(rotation_delta.pitch > 90 || rotation_delta.pitch < -90))
				Rotation = Rotation.WithPitch(Rotation.pitch + ((rotation_delta.pitch - Rotation.pitch * RotationResetRate/10) * (RotationRate/100)));

			PrevViewDelta = player.EyeRot.Angles();
		}

		public void DoOffset()
		{

		}
		
		public void SetStyle(params string[] props)
		{
			foreach (string item in props)
			{
				var element_prop = item.Split(":");
				Style.Set(element_prop[0], element_prop[1].Trim());
			}
		}

		public override void Tick()
		{
			var player = Local.Pawn;
			if ( player == null ) return;
			DoRotation();
			DoOffset();
			Rotation += DefaultRotation;
			Offset += DefaultOffset;
			SetStyle( "transform: translateX(" + Rotation.yaw + "px) translateY(" + (-Rotation.pitch) + "px)");
			//SetStyle( "transform: rotateX(" + Rotation.pitch + "deg) " + "rotateY(" + Rotation.yaw + "deg)");
			ContainerTick();
		}
	}

}
