namespace FileFlows.ImageNodes.Images;

/// <summary>
/// Flow element that rotates an image
/// </summary>
public class ImageRotate: ImageNode
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc /> 
    public override string Icon => "fas fa-undo";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/image-nodes/image-rotate";

    /// <summary>
    /// Gets or sets angle to rotate the image
    /// </summary>
    [Select(nameof(AngleOptions), 2)]
    public int Angle { get; set; }

    private static List<ListOption>? _AngleOptions;
    /// <summary>
    /// Gest the Angle Options
    /// </summary>
    public static List<ListOption> AngleOptions
    {
        get
        {
            if (_AngleOptions == null)
            {
                _AngleOptions = new List<ListOption>
                {
                    new () { Value = 90, Label = "90"},
                    new () { Value = 180, Label = "180"},
                    new () { Value = 270, Label = "270"}
                };
            }
            return _AngleOptions;
        }
    }

    /// <inheritdoc />
    protected override Result<bool> PerformAction(NodeParameters args, string localFile, string destination)
        => args.ImageHelper.Rotate(localFile, destination, Angle, GetImageTypeFromFormat(), Quality);
}
