using System.Collections.Generic;

namespace Lurgle.Logging
{
    public interface ILevel : IHideObjectMembers
    {
        IAddProperty AddProperty(string name, object value);
        IAddProperty AddProperty(Dictionary<string, object> propertyPairs);
        void Add(string logTemplate, params object[] args);
    }
}
