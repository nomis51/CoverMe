namespace CoverMe.Ipc.Exceptions;

public class IpcChannelCreationFailedException : Exception
{
    public static IpcChannelCreationFailedException Static { get; } = new();
}