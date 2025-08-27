namespace ApiBase.Exceptions
{
    public class HttpFailureResponseException : Exception
    {

        public HttpFailureResponseException() { }
        public HttpFailureResponseException(string message) : base(message) { }

    }
}
