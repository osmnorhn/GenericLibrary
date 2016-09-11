using Generic.Utils.Collections;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Linq;
using System.Linq.Expressions;
using Generic.Utils.Reflection;

namespace Generic.Utils.Extensions
{
    public static class GenericExtensions
    {
        public static T Cast<T>(this object obj)
        {
            return null != obj ? (T)obj : default(T);
        }
        public static T ConvertTo<T>(this object value)
        {
            if (null != value)
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch { }
            }
            return default(T);
        }
        public static TDest? ConvertTo<TSource, TDest>(this TSource? value)
            where TSource : struct
            where TDest : struct
        {
            if (value.HasValue)
            {
                return ConvertTo<TDest>(value.Value);
            }
            return null;
        }

        public static object ConvertTo(this object value, TypeCode type)
        {
            if (null != value)
            {
                try
                {
                    return Convert.ChangeType(value, type);
                }
                catch { }
            }
            return null;
        }

        public static T? ToNullable<T>(this object value)
            where T : struct
        {
            if (null != value)
            {
                string stringValue = value.ToString();
                if (stringValue.Length != 0)
                    return ConvertTo<T>(value);
            }
            return null;
        }
        public static T ToValue<T>(this T? n)
            where T : struct
        {
            return n ?? default(T);
        }
        public static bool IsNull(this object obj)
        {
            return (null == obj || obj.ToString().Length == 0);
        }
        public static bool In<T>(this T obj, params T[] items)
        {
            if (null != obj)
            {
                foreach (T item in items)
                    if (obj.Equals(item))
                        return true;
            }
            return false;
        }


        public static T Copy<T>(this T source)
        {
            using (Stream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, source);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        public static bool IsEmptyList<T>(this IEnumerable<T> en)
        {
            if (en != null)
            {
                IList<T> list = en as IList<T>;
                if (list != null)
                    return (list.Count == 0 || (list.Count == 1 && list[0] == null));
                else
                {
                    using (IEnumerator<T> enumerator = en.GetEnumerator())
                    {
                        if (enumerator.MoveNext())
                            return enumerator.Current == null;
                        else
                            return true;
                    }
                }
            }
            return true;
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> source)
        {
            if (null != collection && !source.IsEmptyList())
            {
                foreach (T item in source)
                {
                    collection.Add(item);
                }
            }
        }

        private static readonly object syncRoot = new object();
        public static string GenerateUniqueName()
        {
            try
            {
                Monitor.Enter(syncRoot);
                byte[] arr = Guid.NewGuid().ToByteArray();
                long i = 1;
                foreach (byte b in arr)
                {
                    i *= (((int)b) + 1);
                }
                return string.Format("{0:x}", i - DateTime.Now.Ticks);
            }
            finally
            {
                Monitor.Exit(syncRoot);
            }
        }

        private static readonly object syncRoot2 = new object();
        public static int GenerateUniqueHashCode(params object[] args)
        {
            try
            {
                Monitor.Enter(syncRoot2);
                if (null == args || 0 == args.Length)
                    throw new ArgumentNullException(nameof(args));

                int hash = 17;
                foreach (object item in args)
                {
                    if (null == item)
                        throw new NullReferenceException("args.Item");

                    unchecked
                    {
                        hash = hash * 23 + item.GetHashCode();
                    }
                }

                return hash;
            }
            finally
            {
                Monitor.Exit(syncRoot2);
            }
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return String.IsNullOrEmpty(s);
        }

        public static Exception FindRoot(this Exception ex)
        {
            if (null != ex)
            {
                if (null != ex.InnerException)
                    return FindRoot(ex.InnerException);
            }
            return ex;
        }

        public static IEnumerable<T> Paging<T>(this IEnumerable<T> input, int page, int pagesize)
        {
            if (input != null)
                return input.Skip(page * pagesize).Take(pagesize);
            return new List<T>();
        }

        public static IList<T> ElementToList<T>(this T item)
        {
            List<T> ret = new List<T>();
            if (null != item)
                ret.Add(item);
            return ret;
        }

        public static SingleItemList<T> ToSingleItemList<T>(this T item)
        {
            return new SingleItemList<T>(item);
        }

        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            if (null != values && null != action)
                foreach (T obj in values)
                    action(obj);
        }

        public static object IfNullSetEmpty<TTarget>(this object value)
        {
            if (null == value)
            {
                Type targetType = typeof (TTarget);
                if (targetType == CachedTypes.String)
                    return String.Empty;
                else if (targetType.IsNullableType())
                {
                    Type genericType = targetType.GetGenericArguments()[0];
                    return Activator.CreateInstance(genericType);
                }
                else if (targetType == CachedTypes.ByteArray)
                    return new byte[0];
                else
                    throw new NotSupportedException(targetType.FullName);
            }
            return value;
        }

        public static IEnumerable<T> RemoveEmptyItemsBy<T>(this IEnumerable<T> list, Expression<Func<T, object>> prop)
        {
            if (!list.IsEmptyList() && null != prop)
            {
                var pi = ReflectionExtensions.GetPropertyInfo(prop);
                if (null != pi)
                {
                    return list.Where(i => !pi.GetValue(i).IsNull());//.ToList();
                }
            }
            return list;
        }
    }
}
