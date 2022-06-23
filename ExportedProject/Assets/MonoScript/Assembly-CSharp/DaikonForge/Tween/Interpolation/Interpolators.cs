using System;
using System.Collections.Generic;

namespace DaikonForge.Tween.Interpolation
{
	public static class Interpolators
	{
		private static Dictionary<Type, object> registry;

		static Interpolators()
		{
			registry = new Dictionary<Type, object>();
			Register(IntInterpolator.Default);
			Register(FloatInterpolator.Default);
			Register(RectInterpolator.Default);
			Register(ColorInterpolator.Default);
			Register(Vector2Interpolator.Default);
			Register(Vector3Interpolator.Default);
			Register(Vector4Interpolator.Default);
		}

		public static Interpolator<T> Get<T>()
		{
			return (Interpolator<T>)Get(typeof(T), true);
		}

		public static object Get(Type type, bool throwOnNotFound)
		{
			if (type == null)
			{
				throw new ArgumentNullException("You must provide a System.Type value");
			}
			object value = null;
			if (!registry.TryGetValue(type, out value) && throwOnNotFound)
			{
				throw new KeyNotFoundException(string.Format("There is no default interpolator defined for type '{0}'", type.Name));
			}
			return value;
		}

		public static void Register<T>(Interpolator<T> interpolator)
		{
			registry[typeof(T)] = interpolator;
		}
	}
}
