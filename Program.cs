using Microsoft.DotNet.Helix.Client;
using Microsoft.DotNet.Helix.Client.Models;

var username = "LoopedBard3";
var helixKey = Environment.GetEnvironmentVariable("HelixKey");
var api = ApiFactory.GetAuthenticated(helixKey);
var queuesToTest = new List<string>()
{
    "Windows.10.Amd64.19H1.Tiger.Perf",
    "Ubuntu.2204.Amd64.Tiger.Perf",
    "Windows.10.Amd64.20H2.Owl.Perf",
    "Ubuntu.1804.Amd64.Owl.Perf",
    "Windows.10.Arm64.Perf.Surf",
    "Ubuntu.2004.Arm64.Perf"
};
var jobs = new List<(ISentJob, string)>();
string[] files = new string[] { "runTest.bat", "runTest.sh" };

if (string.IsNullOrEmpty(helixKey)) throw new ArgumentException("Must set HelixKey to the key to use for helix access.");
foreach (var queue in queuesToTest)
{
    var command = queue.Contains("windows", StringComparison.OrdinalIgnoreCase)
                ? $"%HELIX_WORKITEM_PAYLOAD%\\runTest.bat"
                : $"chmod +x $HELIX_WORKITEM_PAYLOAD/runTest.sh; $HELIX_WORKITEM_PAYLOAD/runTest.sh";

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

    jobs.Add((job, queue));
    Console.WriteLine($"Job '{job.CorrelationId}' created in queue {queue}. (https://helix.dot.net/api/jobs/{job.CorrelationId}/workitems?api-version=2019-06-17&access_token={helixKey})");
}


var taskList = new List<Task<JobPassFail>>();
var loopInterval = TimeSpan.FromMinutes(2);
var pollingInterval = (int)loopInterval.Subtract(TimeSpan.FromSeconds(1)).TotalMilliseconds;
foreach (var (job, _) in jobs)
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
        Console.WriteLine($"{DateTime.Now} - {job.Item1.CorrelationId} - {job.Item2}: {taskList[i].Status}");
    }
    Console.WriteLine();
    await Task.Delay(loopInterval);
}

foreach (var (job, queue) in jobs)
{
    Console.WriteLine($"Finished {job.CorrelationId} - {queue}: https://helix.dot.net/api/jobs/{job.CorrelationId}/workitems?api-version=2019-06-17&access_token={helixKey}");
}


