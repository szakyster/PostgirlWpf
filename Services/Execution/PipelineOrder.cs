namespace Postgirl.Services.Execution;

/// <summary>Conventional ordering constants for built-in pipeline steps.</summary>
public static class PipelineOrder
{
    public const int VariableSubstitution = 10;
    public const int PreRequestScript = 50;
    public const int PostResponseScript = 100;
}
