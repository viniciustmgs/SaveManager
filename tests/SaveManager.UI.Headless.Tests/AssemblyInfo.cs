// AppServiceProvider holds a single static container, so test classes must not run
// concurrently. one class rebuilding it would swap the services out from under another.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
