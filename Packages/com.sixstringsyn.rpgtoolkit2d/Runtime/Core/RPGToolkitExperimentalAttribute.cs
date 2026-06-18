using System;

namespace SixStringSyn.RPGToolkit2D.Runtime.Core
{
    /// <summary>
    /// Marks an API as experimental for the current package release.
    /// Experimental APIs are usable, but may receive source or behavioral changes
    /// before they are promoted to the stable public API surface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Delegate, Inherited = false)]
    public sealed class RPGToolkitExperimentalAttribute : Attribute
    {
        /// <summary>
        /// Creates an experimental API marker with a short rationale.
        /// </summary>
        /// <param name="reason">The reason the API is not considered stable yet.</param>
        public RPGToolkitExperimentalAttribute(string reason)
        {
            Reason = reason ?? string.Empty;
        }

        /// <summary>
        /// Gets the reason this API is still experimental.
        /// </summary>
        public string Reason { get; }
    }
}
