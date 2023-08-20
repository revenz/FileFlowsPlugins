namespace FileFlows.VideoNodes.FfmpegBuilderNodes;

/// <summary>
/// Flow element that disables an encoder
/// </summary>
public abstract class DisableEncoder:Node
{
    /// <summary>
    /// Gets the number of inputs
    /// </summary>
    public override int Inputs => 1;
    /// <summary>
    /// Gets the number of outputs
    /// </summary>
    public override int Outputs => 1;
    /// <summary>
    /// Gets the icon
    /// </summary>
    public override string Icon => "far fa-times-circle";
    /// <summary>
    /// Gets the flow element type
    /// </summary>
    public override FlowElementType Type => FlowElementType.Logic;
    
    /// <summary>
    /// Gets the encoder variable to set to disabled
    /// </summary>
    protected abstract string EncoderVariable { get; }

    /// <summary>
    /// Executes the node
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public override int Execute(NodeParameters args)
    {
        args.Variables[EncoderVariable] = true;
        return 1;
    }
}