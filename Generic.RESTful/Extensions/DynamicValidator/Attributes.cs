using System;

namespace Generic.RESTful.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ValidateAttribute : Attribute
    {
        private static readonly string[] namespaces =
        {
            "System", "System.Text", "System.Text.RegularExpressions",
            "System.Collections.Generic"
        };

        public string[] Namespaces { get; set; }
        public string Expression { get; private set; }

        public ValidateAttribute(string expression)
        {
            this.Expression = expression;
            this.Namespaces = namespaces;
        }

    }
}