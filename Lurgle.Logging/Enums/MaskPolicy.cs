namespace Lurgle.Logging
{
    /// <summary>
    ///     Masking policy to be used when masking properties
    /// </summary>
    public enum MaskPolicy
    {
        /// <summary>
        ///     No masking
        /// </summary>
        None,

        /// <summary>
        ///     Mask with static string
        /// </summary>
        MaskWithString,

        /// <summary>
        ///     Mask characters and digits
        /// </summary>
        MaskLettersAndNumbers
    }
}