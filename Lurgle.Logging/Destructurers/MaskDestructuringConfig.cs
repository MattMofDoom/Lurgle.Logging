using Serilog;
using Serilog.Configuration;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Lurgle.Logging.Destructurers
{
    /// <summary>
    ///     Add a mask destructurer configuration
    /// </summary>
    public static class MaskDestructuringConfig
    {
        /// <summary>
        ///     Mask configured properties
        /// </summary>
        /// <param name="config"></param>
        /// <returns></returns>
        public static LoggerConfiguration WithMaskProperties(this LoggerDestructuringConfiguration config)
        {
            return config.With(new MaskDestructuringPolicy());
        }
    }
}