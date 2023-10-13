using System.Reflection;

namespace Game.Shared.WebSockets.Router;

public class HandlerMethodInfo
{
    public MethodInfo MethodInfo { get; }
    public bool RequiresAuthentication { get; }
    public bool IsAwaitable { get; }
    public ParameterInfo[] Parameters { get; }
    public bool HasParameters { get; }

    public HandlerMethodInfo(MethodInfo methodInfo, bool requiresAuthentication = false)
    {
        MethodInfo = methodInfo;
        RequiresAuthentication = requiresAuthentication;

        IsAwaitable = methodInfo.ReturnType.GetMethod(nameof(Task.GetAwaiter)) != null;
        Parameters = methodInfo.GetParameters();
        HasParameters = Parameters.Length > 0;
    }
}