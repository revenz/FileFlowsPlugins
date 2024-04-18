using FileFlows.Plugin;

namespace FileFlows.BasicNodes.Templating;

/// <summary>
/// Special templating node for the output path 
/// </summary>
public class OutputPath : TemplatingNode
{
    /// <summary>
    /// Gets the number of inputs
    /// </summary>
    public override int Inputs => 1;

    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "fas fa-folder";
    
    /// <summary>
    /// Gets or sets the URL to the help page
    /// </summary>
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/templating/output-path";
    
    /// <summary>
    /// Gets or sets the flow element type
    /// </summary>
    public override FlowElementType Type => FlowElementType.Logic;

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node paramters</param>
    /// <returns>the output</returns>
    public override int Execute(NodeParameters args)
    {
        args.Logger?.ELog("This templating flow element cannot be used in an executed flow");
        return -1;
    }
}