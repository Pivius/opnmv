using System.Numerics;
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Sandbox;

namespace Core
{
	public class Interp<T>
	{
		public bool Enabled {get; set;} = false;
		public List<float> StartValue {get; protected set;}
		public List<float> EndValue {get; protected set;}
		public List<float> Value {get; protected set;}
		public float Duration {get; set;}
		public float StartTime {get; set;}
		public float CurrentTime {get; set;}
		public float DeltaTime {get; set;} = 0.0f;
		public Func<float, float, float, float, float> Easing {get; set;}

		public Interp(object start, object end, float duration, float dt, Func<float, float, float, float, float> easing)
		{
			SetValues(start, end, duration, dt, easing);
		}

		public void SetValues(object start, object end, float duration, float dt, Func<float, float, float, float, float> easing)
		{
			Value = new List<float>(SetStartValue(start));
			SetEndValue(end);
			Duration = duration;
			Easing = easing;
			DeltaTime = dt;
			CurrentTime = 0;
			StartTime = 0;
		}

		public List<float> SetStartValue(object start)
		{
			if (typeof(T) == typeof(List<float>))
				StartValue = (List<float>) start;
			else
				StartValue = Parse.ToList(start);

			return StartValue;
		}

		public List<float> SetEndValue(object end)
		{
			if (typeof(T) == typeof(List<float>))
				EndValue = (List<float>) end;
			else
				EndValue = Parse.ToList(end);

			return EndValue;
		}

		public T GetValue()
		{
			var type = typeof(T).ToString();

			if (type == typeof(List<float>).ToString())
				return (T) (object) Value;
			else if (type == typeof(float).ToString())
				return (T) (object) Value[0];
			else
				return Parse.FromListToEquatable<T>(Value);
		}

		public void Reset()
		{
			Value = new List<float>(StartValue);
			CurrentTime = 0;
			StartTime = 0;
		}

		public float PerformEasing(float start, float end, float time, float duration)
		{
			return Easing(start, end - start, time, duration);
		}

		public void Update(float dt = 0)
		{
			if (Enabled == true)
			{
				var time = (dt <= 0) ? (Time.Now - StartTime) : (CurrentTime + dt);

				CurrentTime = MathX.Clamp(time, 0, Duration);

				if (CurrentTime == 0.0f || StartTime == 0.0f)
				{
					Value = new List<float>(StartValue);
					StartTime = Time.Now;
				}
				else if (CurrentTime == Duration)
				{
					Value = new List<float>(EndValue);
				}
				else
				{
					for (int i = 0; i < Value.Count; i++)
					{
						Value[i] = PerformEasing(StartValue[i], EndValue[i], CurrentTime, Duration);
					}
				}
			}
		}
	}

	public struct InterpFunctions // https://github.com/kikito/tween.lua/blob/master/tween.lua
	{
		public static float Linear(float start, float delta, float time, float duration)
		{
			return start + ((delta * time)/duration);
		}

		public static float InQuad(float start, float delta, float time, float duration)
		{
			time /= duration;

			return start + (delta * time * time);
		}

		public static float OutQuad(float start, float delta, float time, float duration)
		{
			time /= duration;
			return start + (-delta * time * (time - 2));
		}

		public static float InOutQuad(float start, float delta, float time, float duration)
		{
			time /= duration * 2;
			if (time < 1)
				return start + ((delta/2) * (time * time));
			else
				return start + ((-delta/2) * ((time - 1) * (time - 3) - 1));
		}

		public static float OutInQuad(float start, float delta, float time, float duration)
		{
			delta /= 2;
			time *= 2;

			if (time < duration)
				return OutQuad(start, delta, time, duration);
			else
				return InQuad(start + delta, delta, time - duration, duration);
		}

		public static float InCubic(float start, float delta, float time, float duration)
		{
			return start + (delta * MathF.Pow(time/duration, 3));
		}

		public static float OutCubic(float start, float delta, float time, float duration)
		{
			return start + (delta * MathF.Pow((time/duration) - 1, 3) + 1);
		}

		public static float InOutCubic(float start, float delta, float time, float duration)
		{
			time /= duration * 2;
			delta /= 2;

			if (time < 1)
				return start + ((delta) * time * time * time);
			else
			{
				time -= 2;
				return start + ((delta) * ((time * time * time) + 2));
			}
		}

		public static float OutInCubic(float start, float delta, float time, float duration)
		{
			delta /= 2;
			time *= 2;

			if (time < duration)
				return OutCubic(start, delta, time, duration);
			else
				return InCubic(start + delta, delta, time - duration, duration);
		}

