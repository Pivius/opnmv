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
			container.SetProperties(new Dictionary<string, string>()
			{
				["Pitch"] = angle.pitch.ToString(),
				["Yaw"] = angle.yaw.ToString()
			});
			container.SetStyle("transform:rotateX(" + angle.pitch + "deg) " + "rotateY(" + angle.yaw + "deg) rotateZ(" + angle.roll + "deg)");
		}

		public void SetContainerAngle(Container container)
		{
			var align = container.Alignment;

			if (align == 1)
			{
				container.SetStyle("transform-origin:50% 50%");
				SetContainerAngle(container, new Angles(9,-10,-1));
			}
			else if (align == 3)
			{
				container.SetStyle("transform-origin:50% 50%");
				SetContainerAngle(container, new Angles(-9,-10,1));
			}
			else if (align == 4)
			{
				container.SetStyle("transform-origin:50% 50%");
				SetContainerAngle(container, new Angles(0,-10,-1));
			}
			else if (align == 6)
			{
				container.SetStyle("transform-origin:50% 50%");
				SetContainerAngle(container, new Angles(0,10,1));
			}
			else if (align == 7)
			{
				container.SetStyle("transform-origin:50% 50%");
				SetContainerAngle(container, new Angles(9,-10,-1));
			}
			else if (align == 9)
			{
				container.SetStyle("transform-origin:50% 50%");
				SetContainerAngle(container, new Angles(-9,-10,1));
			}
		}
		
		public void ContainerTick()
		{
			foreach (var container in Containers)
			{
				SetContainerAngle(container.Value);
				container.Value.Tick();
			}
		}
	}
}
