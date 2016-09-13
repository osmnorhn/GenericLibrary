using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Http;
using System.Web.Http.Results;
namespace Generic.RESTful
{
    internal static class ResultConst
    {
        internal static readonly JsonSerializerSettings DefaultJsonSerializerSettings = new JsonSerializerSettings();
        internal static readonly UTF8Encoding DefaultUTF8Encoding = new UTF8Encoding(false, true);
    }

    public class Result<T> : JsonResult<Response<T>>
    {
        public static Result<T> CreateInvalidParameters(ApiController controller)
        {
            return new Result<T>(Response<T>.InvalidParameters, controller);
        }

        public Result(Response<T> data, ApiController controller)
            : base(data, ResultConst.DefaultJsonSerializerSettings, ResultConst.DefaultUTF8Encoding, controller)
        {
        }
    }


    public class Response<T>
    {
        public static readonly Response<T> InvalidParameters = new Response<T>() { Error = new Exception("Invalid Parameters") };

        public IEnumerable<T> Data { get; set; }
        public int Total { get; set; }
        public Exception Error { get; set; } //System.Exceptionda Desirialize edilebilir.
    }


    public class ImageResult : IHttpActionResult
    {
        private readonly byte[] image;

        public ImageResult(byte[] image)
        {
            if (null == image)
                throw new ArgumentNullException(nameof(image));

            this.image = image;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                HttpResponseMessage httpResponseMessage = new HttpResponseMessage();

                httpResponseMessage.Content = new ByteArrayContent(this.image);

                httpResponseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                httpResponseMessage.StatusCode = HttpStatusCode.OK;

                return httpResponseMessage;
            }, cancellationToken);
        }
    }
}
