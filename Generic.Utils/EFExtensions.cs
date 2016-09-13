using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Generic.Utils
{
    public static class EFExtensions
    {
        private static readonly HashSet<Type> PrimitiveTypes = new HashSet<Type>()
        {
            typeof(String),
            typeof(Boolean),
            typeof(Byte),
            typeof(byte[]),
            typeof(Char),
            typeof(DateTime),
            typeof(Decimal),
            typeof(Double),
            typeof(Single),
            typeof(Guid),
            typeof(Int16),
            typeof(Int32),
            typeof(Int64),
            typeof(UInt16),
            typeof(UInt32),
            typeof(UInt64),
            typeof(SByte),
            typeof(Boolean?),
            typeof(Byte?),
            typeof(Char?),
            typeof(DateTime),
            typeof(Single),
            typeof(Decimal?),
            typeof(Double?),
            typeof(Guid?),
            typeof(Int16?),
            typeof(Int32?),
            typeof(Int64?)
            ,
            typeof(UInt16?),
            typeof(UInt32?),
            typeof(UInt64?),
            typeof(SByte?)
        };

        private static readonly MethodInfo MethodInfoCopyPropertiesFrom =
            typeof(EFExtensions).GetMethod("CopyPropertiesFrom");

        public static void CopyPropertiesFrom<T>(this T destObject, T sourceObject)
        {
            //if (destObject == null)
            //   throw new ArgumentNullException(nameof(destObject));
            //if (sourceObject == null)
            // throw new ArgumentNullException(nameof(sourceObject));

            foreach (PropertyInfo pi in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (pi.SetMethod != null)
                {
                    Type piType = pi.PropertyType;
                    if (PrimitiveTypes.Contains(piType))
                    {
                        object sourcePropertyValue = pi.GetValue(sourceObject);
                        pi.SetValue(destObject, sourcePropertyValue, null);
                    }
                    else
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(piType)) continue; //Şimdilik

                        if (piType.IsInterface || piType.IsAbstract) continue;
                        ;

                        object sourcePropertyValueEntity = pi.GetValue(sourceObject); //this is proxy Type
                        if (sourcePropertyValueEntity != null) //Custom type and not null
                        {
                            object destEmptyObject = Activator.CreateInstance(piType); //Burası proxy değil işte.
                            MethodInfo mi = MethodInfoCopyPropertiesFrom.MakeGenericMethod(piType);
                            mi.Invoke(null, BindingFlags.Static, null,
                                new[] { destEmptyObject, sourcePropertyValueEntity }, null);

                            pi.SetValue(destObject, destEmptyObject);
                        }
                    }
                }
            }
        }

        public static TEntity ToPoco<TEntity>(this TEntity proxy)
            where TEntity : new()
        {
            TEntity t = new TEntity();
            t.CopyPropertiesFrom(proxy);
            return t;
        }

        public static TEntity ToPocoSafe<TEntity>(this TEntity proxy, DbContext context)
            where TEntity : new()
        {
            bool temp = context.Configuration.LazyLoadingEnabled;
            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                return ToPoco(proxy);
            }
            finally
            {
                context.Configuration.LazyLoadingEnabled = temp;
            }
        }

        public static IEnumerable<TEntity> ToPocoList<TEntity>(this IEnumerable<TEntity> list)
            where TEntity : new()
        {
            List<TEntity> ret = new List<TEntity>();
            if (list != null && list.Any())
            {
                foreach (var item in list)
                {
                    ret.Add(item.ToPoco());
                }
            }

            return ret;
        }
    }
}