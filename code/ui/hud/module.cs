using System.Runtime.Serialization;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Text;
using Core;


namespace OMHUD
{
	public partial class HUDModule : Panel
	{
		public Dictionary<string, Elements> Elements = new Dictionary<string, Elements>();
		public Dictionary<string, Container> Containers = new Dictionary<string, Container>();
		public Angles PrevViewAngles {get; set;}
		public Angles DeltaViewAngles {get; set;}
		public float OffsetAimRate{get; set;} = 45.0f;
		public float OffsetResetRate{get; set;} = 45f;

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

		public void DoOffset()
		{
			var player = Local.Pawn;
			var rotation_delta = player.EyeRot.Angles() - PrevViewAngles;

			DeltaViewAngles = DeltaViewAngles.WithYaw(InterpFunctions.Linear(DeltaViewAngles.yaw, MathX.Clamp((rotation_delta.yaw * OffsetAimRate) - DeltaViewAngles.yaw, -90, 90) , Time.Delta * OffsetResetRate, 25f));
			DeltaViewAngles = DeltaViewAngles.WithPitch(InterpFunctions.Linear(DeltaViewAngles.pitch, MathX.Clamp((rotation_delta.pitch * OffsetAimRate) - DeltaViewAngles.pitch, -90, 90), Time.Delta * OffsetResetRate, 25f));

			if (Angles.AngleVector(DeltaViewAngles).IsNaN)
				DeltaViewAngles = new Angles();

			PrevViewAngles = player.EyeRot.Angles();
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
			if ( Local.Pawn == null ) return;
			var transform = new PanelTransform();

			DoOffset();
			ContainerTick();
			transform.AddTranslateX(DeltaViewAngles.yaw);
			transform.AddTranslateY(-DeltaViewAngles.pitch);
			Style.Transform = transform;
			Style.Dirty();
		}
	}

}
