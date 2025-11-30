using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Numerics.Tensors;
using Dapper;
using Npgsql;
using RiverAPI.Domain;
using RiverAPI.Responses;

namespace RiverAPI.Services
{
    public class SemanticRiverService
    {
        private readonly NpgsqlDataSource _dataSource;

        // Key is String (River ID), Value is the Vector
        private Dictionary<string, double[]> _vectorCache = new();
        private bool _isCacheLoaded = false;

        // Static HttpClient is best practice for reuse across requests
        private static readonly HttpClient _httpClient = new HttpClient();

        public SemanticRiverService(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource;
        }

        public async Task LoadVectorsAsync()
        {
            if (_isCacheLoaded) return;

            await using var conn = await _dataSource.OpenConnectionAsync();

            var sql = "SELECT river_id, embedding FROM river_semantic_index";
            var rows = await conn.QueryAsync(sql);

            foreach (var row in rows)
            {
                if (row.embedding != null)
                {
                    string rId = (string)row.river_id;
                    _vectorCache[rId] = (double[])row.embedding;
                }
            }

            _isCacheLoaded = true;
            Console.WriteLine($"[SemanticService] Cached {_vectorCache.Count} river vectors in memory.");
        }

        public async Task<IEnumerable<SearchResponse>> SearchAsync(string query)
        {
            await LoadVectorsAsync();

            // Normalize input to handle plural/singular variations better
            var normalizedQuery = query.ToLower().Trim();

            var queryVector = await GetQueryEmbeddingFromPython(normalizedQuery);
            if (queryVector == null) return Enumerable.Empty<SearchResponse>();

            var matches = new List<(string Id, double Score)>();

            foreach (var item in _vectorCache)
            {
                double similarity = TensorPrimitives.CosineSimilarity(
                    new ReadOnlySpan<double>(queryVector),
                    new ReadOnlySpan<double>(item.Value)
                );

                // Slight threshold increase to reduce noise from common words like "River"
                if (similarity > 0.55)
                {
                    matches.Add((item.Key, similarity));
                }
            }

            if (!matches.Any()) return Enumerable.Empty<SearchResponse>();

            var topMatches = matches
                .OrderByDescending(x => x.Score)
                .ToList();

            var ids = topMatches.Select(x => x.Id).ToList();

            await using var conn = await _dataSource.OpenConnectionAsync();

            var sql = @"SELECT id, flow_direction, length, fictitious::boolean, form, 
                       watercourse_name, watercourse_name_alternative, 
                       start_node, end_node, 
                       ST_Transform(ST_SetSRID(geometry, 27700), 4326) as geometry
                FROM rivers 
                WHERE id = ANY(@ids)";

            var rivers = await conn.QueryAsync<River>(sql, new { ids });

            var results = new List<SearchResponse>();

            foreach (var match in topMatches)
            {
                var river = rivers.FirstOrDefault(r => r.Id == match.Id);
                if (river != null)
                {
                    results.Add(new SearchResponse
                    {
                        SimilarityScore = match.Score,
                        River = river
                    });
                }
            }

            return results;
        }

        private async Task<double[]?> GetQueryEmbeddingFromPython(string text)
        {
            try
            {
                // Communication with the Python Microservice (api.py)
                var url = "http://localhost:5000/embed";

                var payload = new { text = text };
                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send Request
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode) return null;

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<double[]>(jsonResponse);
            }
            catch
            {
                // Fail gracefully if Python API is down
                return null;
            }
        }
    }
}