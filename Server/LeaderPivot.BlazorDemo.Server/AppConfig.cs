namespace LeaderPivot.BlazorDemo.Server;

public class AppConfig
{
    public readonly string EnvironmentName;

    public AppConfig(string envronmentName)
    {
        EnvironmentName = envronmentName;
    }
}
