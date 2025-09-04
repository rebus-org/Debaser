namespace Debaser.Core.Internals.Exceptions;

/// <summary>
/// Special exception used internally when upserting and it turns out the sequence was empty
/// </summary>
public class EmptySequenceException : Exception;