namespace FileFlows.ResellerPlugin.FlowElements;

/// <summary>
/// Sets the display name ot a reseller friendly name
/// </summary>
public class SetResellerDisplayName : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/reseller/set-reseller-display-name";
    /// <inheritdoc />
    public override string Group => "Reseller";
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;
    /// <inheritdoc />
    public override string Icon => "";

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        var username = Variables["ResellerUser"]?.ToString() ??
                          Variables["ResellerUserUid"]?.ToString();
        if (string.IsNullOrWhiteSpace(username))
        {
            args.Logger?.WLog("Failed to get reseller username");
            return 1;
        }

        var shortname = Variables["ShortName"]?.ToString();
        if (string.IsNullOrWhiteSpace(shortname))
        {
            args.Logger?.WLog("Failed to get shortname");
            return 1;
        }
        
        args.SetDisplayName($"{username}: {shortname}");
        
        return base.Execute(args);
    }
}