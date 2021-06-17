using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace OMHUD
{
	public class Container : Panel
	{
		public Dictionary<string, Elements> Elements = new Dictionary<string, Elements>();
		public Dictionary<string, string> Properties = new Dictionary<string, string>();
		public string StylePath {get; set;} = "/ui/hud/containers.scss";
		public int Alignment {get; set;} = 1;
		public Container()
		{
			StyleSheet.Load("/ui/hud/containers.scss");
		}

		public void SetStyleSheet(string path)
		{
			if (path != StylePath)
			{
				StyleSheet.Load(path);
				StylePath = path;
			}
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

		public void SetProperties(Dictionary<string, string> props)
		{
			foreach (var item in props)
			{
				Properties[item.Key] = item.Value;
			}
		}

		public void SetStyle(params string[] props)
		{
			foreach (string item in props)
			{
				var element_prop = item.Split(":");
				Style.Set(element_prop[0], element_prop[1]);
			}
		}

		public override void Tick()
		{
			foreach (var element in Elements)
			{
				element.Value.Tick();
			}
		}
	}
}
