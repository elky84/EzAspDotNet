using System.Net;

namespace EzAspDotNet.Exception
{
    public class DeveloperException : System.Exception
    {
        public Protocols.Code.ResultCode ResultCode { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }

        public string Detail { get; set; }

        public DeveloperException(Protocols.Code.ResultCode resultCode, 
            HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError, 
            string detail = null)
        {
            ResultCode = resultCode;
            HttpStatusCode = httpStatusCode;
            Detail = detail;
        }
    }
}
