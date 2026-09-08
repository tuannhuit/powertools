namespace PowerTools.Core.Models
{
    public class RemoteToolModule
    {
        /// <summary>
        /// The name of module which is the module identifier name
        /// </summary>
        public string Name { get; set; }

        /// <summary> 
        /// The display name of module
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets description of the tool
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the entry point of the tool
        /// </summary>
        public string ExecutionName { get; set; }

        /// <summary>
        /// The text icon of the tool, can be a unicode character or a string
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// The color of the icon, can be a hex color code or a named color
        /// </summary>
        public string IconColor { get; set; }

        /// <summary>
        /// The name of publisher of the tool, can be a person or an organization
        /// </summary>
        public string PublisherDisplayName { get; set; }

        /// <summary>
        /// The link to the repository of the tool. Based on the RepoType (github, gitlab, bitbucket), the link can be constructed
        /// </summary>
        public string RepoLink { get; set; }

        /// <summary>
        /// The name of the owner of the tool, can be a person or an organization
        /// </summary>
        public string OwnerName { get; set; }

        /// <summary>
        /// The name of the repository of the tool
        /// </summary>
        public string RepoName { get; set; }

        /// <summary>
        /// The type of the repository of the tool (e.g., github, gitlab, bitbucket)
        /// </summary>
        public string RepoType { get; set; }

        public string IconImageRelativeLocation { get; set; }

        /// <summary>
        /// PowerTools use Token to query Remote Module
        /// </summary>
        public bool TokenRequired { get; set; }
    }
}
