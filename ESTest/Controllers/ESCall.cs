using ESTest.Model;
using ESTest.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Threading.Tasks;

namespace ESTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ESCall: ControllerBase
    {
        private readonly ILogger<ESCall> _logger;
        private readonly ESClient _client;
        private readonly ESConnection _settings;

        public ESCall(ILogger<ESCall> logger, IConfiguration config, ESClient client)
        {
            _logger = logger;
            _client = client;
            _settings = config.GetSection("AppSettings:ESConnection").Get<ESConnection>();
        }

        [HttpGet]
        [Route("ping")]
        public string Ping()
        {
            return "Service is alive";
        }

        [HttpGet]
        [Route("upload")]
        public async Task<string> UploadSources()
        {
            var res = await _client.UploadSourceFiles();
            return res ? "Source files have been uploaded successfully": "Error while uploading files";
        }

        [HttpGet]
        [Route("indexes")]
        public async Task<string> GetIndexes()
        {
            var indexes = await _client.GetAllIndexes();
            return JsonSerializer.Serialize(indexes);
        }

        [HttpGet]
        [Route("delind/{indName}")]
        public async Task<string> DeleteIndex(string indName)
        {
            if (string.IsNullOrEmpty(indName))
            {
                return "Please specify the index name";
            }

            var res = await _client.DeleteIndex(indName);
            return res ? "Index has been deleted successfully": "Error while deleting index";
        }

        [HttpGet]
        [Route("docs")]
        public async Task<string> FindData([FromQuery] int limit, [FromQuery] string qry, [FromQuery] string market = null)
        {
            if (string.IsNullOrEmpty(qry))
            {
                return "Wrong request. The qry parameter is required";
            }

            if (limit == 0)
            {
                limit = _settings.Limit;
            }

            market = market.Replace(",", " ");

            var res = await _client.FindData(qry, market, limit);
            return res;
        }

    }
}
