using Generic.Utils.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Generic.Utils.Reflection
{
    public static class ReflectionExtensions
    {
        private static readonly MethodInfo ToNullable_MethodInfo = typeof(GenericExtensions).GetMethod("ToNullable", BindingFlags.Static | BindingFlags.Public);

        private static MethodInfo convertTo_MethodInfo;
        private static readonly object forLock = new object();
        private static MethodInfo ConvertTo_MethodInfo
        {
            get
            {
                if (null == convertTo_MethodInfo)
                {
                    lock (forLock)
                    {
                        if (null == convertTo_MethodInfo)
                        {
                            convertTo_MethodInfo = ReflectionExtensions.FixAmbiguousMatch(typeof(GenericExtensions).GetMethods(BindingFlags.Static | BindingFlags.Public),
                                "ConvertTo", 1, 1);

                            if (null == convertTo_MethodInfo)
                                throw new NotFoundException("Generic.Extensions.ConvertTo method not found");
                        }
                    }
                }

                return convertTo_MethodInfo;
            }
        }
        public static MethodInfo FixAmbiguousMatch(MethodInfo[] methods, string methodName, int parameterCount, int genericParameterCount)
        {
            if (!methods.IsEmptyList() && !String.IsNullOrEmpty(methodName))
            {
                foreach (MethodInfo mi in methods)
                {
                    if (String.Equals(mi.Name, methodName))
                    {
                        var pars = mi.GetParameters();
                        if (!pars.IsEmptyList() && pars.Count() == parameterCount)
                        {
                            var genericPars = mi.GetGenericArguments();
                            if (!genericPars.IsEmptyList() && genericPars.Count() == genericParameterCount)
                            {
                                return mi;
                            }
                        }
                    }
                }
            }

            return null;
        }
        public static void SetValueSafely(this PropertyInfo pi, object entity, object propertyValue)
        {
            Type propertyType = pi.PropertyType;
            MethodInfo mi;
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == CachedTypes.PureNullableType)
            {
                mi = ReflectionExtensions.ToNullable_MethodInfo.MakeGenericMethod(propertyType.GetGenericArguments()[0]);
            }
            else
            {
                mi = ReflectionExtensions.ConvertTo_MethodInfo.MakeGenericMethod(propertyType);
            }
            pi.SetValue(entity, mi.Invoke(null, new object[] { propertyValue }));// Convert.ChangeType(dbValue, pi.PropertyType));
        }


        public static IList CreateGenericList(this Type elementType)
        {
            if (null != elementType)
            {
                Type genericType = typeof(List<>);
                Type listType = genericType.MakeGenericType(elementType);
                return Activator.CreateInstance(listType) as IList;
            }
            return null;
        }

        public static object GetDefault(this Type type)
        {
            if (null != type)
            {
                if (type.IsValueType)
                {
                    return Activator.CreateInstance(type);
                }
            }
            return null;
        }


        public static void CopyPropertiesFrom(this object destObject, object sourceObject)
        {
            if (null == destObject)
                throw new ArgumentNullException(nameof(destObject));
            if (null == sourceObject)
                throw new ArgumentNullException(nameof(sourceObject));

            Type destObjectType = destObject.GetType();
            foreach (PropertyInfo sourcePi in sourceObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                PropertyInfo destPi = destObjectType.GetProperty(sourcePi.Name);
                if (null != destPi && null != destPi.SetMethod)
                {
                    object sourcePropertyValue = sourcePi.GetValue(sourceObject);

                    destPi.SetValueSafely(destObject, sourcePropertyValue);
                }
            }
        }

        public static void CopyPropertiesFrom<T>(this T destObject, T sourceObject)
        {
            if (null == destObject)
                throw new ArgumentNullException(nameof(destObject));
            if (null == sourceObject)
                throw new ArgumentNullException(nameof(sourceObject));

            foreach (PropertyInfo pi in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (null != pi.SetMethod)
                {
                    object sourcePropertyValue = pi.GetValue(sourceObject);
                    pi.SetValue(destObject, sourcePropertyValue, null);
                }
            }
        }

        public static Type GetType(string typeName)//Type.GetType("TypeName,DllName");
        {
            if (String.IsNullOrEmpty(typeName))
                throw new ArgumentNullException(nameof(typeName));

            return Type.GetType(typeName, true, true);
        }



        public static MethodInfo GetMethod(Type objectType, string methodName, int parameterCount)
        {
            if (null != objectType && !String.IsNullOrEmpty(methodName))
            {
                MethodInfo method;
                try
                {
                    method = objectType.GetMethod(methodName);
                }
                catch (AmbiguousMatchException)
                {
                    method = ReflectionExtensions.FixAmbiguousMatch(objectType.GetMethods(BindingFlags.Public | BindingFlags.Instance), methodName, parameterCount, 1);
                }
                return method;
            }

            return null;
        }
        public static MethodInfo GetGenericMethod(Type objectType, string methodName, Type genericType, int parameterCount)
        {
            if (null != genericType)
            {
                MethodInfo method = GetMethod(objectType, methodName, parameterCount);
                if (null != method)
                {
                    MethodInfo generic = method.MakeGenericMethod(genericType);
                    return generic;
                }
            }

            return null;
        }
        public static object InvokeGeneric(this object obj, string methodName, Type genericType, params object[] pars)
        {
            if (null != obj)
            {
                MethodInfo generic = GetGenericMethod(obj.GetType(), methodName, genericType, (pars == null ? 0 : pars.Length));
                if (null != generic)
                    return generic.Invoke(obj, pars);
            }
            return null;
        }



        public static PropertyInfo GetPropertyInfo(Expression exp)
        {
            PropertyInfo pi = null;
            MemberExpression me = exp as MemberExpression;
            if (null != me)
            {
                pi = (PropertyInfo)me.Member;
            }
            else
            {
                UnaryExpression ue = exp as UnaryExpression;
                if (null != ue)
                {
                    return GetPropertyInfo(ue.Operand);
                }
                else
                {
                    LambdaExpression lax = exp as LambdaExpression;
                    if (null != lax)
                    {
                        return GetPropertyInfo(lax.Body);
                    }
                }
            }
            return pi;
        }

        public static TAttribute GetCustomAttribute<TAttribute>(this Expression exp, bool inherit)
            where TAttribute : Attribute
        {
            if (null != exp)
            {
                PropertyInfo pi = GetPropertyInfo(exp);
                if (null != pi)
                    return pi.GetCustomAttribute<TAttribute>(inherit);
            }
            return null;
        }
        public static TAttribute GetCustomAttribute<TAttribute>(this Expression exp)
            where TAttribute : Attribute
        {
            return GetCustomAttribute<TAttribute>(exp, false);
        }

        private static readonly HashSet<Type> primitiveTypes = new HashSet<Type>() { CachedTypes.String, CachedTypes.Decimal, CachedTypes.DateTime, CachedTypes.ByteArray, CachedTypes.Guid };
        public static bool IsPrimitiveType(PropertyInfo pi)
        {
            Type propertyType = pi.PropertyType;
            return propertyType.IsPrimitive || primitiveTypes.Contains(propertyType)
                   || (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == CachedTypes.PureNullableType);
        }


        private static readonly Type IEnumerableType = typeof(IEnumerable);
        public static bool IsEnumerable(PropertyInfo pi)
        {
            Type propertyType = pi.PropertyType;
            return (IEnumerableType.IsAssignableFrom(propertyType) && (propertyType != CachedTypes.String && propertyType != CachedTypes.ByteArray));
        }

        public static bool IsNullableType(this Type type)
        {
            if (null != type)
            {
                return type.IsGenericType && type.GetGenericTypeDefinition() == CachedTypes.PureNullableType;
            }

            return false;
        }

    }
}
