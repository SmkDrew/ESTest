using ESTest.Model;
using ESTest.Model.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Nest;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ESTest.Services
{
    public class ESClient
    {
        private readonly BaseClient _client;

        private List<DocMgmt> _mgmtsList { get; set; }
        private List<DocProperty> _propList { get; set; }

        private string _mgmtsFilePath => "SourceFiles\\mgmt.json";
        private string _propertiesFilePath => "SourceFiles\\properties.json";
        private string _indName => "main";

        public ESClient(IConfiguration configuration, BaseClient client)
        {
            _client = client;
        }

        private void LoadSourceFiles()
        {
            var location = System.Reflection.Assembly.GetEntryAssembly().Location;
            var directory = Path.GetDirectoryName(location);

            var pathMgmt = Path.Combine(directory, _mgmtsFilePath);
            var pathProp = Path.Combine(directory, _propertiesFilePath);

            var strMgmt = string.Join("", File.ReadAllLines(pathMgmt));
            var strProp = string.Join("", File.ReadAllLines(pathProp));

            _mgmtsList = JsonConvert.DeserializeObject<List<DocMgmt>>(strMgmt);
            _propList = JsonConvert.DeserializeObject<List<DocProperty>>(strProp);
        }

        /// <summary>
        /// Upload source files tino ES
        /// </summary>
        /// <returns>Result</returns>
        public async Task<bool> UploadSourceFiles()
        {
            bool resPropUpld = false, resMgmtUpld = false;

            LoadSourceFiles();

            var resInd = _client.CreateIndex<Property, Mgmt>(_indName);
            if (resInd)
            {
                resPropUpld = await _client.UploadDocs(_indName, _propList.Select(m => m.property));
                resMgmtUpld = await _client.UploadDocs(_indName, _mgmtsList.Select(m => m.mgmt));
            }

            return resPropUpld && resMgmtUpld;
        }

        /// <summary>
        /// Show indexes
        /// </summary>
        /// <returns>List of indexes</returns>
        public async Task<List<string>> GetAllIndexes()
        {
            var indexes = await _client.GetIndexes();
            return indexes.Indices.Select(i => i.Key.Name).ToList();            
        }

        /// <summary>
        /// Delete index by name
        /// </summary>
        /// <param name="indName">Index name</param>
        /// <returns>Result</returns>
        public async Task<bool> DeleteIndex(string indName)
        {
            var res = await _client.DeleteIndex(indName);
            return res;
        }

        /// <summary>
        /// Query to the Prop and Mgmt documents
        /// </summary>
        public async Task<string> FindData(string qry, string market, int limit)
        {
            var propRes = await QueryProperties(qry, market, limit);
            var mgmtRes = await QueryCompanies(qry, market, limit);

            return $"{propRes}\r\n{mgmtRes}";
        }

        /// <summary>
        /// Get property documents
        /// </summary>
        private async Task<string> QueryProperties(string qry, string market, int limit)
        {
            var res = await _client.Client.SearchAsync<Property>(s =>
                            s.Index(_indName)
                            .Size(limit)
                            .Query(q => q
                                .MultiMatch(m => m
                                    .Fields(f => f
                                        .Field(f => f.city)
                                        .Field(f => f.state)
                                        .Field(f => f.market)
                                        .Field(f => f.streetAddress)
                                        .Field(f => f.name)
                                        .Field(f => f.formerName)
                                    )
                                    .Operator(Operator.Or)
                                    .Query($"{qry}")
                                ) && q.Match(m => m.Field(f => f.market).Query(market))
                            )
                        );

            var props = res.Documents.ToList();
            var strRes = JsonConvert.SerializeObject(props, Formatting.Indented);

            return strRes;
        }

        /// <summary>
        /// Get property documents
        /// </summary>
        private async Task<string> QueryCompanies(string qry, string market, int limit)
        {
            var res = await _client.Client.SearchAsync<Mgmt>(s =>
                            s.Index(_indName)
                            .Size(limit)
                            .Query(q => q
                                .MultiMatch(m => m
                                    .Fields(f => f
                                        .Field(f => f.state)
                                        .Field(f => f.market)
                                        .Field(f => f.name)
                                    )
                                    .Operator(Operator.Or)
                                    .Query($"{qry}")
                                ) && q.Match(m => m.Field(f => f.market).Query(market))
                            )
                        );

            var props = res.Documents.ToList();
            var strRes = JsonConvert.SerializeObject(props, Formatting.Indented);

            return strRes;
        }

    }
}
