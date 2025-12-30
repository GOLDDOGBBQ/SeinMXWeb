using System;

namespace SEINMX.Clases.Generales;

public class ClApiResponseException : Exception
{
    public string Msg { get; }
    public string? MsgDev { get; }

    public ClApiResponseException(string message, string? messageDev = null) :
        base(message + (string.IsNullOrEmpty(messageDev) ? "" : "\n" + messageDev))
    {
        Msg = message;
        MsgDev = messageDev;
    }
}