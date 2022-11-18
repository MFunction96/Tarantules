using Microsoft.PowerShell.Commands;
using System.Management.Automation;
using System.Text;
using System.Xml;

namespace Tarantules.Utils
{
    /// <summary>
    /// 
    /// </summary>
    [Cmdlet(VerbsDiagnostic.Resolve, "Nuspec")]
    public class GenerateNuspecPSCmdlet : PSCmdlet
    {
        /// <summary>
        /// 
        /// </summary>
        [Parameter(Mandatory = true)]
        public string SourceNuspec { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Parameter(Mandatory = true)]
        public string Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Parameter(Mandatory = false)]
        public string DestinationNuspec { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public const string NuspecSchema =
            "https://github.com/NuGet/NuGet.Client/raw/dev/src/NuGet.Core/NuGet.Packaging/compiler/resources/nuspec.xsd";

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            try
            {
                if (string.IsNullOrEmpty(this.SourceNuspec))
                {
                    this.ThrowTerminatingError(new ErrorRecord(new ArgumentException($"{nameof(this.SourceNuspec)} is invalid!"), "InvalidArgument", ErrorCategory.InvalidArgument, this.SourceNuspec));
                }

                this.SourceNuspec = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(this.SourceNuspec, out _, out _);
                if (!File.Exists(this.SourceNuspec))
                {
                    this.ThrowTerminatingError(new ErrorRecord(new ArgumentException($"{nameof(this.SourceNuspec)} is invalid!"), "InvalidArgument", ErrorCategory.InvalidArgument, this.SourceNuspec));

                }

                if (string.IsNullOrEmpty(this.DestinationNuspec))
                {
                    this.DestinationNuspec = this.SourceNuspec;
                    WriteWarning($"Parameter {nameof(this.DestinationNuspec)} is not set. Fallback to overwrite {nameof(this.SourceNuspec)}.");
                }
                else
                {
                    this.DestinationNuspec = this.SessionState.Path.GetUnresolvedProviderPathFromPSPath(this.DestinationNuspec, out _, out _);
                }
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Unexpected", ErrorCategory.InvalidResult, this.SourceNuspec));
            }

            WriteObject("All parameters are valid. Move on.");

            try
            {
                WriteObject("Generating the Nuspec file...");
                var xmlDocument = new XmlDocument();
                xmlDocument.Load(this.SourceNuspec);
                var ns = new XmlNamespaceManager(xmlDocument.NameTable);
                ns.AddNamespace("ns", xmlDocument.DocumentElement!.NamespaceURI);
                var metaNode = xmlDocument.SelectSingleNode("//ns:metadata", ns);
                var versionElement = xmlDocument.CreateElement("version", xmlDocument.DocumentElement.NamespaceURI);
                versionElement.InnerText = this.Version;
                metaNode!.AppendChild(versionElement);
                using var writer = new XmlTextWriter(this.DestinationNuspec, Encoding.UTF8)
                {
                    Formatting = Formatting.Indented,
                    IndentChar = '\t',
                    QuoteChar = '\"',
                    Indentation = 1
                };
                xmlDocument.Save(writer);
                WriteObject("Completed to generate the Nuspec file...");
            }
            catch (Exception e)
            {
                WriteError(new ErrorRecord(e, "Unexpected", ErrorCategory.InvalidResult, this.SourceNuspec));
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
        }

        protected override void StopProcessing()
        {
            base.StopProcessing();
        }

        private PSObject GetFileCustomObject(FileInfo file)
        {
            // this message will be shown if the -verbose switch is given
            WriteVerbose("GetFileCustomObject " + file);
            // create a custom object with a few properties
            var custom = new PSObject();
            custom.Properties.Add(new PSNoteProperty("Size", file.Length));
            custom.Properties.Add(new PSNoteProperty("Name", file.Name));
            custom.Properties.Add(new PSNoteProperty("Extension", file.Extension));
            return custom;
        }
        private PSObject GetDirectoryCustomObject(DirectoryInfo dir)
        {
            // this message will be shown if the -verbose switch is given
            WriteVerbose("GetDirectoryCustomObject " + dir);
            // create a custom object with a few properties
            var custom = new PSObject();
            var files = dir.GetFiles().Length;
            var subdirs = dir.GetDirectories().Length;
            custom.Properties.Add(new PSNoteProperty("Files", files));
            custom.Properties.Add(new PSNoteProperty("Subdirectories", subdirs));
            custom.Properties.Add(new PSNoteProperty("Name", dir.Name));
            return custom;
        }
        private bool IsFileSystemPath(ProviderInfo provider, string path)
        {
            var isFileSystem = true;
            // check that this provider is the filesystem
            if (provider.ImplementingType == typeof(FileSystemProvider)) return isFileSystem;
            // create a .NET exception wrapping our error text
            var ex = new ArgumentException(path + " does not resolve to a path on the FileSystem provider.");
            // wrap this in a powershell errorrecord
            var error = new ErrorRecord(ex, "InvalidProvider", ErrorCategory.InvalidArgument, path);
            // write a non-terminating error to pipeline
            this.WriteError(error);
            // tell our caller that the item was not on the filesystem
            isFileSystem = false;
            return isFileSystem;
        }
    }
}