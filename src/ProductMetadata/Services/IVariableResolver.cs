using System.Collections.Generic;

namespace ProductMetadata.Services
{
    public interface IVariableResolver
    {
        string ResolveVariables(string value, IDictionary<string, string> variables);
    }

    public class VariableResolver : IVariableResolver
    {
        public static readonly IVariableResolver Default = new VariableResolver();

        public string ResolveVariables(string value, IDictionary<string, string> variables)
        {
            throw new System.NotImplementedException();
        }
    }
}
