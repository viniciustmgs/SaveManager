namespace SaveManager.Domain.Entities
{
    public enum HotKeyRegistrationStatus
    {
        Registered,

        // no gesture was supplied, so nothing was registered
        Skipped,

        // another application already owns this combination
        AlreadyInUse,

        // the key or modifier combination cannot be expressed on this platform
        UnsupportedGesture,

        // the OS rejected the request for a reason I cannot classify
        Failed
    }

    public readonly struct HotKeyRegistrationResult
    {
        public HotKeyRegistrationResult(HotKeyRegistrationStatus status, string? message = null)
        {
            Status = status;
            Message = message;
        }

        public HotKeyRegistrationStatus Status { get; }
        public string? Message { get; }

        public bool IsRegistered => Status == HotKeyRegistrationStatus.Registered;

        public bool IsFailure =>
            Status is HotKeyRegistrationStatus.AlreadyInUse
                or HotKeyRegistrationStatus.UnsupportedGesture
                or HotKeyRegistrationStatus.Failed;

        public static HotKeyRegistrationResult Registered { get; } =
            new(HotKeyRegistrationStatus.Registered);

        public static HotKeyRegistrationResult Skipped { get; } =
            new(HotKeyRegistrationStatus.Skipped);

        public static HotKeyRegistrationResult AlreadyInUse(string? message = null) =>
            new(HotKeyRegistrationStatus.AlreadyInUse, message);

        public static HotKeyRegistrationResult UnsupportedGesture(string? message = null) =>
            new(HotKeyRegistrationStatus.UnsupportedGesture, message);

        public static HotKeyRegistrationResult Failed(string? message = null) =>
            new(HotKeyRegistrationStatus.Failed, message);
    }
}