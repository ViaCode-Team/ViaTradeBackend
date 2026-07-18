namespace Domain.Common;

/// <summary>
/// Base marker for a Value Object.
/// In modern C#, value-based equality is automatically handled by the `record` type.
/// </summary>
public abstract record ValueObject;
