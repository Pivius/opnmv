using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;


namespace OMHUD
{
	public partial class HUDModule : Panel
	{
		public void InitContainers()
		{
			NewContainer<Container>("TopLeft", 1);
			NewContainer<Container>("TopCenter", 2);
			NewContainer<Container>("TopRight", 3);
			NewContainer<Container>("Left", 4);
			NewContainer<Container>("Center", 5);
			NewContainer<Container>("Right", 6);
			NewContainer<Container>("BottomLeft", 7);
			NewContainer<Container>("BottomCenter", 8);
			NewContainer<Container>("BottomRight", 9);
		}

		public void NewContainer<T>(string identifier = null, int alignment = 1) where T : Container, new()
		{
			if (identifier == null)
				identifier = typeof(T).ToString().Split(".")[1];

			if (!Containers.ContainsKey(identifier))
			{
				var child = AddChild<T>();
				child.Alignment = alignment;
				Containers.Add(identifier, child);
			}
		}

		public Container GetContainer(string type)
		{
			Container child = null;

			if (Containers.ContainsKey(type))
				child = Containers[type];

			return child;
		}

		public void SetContainerAngle(Container container, Angles angle)
		{
			var transform = new PanelTransform();

			container.SetProperties(new Dictionary<string, object>()
			{
				["Pitch"] = angle.pitch,
				["Yaw"] = angle.yaw
			});
			transform.AddRotation(angle.pitch, angle.yaw, angle.roll);
			container.Style.Transform = transform;
			container.Style.Dirty();
		}

		public void ContainerTick()
		{
			foreach (var item in Containers)
			{
				var container = item.Value;
				
				container.Tick();
			}
		}
	}
}
