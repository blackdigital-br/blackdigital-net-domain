namespace BlackDigital.Domain.Infrastructures
{
    /// <summary>
    /// Represents a template-based email that extends <see cref="Email"/> with a template name
    /// and a dictionary of placeholder parameters used to render the HTML template.
    /// </summary>
    public class TemplateEmail : Email
    {
        /// <summary>
        /// Gets or sets the name of the template file (without path).
        /// The <c>.html</c> extension is appended automatically if not present.
        /// </summary>
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the placeholder parameters used to render the template.
        /// Keys match <c>{{ key }}</c> placeholders in the template file.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new();

        /// <summary>
        /// Sets the template name to use for this email.
        /// </summary>
        /// <param name="templateName">The template file name (without path).</param>
        /// <returns>This <see cref="TemplateEmail"/> instance for method chaining.</returns>
        public TemplateEmail UseTemplate(string templateName)
        {
            TemplateName = templateName;
            return this;
        }

        /// <summary>
        /// Adds or updates a single template parameter.
        /// </summary>
        /// <param name="name">The placeholder key.</param>
        /// <param name="value">The value to substitute.</param>
        /// <returns>This <see cref="TemplateEmail"/> instance for method chaining.</returns>
        public TemplateEmail AddParameter(string name, string value)
        {
            Parameters[name] = value;
            return this;
        }

        /// <summary>
        /// Adds multiple template parameters.
        /// </summary>
        /// <param name="parameters">The parameters to add.</param>
        /// <param name="overwrite">Whether to overwrite existing keys. Defaults to <c>true</c>.</param>
        /// <returns>This <see cref="TemplateEmail"/> instance for method chaining.</returns>
        public TemplateEmail AddParameters(Dictionary<string, string> parameters, bool overwrite = true)
        {
            foreach (var parameter in parameters)
            {
                if (overwrite || !Parameters.ContainsKey(parameter.Key))
                    Parameters[parameter.Key] = parameter.Value;
            }

            return this;
        }
    }
}
