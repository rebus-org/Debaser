// This class has been moved to Debaser.Core.Internals.Exceptions
// This file provides backward compatibility

namespace Debaser.Internals.Exceptions;

/// <summary>
/// Special exception used internally when upserting and it turns out the sequence was empty
/// </summary>
internal class EmptySequenceException : Core.Internals.Exceptions.EmptySequenceException;