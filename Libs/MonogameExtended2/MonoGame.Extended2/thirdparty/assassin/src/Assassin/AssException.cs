using System;

namespace Assassin;

public class AssException : ApplicationException
{

    public AssException()
    {
    }

    public AssException(string message)
        : base(message)
    {
    }

    public AssException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

}
