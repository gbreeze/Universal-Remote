using System.Collections.Generic;
using System.Web.Http;

namespace RemoteService.ApiControllers
{
    public class MediaController : ApiController
    {
        // GET http://localhost:7055/api/v1/media/demo
        public string Get(string cmd)
        {
            return cmd;
        }

        // POST http://localhost:8888/api/v1/media/play
        public void Post(string cmd, [FromBody]string value)
        {
            var ms = new MediaService();
            ms.Send(cmd.Trim());
        }

        // http://localhost:7055/api/v1/media/test
        [Route("test")]
        [HttpGet]
        public string Test()
        {
            return "OK";
        }
    }
}
