using System;

namespace AI
{
    /// <summary>
    /// Raised when the client cannot reach the LLM service.
    /// </summary>
    public class NoInternetException : Exception
    {
        public NoInternetException(string message, Exception inner)
            : base(message, inner) { }
    }

    /// <summary>
    /// Raised when billing or quota prevents further requests.
    /// </summary>
    public class NoTokensException : Exception { }

    /// <summary>
    /// Raised when the provided API key is invalid.
    /// </summary>
    public class InvalidApiKeyException : Exception { }

    /// <summary>
    /// Raised when too many requests are sent to the service.
    /// </summary>
    public class RateLimitException : Exception { }

    /// <summary>
    /// General-purpose API failure wrapper.
    /// </summary>
    public class ApiException : Exception
    {
        public ApiException(string message) : base(message) { }
    }
}
