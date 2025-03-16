namespace FileFlows.BasicNodes.Functions;

using FileFlows.Plugin;
using FileFlows.Plugin.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Node that sleeps for a given time
/// </summary>
public class Sleep : Node
{
    /// <inheritdoc />
    public override int Inputs => 1;
    /// <inheritdoc />
    public override int Outputs => 1;
    /// <inheritdoc />
    public override FlowElementType Type => FlowElementType.Logic;
    /// <inheritdoc />
    public override string Icon => "fas fa-clock";
    /// <inheritdoc />
    public override string HelpUrl => "https://fileflows.com/docs/plugins/basic-nodes/sleep";
    /// <inheritdoc />
    public override bool FailureNode => true;

    private CancellationTokenSource _cancellationTokenSource;


    [NumberInt(1)]
    [Range(1, 3_600_000)]
    [DefaultValue(1000)]
    public int Milliseconds{ get; set; }

    /// <inheritdoc />
    public override int Execute(NodeParameters args)
    {
        if (Milliseconds < 1 || Milliseconds > 3_600_000)
        {
            args.Logger.ELog("Milliseconds must be between 1 and 3,600,000");
            return -1;
        }

        // Initialize a new CancellationTokenSource for this execution.
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        try
        {
            // Start the task to run the sleep operation with cancellation support.
            var _executionTask = Task.Run(() => RunSleep(cancellationToken, args));
            // Wait for the task to complete or be canceled, using the cancellation token.
            _executionTask.Wait(cancellationToken);

            // Return 1 to indicate the operation completed successfully.
            return 1;
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation of the operation.
            args.Logger.ELog("Execution was canceled.");
            // Return -1 to indicate that the operation was canceled.
            return -1;
        }
    }

    /// <summary>
    /// Runs the sleep operation asynchronously, supporting cancellation.
    /// </summary>
    /// <param name="cancellationToken">The token that allows for cancellation of the operation.</param>
    /// <param name="args">The parameters passed into the node execution, including logging capabilities.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task RunSleep(CancellationToken cancellationToken, NodeParameters args)
    {
        try
        {
            // Use Task.Delay with the cancellation token to simulate sleeping and support cancellation.
            await Task.Delay(Milliseconds, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Catch the cancellation and rethrow to propagate it to the caller.
            throw;
        }
    }

    /// <inheritdoc />
    public override Task Cancel()
    {
        // Trigger the cancellation of the running task if it exists.
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
        }

        // Return a completed task to indicate the cancellation action has been requested.
        return Task.CompletedTask;
    }
}
