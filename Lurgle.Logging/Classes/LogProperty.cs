// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

// ReSharper disable InconsistentNaming

namespace Lurgle.Logging.Classes
{
    /// <summary>
    ///     Log property container
    /// </summary>
    public class LogProperty
    {
        /// <summary>
        ///     LogProperty Constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="destructure"></param>
        public LogProperty(string name, object value, bool destructure = false)
        {
            Name = name;
            Value = value;
            Destructure = destructure;
        }

        /// <summary>
        ///     Property Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Property Value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        ///     Destructure Property
        /// </summary>
        public bool Destructure { get; set; }
    }
}