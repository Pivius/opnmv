using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace OMHUD
{
	public class Elements : Panel
	{
		public Dictionary<string, Panel> Labels = new Dictionary<string, Panel>();
		public Dictionary<string, string> Properties = new Dictionary<string, string>();
		public string StylePath {get; set;} = "/ui/hud/elements.scss";
		public Elements()
		{
			StyleSheet.Load(StylePath);
		}

		public void SetStyleSheet(string path)
		{
			if (path != StylePath)
			{
				StyleSheet.Load(path);
				StylePath = path;
			}
		}

		public void NewLabel(string identifier, string text, string classname)
		{
			if (!Labels.ContainsKey(identifier))
				Labels.Add(identifier, Add.Label(text, classname));
		}

		public void RemoveLabel(string identifier)
		{
			if (Labels.ContainsKey(identifier))
			{
				Labels[identifier].Delete();
				Labels.Remove(identifier);
			}
		}

		public void SetProperties(Dictionary<string, string> props)
		{
			foreach (var item in props)
			{
				Properties[item.Key] = item.Value;
			}
		}

		public void SetStyle(Panel child = null, params string[] props)
		{
			if (child == null)
				child = this;

			foreach (string item in props)
			{
				var element_prop = item.Split(":");
				child.Style.Set(element_prop[0], element_prop[1]);
			}
		}

		public override void Tick()
		{
		}
	}
}
