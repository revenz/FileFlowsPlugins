using BasicNodes.Tools;
using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Node that unzips a file
/// </summary>
public class Unzip :  Node
{
    /// <summary>
    /// Gets that this node is obsolete
    /// </summary>
    public override bool Obsolete => true;
    /// <summary>
    /// Gets the obsolete message
    /// </summary>
    public override string ObsoleteMessage => "This has been replaced with the Unpack flow element.\n\nUse that instead.";
    public override int Inputs => 1;
    public override int Outputs => 1;
    public override FlowElementType Type => FlowElementType.Process;
    public override string Icon => "fas fa-file-archive";
    /// <summary>
    /// Gets the Help URL for this element
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/unzip";
    
    private string _DestinationPath = string.Empty;
    private string _zip = string.Empty;

    [Folder(1)]
    public string DestinationPath
    {
        get => _DestinationPath;
        set { _DestinationPath = value ?? ""; }
    }

    [TextVariable(2)]
    public string Zip
    {
        get => _zip;
        set { _zip = value ?? ""; }
    }

    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args">the arguments</param>
    /// <returns>the output</returns>
    public override int Execute(NodeParameters args)
    {
        var unpack = new Unpack();
        unpack.File = Zip;
        unpack.DestinationPath = DestinationPath;
        return unpack.Execute(args);
    }
}
