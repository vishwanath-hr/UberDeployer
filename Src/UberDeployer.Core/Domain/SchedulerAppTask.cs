using System;
using UberDeployer.Common.SyntaxSugar;

namespace UberDeployer.Core.Domain
{
  public class SchedulerAppTask
  {
    public SchedulerAppTask(string name, string executableName, string userId, int scheduledHour, int scheduledMinute, int executionTimeLimitInMinutes)
    {
      Guard.NotNullNorEmpty(name, "name");
      Guard.NotNullNorEmpty(executableName, "executableName");
      Guard.NotNullNorEmpty(userId, "userId");

      if (scheduledHour < 0 || scheduledHour > 23)
      {
        throw new ArgumentException("Hour must be between 0 and 23 (inclusive).", "scheduledHour");
      }

      if (scheduledMinute < 0 || scheduledMinute > 59)
      {
        throw new ArgumentException("Minute must be between 0 and 59 (inclusive).", "scheduledMinute");
      }

      if (executionTimeLimitInMinutes < 0)
      {
        throw new ArgumentException("Execution time limit must be a non-negative integer.", "executionTimeLimitInMinutes");
      }

      Name = name;
      ExecutableName = executableName;
      UserId = userId;
      ScheduledHour = scheduledHour;
      ScheduledMinute = scheduledMinute;
      ExecutionTimeLimitInMinutes = executionTimeLimitInMinutes;
    }

    public string Name { get; private set; }

    public string ExecutableName { get; set; }

    /// <summary>
    /// A reference to a user that will be used to run the scheduled task. Users are defined in target environments.
    /// </summary>
    public string UserId { get; private set; }

    public int ScheduledHour { get; private set; }

    public int ScheduledMinute { get; private set; }

    /// <summary>
    /// 0 - no limit.
    /// </summary>
    public int ExecutionTimeLimitInMinutes { get; private set; }
  }
}
