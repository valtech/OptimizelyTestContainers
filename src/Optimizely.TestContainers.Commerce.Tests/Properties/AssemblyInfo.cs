using Xunit;

// We do not want integration tests to run in parallel at all to avoid deadlocks and database disposing itself multiple times
[assembly: CollectionBehavior(DisableTestParallelization = true)]