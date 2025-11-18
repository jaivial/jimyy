namespace WorkflowAutomation.Core.Constants
{
    /// <summary>
    /// Standard node categories for organizing nodes in the UI
    /// </summary>
    public static class NodeCategories
    {
        /// <summary>
        /// Nodes that trigger workflow execution (webhooks, schedules, manual triggers)
        /// </summary>
        public const string Triggers = "Triggers";

        /// <summary>
        /// Action nodes that perform operations (HTTP requests, send email, etc.)
        /// </summary>
        public const string Actions = "Actions";

        /// <summary>
        /// Data transformation and manipulation nodes
        /// </summary>
        public const string Data = "Data";

        /// <summary>
        /// Logic and flow control nodes (if, switch, loop, etc.)
        /// </summary>
        public const string Logic = "Logic";

        /// <summary>
        /// Integration nodes for external services (Google, Slack, GitHub, etc.)
        /// </summary>
        public const string Integrations = "Integrations";

        /// <summary>
        /// Database operation nodes (query, insert, update, delete)
        /// </summary>
        public const string Database = "Database";

        /// <summary>
        /// File operation nodes (read, write, upload, download)
        /// </summary>
        public const string Files = "Files";

        /// <summary>
        /// Utility and helper nodes
        /// </summary>
        public const string Utilities = "Utilities";

        /// <summary>
        /// Communication nodes (email, SMS, notifications)
        /// </summary>
        public const string Communication = "Communication";

        /// <summary>
        /// AI and machine learning nodes
        /// </summary>
        public const string AI = "AI";

        /// <summary>
        /// Analytics and monitoring nodes
        /// </summary>
        public const string Analytics = "Analytics";

        /// <summary>
        /// Other/uncategorized nodes
        /// </summary>
        public const string Other = "Other";

        /// <summary>
        /// Gets all standard categories
        /// </summary>
        public static string[] All => new[]
        {
            Triggers,
            Actions,
            Data,
            Logic,
            Integrations,
            Database,
            Files,
            Utilities,
            Communication,
            AI,
            Analytics,
            Other
        };
    }
}
