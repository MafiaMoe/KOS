﻿namespace kOS.Safe.Exceptions
{
    /// <summary>
    /// A version of KOSCommandInvalidHere describing an attempt to use
    /// the RETURN command when not in the body of a user function.
    /// </summary>
    public class KOSReturnInvalidHereException : KOSCommandInvalidHereException
    {
        public override string HelpURL
        {
            get { return "http://ksp-kos.github.io/KOS_DOC/command/flowControl/index.html#RETURN"; }
        }

        public override string VerboseMessage { get { return VerbosePrefix + APPEND_TEXT; } }

        private const string APPEND_TEXT = "\n" +
            "Because RETURN causes the current user function to quit\n" +
            "it doesn't mean anything when it's not inside a\n" +
            "user function.\n";

        public KOSReturnInvalidHereException() :
            base("RETURN", "outside a FUNCTION", "in a FUNCTION body")
        {
        }
    }
}