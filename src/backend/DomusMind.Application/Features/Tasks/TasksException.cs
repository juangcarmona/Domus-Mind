namespace DomusMind.Application.Features.Tasks;

public enum TasksErrorCode
{
    TaskNotFound,
    RoutineNotFound,
    AccessDenied,
    InvalidInput,
    TaskAlreadyCompleted,
    TaskAlreadyCancelled,
    RoutineAlreadyPaused,
    RoutineAlreadyActive,
}

public sealed class TasksException : Exception
{
    public TasksErrorCode Code { get; }

    public TasksException(TasksErrorCode code, string message) : base(message)
    {
        Code = code;
    }
}
