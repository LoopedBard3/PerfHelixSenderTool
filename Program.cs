using Microsoft.DotNet.Helix.Client;
using Microsoft.DotNet.Helix.Client.Models;

if (args.Length != 1) throw new ArgumentException("Must pass one argument for the token to use.");
var username = "LoopedBard3";

var api = ApiFactory.GetAuthenticated(args[0]);
var queuesToTest = new List<string>()
{
    "Windows.10.Amd64.19H1.Tiger.Perf",
    "Ubuntu.2204.Amd64.Tiger.Perf",
    //"Windows.10.Amd64.20H2.Owl.Perf",
    //"Ubuntu.1804.Amd64.Owl.Perf",
    //"Windows.10.Arm64.Perf.Surf",
    //"Ubuntu.2004.Arm64.Perf"
};
var jobs = new List<ISentJob>();
string[] files = new string[] { "runTest.bat", "runTest.sh" };

foreach (var queue in queuesToTest)
{
    var command = queue.Contains("windows", StringComparison.OrdinalIgnoreCase)
                ? $"%HELIX_WORKITEM_PAYLOAD%\\runTest.bat"
                : $"$HELIX_WORKITEM_PAYLOAD/runTest.sh";

    var job = await api.Job.Define()
      .WithType($"test/{username}/tool")
      .WithTargetQueue(queue.ToLower())
      .WithSource($"test/{username}/tool")
      .DefineWorkItem($"{queue} job")
      .WithCommand(command)
      .WithFiles(files)
      .AttachToJob()
      .WithCreator($"{username}")
      .SendAsync();

    jobs.Add(job);
    Console.WriteLine($"Job '{job.CorrelationId}' created in queue {queue}.");
}


var taskList = new List<Task<JobPassFail>>();
var loopInterval = TimeSpan.FromMinutes(1);
var pollingInterval = (int)loopInterval.Subtract(TimeSpan.FromSeconds(1)).TotalMilliseconds;
foreach (var job in jobs)
{
    var waitTask = job.WaitAsync(pollingIntervalMs: pollingInterval);
    taskList.Add(waitTask);
    Console.WriteLine(job.CorrelationId);
}
var mainTask = Task.WhenAll(taskList);
while (!mainTask.IsCompleted)
{
    for (var i = 0; i < jobs.Count; i++)
    {
        var job = jobs[i];
        Console.WriteLine($"{job.CorrelationId}: {taskList[i].Status}");
    }
    Console.WriteLine();
    await Task.Delay(loopInterval);
}

foreach (ISentJob job in jobs)
{
    Console.WriteLine($"{job.CorrelationId}: https://helix.dot.net/api/jobs/{job.CorrelationId}/workitems?api-version=2019-06-17&access_token={args[0]}");
}


