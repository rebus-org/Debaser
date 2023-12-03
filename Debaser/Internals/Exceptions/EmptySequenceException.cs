using System;
using System.Runtime.Serialization;

namespace Debaser.Internals.Exceptions;

[Serializable]
class EmptySequenceException : Exception
{
    public EmptySequenceException()
    {
    }

    public EmptySequenceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}