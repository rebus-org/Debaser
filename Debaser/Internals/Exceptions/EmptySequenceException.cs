using System;

namespace Debaser.Internals.Exceptions;

/// <summary>
/// Special exception used internally when upserting and it turns out the sequence was empty
/// </summary>
class EmptySequenceException : Exception;