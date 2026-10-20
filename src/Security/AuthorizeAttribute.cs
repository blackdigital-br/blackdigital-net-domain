namespace BlackDigital.Domain.Security
{
    /// <summary>
    /// Marks a class, interface, or method with its access requirements.
    /// </summary>
    /// <remarks>
    /// This attribute carries metadata only — the library never enforces it. The consuming
    /// application (for example a REST middleware or an ASP.NET Core filter) is responsible for
    /// reading it and rejecting requests that do not match. Use it without arguments to require
    /// an authenticated user only, or set <see cref="Roles"/> and/or <see cref="Policy"/> to
    /// narrow the access.
    /// </remarks>
    /// <example>
    /// <code>
    /// [Authorize]                                          // any authenticated user
    /// [Authorize(Roles = new[] { "Admin", "Manager" })]    // any of the listed roles
    /// [Authorize(Policy = "Category.Write")]               // named authorization policy
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AuthorizeAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the roles that grant access. A user holding any of these roles is
        /// authorized. When null or empty, no role is evaluated.
        /// </summary>
        public string[]? Roles { get; set; }

        /// <summary>
        /// Gets or sets the name of the authorization policy that must be satisfied by the
        /// authenticated user. When null or empty, no policy is evaluated.
        /// </summary>
        public string? Policy { get; set; }
    }
}