		public static float InQuart(float start, float delta, float time, float duration)
		{
			return start + (delta * MathF.Pow(time/duration, 4));
		}
		public static float OutQuart(float start, float delta, float time, float duration) 
		{
			return start + (-delta * (MathF.Pow(time/duration - 1, 4) - 1));
		}
		public static float InOutQuart(float start, float delta, float time, float duration)
		{
			time /= duration * 2;
			delta /= 2;

			if (time < 1)
				return start + (delta * MathF.Pow(time, 4));
			else
				return start + (-delta * (MathF.Pow(time - 2, 4) - 2));
		}
		public static float OutInQuart(float start, float delta, float time, float duration)
		{
			delta /= 2;
			time *= 2;

			if (time < duration) 
				return OutQuart(start, delta, time, duration);
			else
				return InQuart(start + delta, delta, time - duration, duration);
		}

		public static float InQuint(float start, float delta, float time, float duration) 
		{
			return start + (delta * MathF.Pow(time/duration, 5));
		}

		public static float OutQuint(float start, float delta, float time, float duration) 
		{
			return start + (delta * (MathF.Pow(time/duration - 1, 5) + 1));
		}

		public static float InOutQuint(float start, float delta, float time, float duration)
		{
			time /= duration * 2;
			delta /= 2;
			if (time < 1)
				return start + (delta * MathF.Pow(time, 5));
			else
				return start + (delta * (MathF.Pow(time - 2, 5) + 2));
		}

		public static float OutInQuint(float start, float delta, float time, float duration)
		{
			time *= 2;
			delta /= 2;

			if (time < duration) 
				return OutQuint(start, delta, time, duration);
			else
				return InQuint(start + delta, delta, time - duration, duration);
		}

		public static float InSine(float start, float delta, float time, float duration) 
		{
			return start + (-delta * MathF.Cos(time/duration * (MathF.PI/2)) + delta);
		}

		public static float OutSine(float start, float delta, float time, float duration) 
		{
			return start + (delta * MathF.Sin(time/duration * (MathF.PI/2)));
		}

		public static float InOutSine(float start, float delta, float time, float duration) 
		{
			return start + (-delta/2 * (MathF.Cos(MathF.PI * time/duration) - 1));
		}

		public static float OutInSine(float start, float delta, float time, float duration)
		{
			time *= 2;
			delta /= 2;

			if (time < duration)
				return OutSine(start, delta, time, duration);
			else
				return InSine(start + delta, delta, time - duration, duration);
		}

		public static float InExpo(float start, float delta, float time, float duration)
		{
			if (time == 0)
				return start;
			else
				return (delta * 0.001f) - start + (delta * MathF.Pow(2, 10 * (time/duration - 1)));
		}
		public static float OutExpo(float start, float delta, float time, float duration)
		{
			if (time == duration)
				return start + delta;
			else
				return start + (delta * 1.001f * (-MathF.Pow(2, -10 * time/duration) + 1));
		}
		public static float InOutExpo(float start, float delta, float time, float duration)
		{
			if (time == 0)
				return start;
			else if (time == duration)
				return start + delta;
			else
			{
				time /= duration * 2;

				if (time < 1) 
					return (delta * 0.0005f) - start + (delta/2 * MathF.Pow(2, 10 * (time - 1)));
				else
					return start + (delta/2 * 1.0005f * (-MathF.Pow(2, -10 * (time - 1)) + 2));
			}
		}

		public static float OutInExpo(float start, float delta, float time, float duration)
		{
			time *= 2;
			delta /= 2;

			if (time < duration) 
				return OutExpo(start, delta, time, duration);
			else
				return InExpo(start + delta, delta, time - duration, duration);
		}

		public static float InCirc(float start, float delta, float time, float duration)
		{
			return start + (-delta * (MathF.Sqrt(1 - MathF.Pow(time/duration, 2)) - 1));
		}

		public static float OutCirc(float start, float delta, float time, float duration)
		{  
			return start + (delta * MathF.Sqrt(1 - MathF.Pow(time/duration - 1, 2)));
		}

		public static float InOutCirc(float start, float delta, float time, float duration)
		{
			time /= duration * 2;
			delta /= 2;

			if (time < 1)
				return start + (-delta * (MathF.Sqrt(1 - time * time) - 1));
			else
			{
				time -= 2;

				return start + (delta * (MathF.Sqrt(1 - time * time) + 1));
			}
		}

		public static float OutInCirc(float start, float delta, float time, float duration)
		{
			time *= 2;
			delta /= 2;

			if (time < duration)
				return OutCirc(start, delta, time, duration);
			else
				return InCirc(start + delta, delta, time - duration, duration);
		}
	}
}
