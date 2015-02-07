using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;

using Sandcastle.Core.BuildAssembler;
using Sandcastle.Core.BuildAssembler.BuildComponent;

// Search for "TODO" to find changes that you need to make to this build component template.

namespace MongoDB.SandcastleTools
{
    /// <summary>
    /// TODO: Set your build component's unique ID and description in the export attribute in the factory class
    /// below.
    /// </summary>
    /// <remarks>The <c>BuildComponentExportAttribute</c> is used to export your component so that the help
    /// file builder finds it and can make use of it.  The example below shows the basic usage for a common
    /// build component.  Multiple copies of build components can be created depending on their usage.  The
    /// host process will create instances as needed and will dispose of them when it is done with them.</remarks>
    public class ResolveMongoDBLinksComponent : BuildComponentCore
    {
        [BuildComponentExport("Resolve MongoDB Links")]
        public sealed class Factory : BuildComponentFactory
        {
            public override BuildComponentCore Create()
            {
                return new ResolveMongoDBLinksComponent(BuildAssembler);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">The parent build component</param>
        public ResolveMongoDBLinksComponent(BuildAssemblerCore assembler)
             : base(assembler)
        {
        }

        /// <summary>
        /// Initialize the build component
        /// </summary>
        /// <param name="configuration">The component configuration</param>
        public override void Initialize(XPathNavigator configuration)
        {

        }

        /// <summary>
        /// Apply this build component's changes to the document
        /// </summary>
        /// <param name="document">The document to modify</param>
        /// <param name="key">The document's key</param>
        public override void Apply(XmlDocument document, string key)
        {
            var mongoManualNodes = document.SelectNodes("//mongoManual");
            var mongoServerReleaseNodes = document.SelectNodes("//mongoServerRelease");

            var list = document.CreateElement("list");
        }
    }
}
