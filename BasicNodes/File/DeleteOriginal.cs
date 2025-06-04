using FileFlows.Plugin;

namespace FileFlows.BasicNodes.File;

/// <summary>
/// Flow element to delete the original input file/folder
/// </summary>
public class DeleteOriginal : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;

    /// <inheritdoc />
    public override int Outputs => 1;

    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Process;

    /// <inheritdoc />
    public override string Icon => "far fa-trash-alt";

    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/delete-original";

    /// <summary>
    /// Executes the flow element
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next, -1 to abort flow, 0 to end flow</returns>
    public override int Execute(NodeParameters args)
    {
        if (args.OriginalIsDirectory)
            return DeleteDirectory(args);

        if (args.LibraryFileName.StartsWith("http:") || args.LibraryFileName.StartsWith("https:"))
            return args.Fail("Cannot delete URL: " + args.LibraryFileName);

        return DeleteFile(args);
    }

    /// <summary>
    /// Deletes a folder
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    private int DeleteDirectory(NodeParameters args)
    {
        var dirExistsResult = args.FileService.DirectoryExists(args.LibraryFileName);
        if (dirExistsResult.Failed(out var error))
            return args.Fail($"Failed to test directory '{args.LibraryFileName}' exist: {error}");
        if (dirExistsResult.Value == false)
        {
            args.Logger?.ILog("Directory does not exist: " + args.LibraryFileName);
            return 1;
        }

        var deleteDirResult = args.FileService.DirectoryDelete(args.LibraryFileName, true);
        if (deleteDirResult.Failed(out error))
            return args.Fail($"Failed to delete directory '{args.LibraryFileName}': {error}");
        
        args.Logger.ILog("Directory deleted: " + args.LibraryFileName);
        return 1;
    }

    /// <summary>
    /// Deletes a file
    /// </summary>
    /// <param name="args">the node parameters</param>
    /// <returns>the output to call next</returns>
    private int DeleteFile(NodeParameters args)
    {
        var existsResult = args.FileService.FileExists(args.LibraryFileName);
        if (existsResult.Failed(out var error))
            return args.Fail($"Failed to test file '{args.LibraryFileName}' exist: {error}");
        if (existsResult.Value == false)
        {
            args.Logger?.ILog("File does not exist: " + args.LibraryFileName);
            return 1;
        }

        var deleteResult = args.FileService.FileDelete(args.LibraryFileName);
        if(deleteResult.Failed(out error))
            return args.Fail($"Failed to delete file '{args.LibraryFileName}': {error}");
        args.Logger.ILog("File deleted: " + args.LibraryFileName);
        return 1;
    }
}