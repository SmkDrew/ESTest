using ESTest.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ESTest.Services
{
    public class BaseClient
    {
        private readonly ElasticClient _client;
        public ElasticClient Client => _client;
        private readonly ILogger<BaseClient> _logger;


        private string _user { get; set; }
        private string _password { get; set; }
        private string _baseUrl { get; set; }

        public BaseClient(ILogger<BaseClient> logger, IConfiguration config)
        {
            var esSettings = config.GetSection("AppSettings:ESConnection").Get<ESConnection>();
            _user = esSettings.User;
            _password = esSettings.Password;
            _baseUrl = esSettings.Url;
            _logger = logger;

            var node = new Uri(_baseUrl);
            var settings = new ConnectionSettings(node).BasicAuthentication(_user, _password);
            settings.DisableDirectStreaming();

            _client = new ElasticClient(settings);
        }

        /// <summary>
        /// Create a new index in ES
        /// </summary>
        public bool CreateIndex<T1, T2>(string indName) where T1: class where T2: class
        {
            CreateIndexResponse resp = null;

            try
            {
                if (!_client.Indices.Exists(indName).Exists)
                {
                    resp = _client.Indices.Create(indName, c => c.Map<T1>(m => m.AutoMap()).Map<T2>(m => m.AutoMap()));
                }

                _logger.LogInformation($"Index {indName} has been created");
            }
            catch (Exception)
            {
                _logger.LogInformation($"An error occurred while creating the index {indName}");
                return false;
            }


            return resp.IsValid;
        }

        /// <summary>
        /// Upload docs into ES
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UploadDocs<T>(string indName, IEnumerable<T> items) where T: class
        {
            BulkResponse resp = null;

            try
            {
                resp = await _client.BulkAsync(b => b.Index(indName).IndexMany(items));
                _logger.LogInformation($"Documents have been uploaded into index {indName}");
            }
            catch (Exception)
            {
                _logger.LogInformation($"An error occurred while uploading documents");
                return false;
            }
            
            return resp.IsValid;
        }

        /// <summary>
        /// Get all indexes
        /// </summary>
        public async Task<GetIndexResponse> GetIndexes()
        {
            GetIndexResponse result = null;

            try
            {
                result = await _client.Indices.GetAsync(new GetIndexRequest(Indices.All));                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to get indexes");
            }

            return result;
        }

        /// <summary>
        /// Delete index by name
        /// </summary>
        public async Task<bool> DeleteIndex(string indName)
        {
            DeleteIndexResponse resp = null;

            try
            {
                if (_client.Indices.Exists(indName).Exists)
                {
                    resp = await _client.Indices.DeleteAsync(indName);
                }
                else
                {
                    return false;
                }

                _logger.LogInformation($"Index {indName} has been deleted");
            }
            catch (Exception)
            {
                _logger.LogError($"Failed to delete index {indName}");
                return false;
            }

            return resp.IsValid;
        }
    }
}
