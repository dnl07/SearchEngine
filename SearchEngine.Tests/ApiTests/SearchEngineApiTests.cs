using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using SearchEngine.Api.Dto.Documents;
using SearchEngine.Api.Dto.Indexes;
using SearchEngine.Api.Dto.Search;

namespace SearchEngine.Tests.ApiTests
{
    public class SearchEngineApiTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private static readonly object Settings = new
        {
            fields = new[]
            {
                new { name = "category", weight = 3.0 },
                new { name = "color", weight = 2.0 },
            },
            stopwords = new { languages = new[] { "de", "en" } },
        };

        private readonly HttpClient _client;

        public SearchEngineApiTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private static CancellationToken Ct => TestContext.Current.CancellationToken;

        [Fact]
        public async Task Index_FullLifecycle_Works()
        {
            var created = await _client.PutAsJsonAsync("indexes/lifecycle", Settings, Ct);
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);

            var replaced = await _client.PutAsJsonAsync(
                "indexes/lifecycle/documents",
                new[] { Doc("1", "Coat", "Red"), Doc("2", "Pants", "Blue") },
                Ct
            );
            replaced.EnsureSuccessStatusCode();
            var written = await replaced.Content.ReadFromJsonAsync<DocumentsWrittenDto>(Ct);
            Assert.Equal(2, written!.DocumentCount);

            var upserted = await _client.PostAsJsonAsync(
                "indexes/lifecycle/documents",
                new[] { Doc("3", "Raincoat", "Brown") },
                Ct
            );
            upserted.EnsureSuccessStatusCode();

            Assert.Equal(new[] { "1", "3" }, await SearchIds("lifecycle", "coat"));

            var removed = await _client.DeleteAsync("indexes/lifecycle/documents/1", Ct);
            Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);
            Assert.Equal(new[] { "3" }, await SearchIds("lifecycle", "coat"));

            var info = await _client.GetFromJsonAsync<IndexInfoDto>("indexes/lifecycle", Ct);
            Assert.Equal(2, info!.DocumentCount);
            Assert.Equal(new[] { "category", "color" }, info.Settings.Fields.Select(f => f.Name));
            Assert.Equal(new[] { "de", "en" }, info.Settings.Stopwords.Languages);

            var deleted = await _client.DeleteAsync("indexes/lifecycle", Ct);
            Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
            var missing = await _client.GetAsync("indexes/lifecycle", Ct);
            Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        }

        [Fact]
        public async Task PutIndex_SameSettings_KeepsDocuments()
        {
            await _client.PutAsJsonAsync("indexes/unchanged", Settings, Ct);
            await _client.PutAsJsonAsync(
                "indexes/unchanged/documents",
                new[] { Doc("1", "Jacket", "Red") },
                Ct
            );

            var response = await _client.PutAsJsonAsync("indexes/unchanged", Settings, Ct);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var info = await response.Content.ReadFromJsonAsync<IndexInfoDto>(Ct);
            Assert.Equal(1, info!.DocumentCount);
        }

        [Fact]
        public async Task Search_WithAndWithoutLimit_ReturnsTotal()
        {
            await _client.PutAsJsonAsync("indexes/limits", Settings, Ct);
            var docs = Enumerable
                .Range(0, 12)
                .Select(i => Doc($"{i:D2}", "Jacket", "Red"))
                .ToArray();
            await _client.PutAsJsonAsync("indexes/limits/documents", docs, Ct);

            var limited = await Search("limits", new { query = "jacket", limit = 10 });
            var unlimited = await Search("limits", new { query = "jacket" });

            Assert.Equal(12, limited.Total);
            Assert.Equal(10, limited.Hits.Length);
            Assert.Equal(12, unlimited.Hits.Length);
        }

        [Fact]
        public async Task MultiSearch_ReturnsResultsGroupedByIndex()
        {
            await _client.PutAsJsonAsync("indexes/multi-a", Settings, Ct);
            await _client.PutAsJsonAsync("indexes/multi-b", Settings, Ct);
            await _client.PutAsJsonAsync(
                "indexes/multi-a/documents",
                new[] { Doc("a1", "Jacket", "Red") },
                Ct
            );
            await _client.PutAsJsonAsync(
                "indexes/multi-b/documents",
                new[] { Doc("b1", "Pants", "Red") },
                Ct
            );

            var response = await _client.PostAsJsonAsync(
                "multi-search",
                new
                {
                    queries = new[]
                    {
                        new { index = "multi-a", query = "red" },
                        new { index = "multi-b", query = "red" },
                    },
                },
                Ct
            );
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<MultiSearchResponseDto>(Ct);

            Assert.Collection(
                result!.Results,
                a => Assert.Equal(("multi-a", "a1"), (a.Index, Assert.Single(a.Hits).Id)),
                b => Assert.Equal(("multi-b", "b1"), (b.Index, Assert.Single(b.Hits).Id))
            );
        }

        [Fact]
        public async Task UnknownIndex_ReturnsNotFoundProblem()
        {
            var response = await _client.PostAsJsonAsync(
                "indexes/missing/search",
                new { query = "jacket" },
                Ct
            );

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(
                "application/problem+json",
                response.Content.Headers.ContentType?.MediaType
            );
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Ct);
            Assert.Equal("Index 'missing' does not exist.", problem!.Detail);
        }

        [Fact]
        public async Task UnknownField_ReturnsBadRequestProblem()
        {
            await _client.PutAsJsonAsync("indexes/fields", Settings, Ct);

            var response = await _client.PostAsJsonAsync(
                "indexes/fields/documents",
                new[] { new { id = "1", fields = new { title = new[] { "Jacket" } } } },
                Ct
            );

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(Ct);
            Assert.Equal("Document '1' contains the unknown field 'title'.", problem!.Detail);
        }

        [Fact]
        public async Task PutIndex_ReturnsLengthNormalizationOfEveryField()
        {
            var response = await _client.PutAsJsonAsync(
                "indexes/normalization",
                new
                {
                    fields = new object[]
                    {
                        new
                        {
                            name = "tags",
                            weight = 3.0,
                            lengthNormalization = 0.0,
                        },
                        new { name = "notes", weight = 0.5 },
                    },
                },
                Ct
            );

            var info = await response.Content.ReadFromJsonAsync<IndexInfoDto>(Ct);
            Assert.Equal(
                new double?[] { 0, 0.75 },
                info!.Settings.Fields.Select(field => field.LengthNormalization)
            );
        }

        [Fact]
        public async Task InvalidSettings_ReturnsBadRequest()
        {
            var response = await _client.PutAsJsonAsync(
                "indexes/invalid",
                new { fields = Array.Empty<object>() },
                Ct
            );

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Health_ReturnsOk()
        {
            var response = await _client.GetAsync("health", Ct);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        private static object Doc(string id, string category, string color) =>
            new
            {
                id,
                fields = new Dictionary<string, string[]>
                {
                    ["category"] = [category],
                    ["color"] = [color],
                },
            };

        private async Task<SearchResponseDto> Search(string index, object request)
        {
            var response = await _client.PostAsJsonAsync($"indexes/{index}/search", request, Ct);
            response.EnsureSuccessStatusCode();
            return (await response.Content.ReadFromJsonAsync<SearchResponseDto>(Ct))!;
        }

        private async Task<string[]> SearchIds(string index, string query) =>
            (await Search(index, new { query })).Hits.Select(hit => hit.Id).ToArray();
    }
}
