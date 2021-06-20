using System;
using Sandbox;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Core
{
	public struct Parse
	{
		/// <summary>
		/// Parse Vector, Angles, Rotation or numerals to list
		/// </summary>
		public static List<float> ToList(object obj)
		{
			var list = new List<float>();
			string[] object_array = obj.ToString().Split(',');

			for (int i = 0; i < object_array.Length; i++)
			{
				var match = Regex.Match(object_array[i], @"([-+]?[0-9]*\.?[0-9]+)");
				list.Add(Convert.ToSingle(match.Groups[1].Value));
			}
			
			return list;
		}

		public static T FromListToEquatable<T>(List<float> list)
		{
			string[] array = list.Select(i => i.ToString()).ToArray();
			var type = typeof(T).ToString();
			string value = array[0];

			for (int i = 1; i < array.Length; i++)
			{
				value = value + "," + array[i];
			}

			if (type == "Angles")
				return (T) (object) Angles.Parse(value);
			else if (type == "Rotation")
				return (T) (object) Rotation.Parse(value);
			else if (type == "Vector2")
				return (T) (object) Vector2.Parse(value);
			else if (type == "Vector3")
				return (T) (object) Vector3.Parse(value);
			else if (type == "Vector4")
				return (T) (object) Vector4.Parse(value);
				
			return (T) (object) value;
		}
	}
}
