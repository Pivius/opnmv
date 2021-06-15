using Sandbox;

namespace Core
{
	public static class Ease
	{
		public static float SimpleSpline(float value)
		{
			float value_squared = value * value;

			return (3 * value_squared - 2 * value_squared * value);
		}
	}
}
