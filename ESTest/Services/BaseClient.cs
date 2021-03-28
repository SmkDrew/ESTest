using ESTest.Controllers;
using ESTest.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ESTest.Services
{
    public class BaseClient
    {
        private readonly ElasticClient _client;
        public ElasticClient Client => _client;

        private string _user { get; set; }
        private string _password { get; set; }
        private string _baseUrl { get; set; }

        public BaseClient(string user, string password, string baseUrl)
        {
            _user = user;
            _password = password;
            _baseUrl = baseUrl;

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

            if (!_client.Indices.Exists(indName).Exists)
            {
                resp = _client.Indices.Create(indName, c => c.Map<T1>(m => m.AutoMap()).Map<T2>(m => m.AutoMap()));
            }

            return resp.IsValid;
        }

        /// <summary>
        /// Upload docs into ES
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UploadDocs<T>(string indName, IEnumerable<T> items) where T: class
        {
            var resp = await _client.BulkAsync(b => b.Index(indName).IndexMany(items));
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
                //_logger.LogInformation($"Well data requested. Well Uid:{uid}");
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, $"Failed to get well data. Well Uid:{uid}");
            }

            return result;
        }

        /// <summary>
        /// Delete index by name
        /// </summary>
        public async Task<bool> DeleteIndex(string indName)
        {
            DeleteIndexResponse resp = null;

            if (_client.Indices.Exists(indName).Exists)
            {
                resp = await _client.Indices.DeleteAsync(indName);
            }
            else
            {
                return false;
            }

            return resp.IsValid;
        }
    }
}
