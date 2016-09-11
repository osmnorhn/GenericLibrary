using Generic.Utils.Reflection;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Generic.Utils.Extensions;
using Microsoft.CSharp;

namespace Generic.RESTful.Validation
{

    public static class DynamicValidator
    {
        private static readonly Dictionary<Type, Dictionary<PropertyInfo, MethodInfo>> _cache =
            new Dictionary<Type, Dictionary<PropertyInfo, MethodInfo>>();

        private const string MetHodTemplate = @"
            {0}         
            namespace DynamicValidation
            {1}                
                public static class {3}
                {1}                
                    public static bool IsValid({4} value)
                    {1}
                        return {5};
                    {2}
                {2}
            {2}";


        public static void Init(IEnumerable<Assembly> assemblies)
        {
            if (!assemblies.IsEmptyList())
            {
                foreach (var asm in assemblies)
                {
                    foreach (var type in asm.GetTypes())
                    {
                        foreach (var pi in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                        {
                            var att = pi.GetCustomAttribute<ValidateAttribute>();
                            if (null != att && ReflectionExtensions.IsPrimitiveType(pi) && !String.IsNullOrEmpty(att.Expression))
                            {
                                Dictionary<PropertyInfo, MethodInfo> dic;
                                if (!_cache.TryGetValue(type, out dic))
                                {
                                    dic = new Dictionary<PropertyInfo, MethodInfo>();
                                    _cache.Add(type, dic);
                                }

                                dic.Add(pi, CreateMethodInfo(pi, att));
                            }
                        }
                    }
                }
            }
        }

        private static readonly CSharpCodeProvider codeProvider = new CSharpCodeProvider();

        private static MethodInfo CreateMethodInfo(PropertyInfo pi, ValidateAttribute att)
        {
            StringBuilder namespaces = new StringBuilder();
            if (att.Namespaces != null)
            {
                foreach (var ns in att.Namespaces)
                {
                    namespaces.Append("using ")
                        .Append(ns)
                        .Append(';');

                }
            }

            string parameterTypeName = "";
            if (pi.PropertyType.IsNullableType())
                parameterTypeName = pi.PropertyType.GetGenericArguments().First().FullName + '?';
            else
                parameterTypeName = pi.PropertyType.FullName;

            string className = pi.DeclaringType.Name + pi.Name + "Validator";

            string code = String.Format(MetHodTemplate, namespaces, "{", "}", className, parameterTypeName, att.Expression);

            CompilerParameters parameters = new CompilerParameters();
            parameters.GenerateInMemory = true;
            parameters.GenerateExecutable = false;
            parameters.ReferencedAssemblies.Add("System.dll");//Regex İçin

            CompilerResults results = codeProvider.CompileAssemblyFromSource(parameters, code);

            Type validationMethod = results.CompiledAssembly.GetType("DynamicValidation." + className);
            return validationMethod.GetMethod("IsValid");
        }

        //tYPE' I DA CACLE YECEK YAPI İÇERİSİNE GİR. Ek olarak hata mesajları da işlensin.
        public static bool IsValid<TModel>(this TModel model)
        {
            if (null != model)
            {
                Type modelType = typeof(TModel);
                Dictionary<PropertyInfo, MethodInfo> dic;
                if (_cache.TryGetValue(modelType, out dic))
                {
                    foreach (var kvp in dic)
                    {
                        PropertyInfo pi = kvp.Key;
                        MethodInfo miValidator = kvp.Value;
                        object entityValue = pi.GetValue(model);

                        bool b = (bool)miValidator.Invoke(null, new object[] { entityValue });
                        if (!b)
                            return false;
                    }
                }
                return true;//Eğer ilgili attribute yok ise true dönder.
            }
            return false;
        }
    }
}
