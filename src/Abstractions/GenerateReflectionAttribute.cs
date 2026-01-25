using System;

namespace McInc.Reflection
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, 
        Inherited = false, AllowMultiple = false)]
    public sealed class GenerateReflectionAttribute : Attribute
    {
        

        //public GenerateReflectionAttribute(bool includeInternals = false)
        //{
        //    this.IncludeInternals = includeInternals;
        //}

        public bool IncludeInternals { get; set; }
    }
}