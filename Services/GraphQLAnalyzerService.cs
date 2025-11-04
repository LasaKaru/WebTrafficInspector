using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    /// <summary>
    /// Service for analyzing, testing, and exploiting GraphQL endpoints
    /// </summary>
    public class GraphQLAnalyzerService
    {
        private List<GraphQLQuery> _queries = new List<GraphQLQuery>();
        private List<GraphQLMutation> _mutations = new List<GraphQLMutation>();
        private GraphQLSchema _schema;

        public GraphQLAnalyzerService()
        {
        }

        /// <summary>
        /// Detect if a traffic entry is a GraphQL request
        /// </summary>
        public bool IsGraphQLRequest(TrafficEntry entry)
        {
            if (entry.Method != "POST") return false;

            // Check URL patterns
            if (entry.Path != null && (
                entry.Path.Contains("/graphql") ||
                entry.Path.Contains("/api/graphql") ||
                entry.Path.Contains("/graph")))
            {
                return true;
            }

            // Check request body for GraphQL structure
            if (!string.IsNullOrEmpty(entry.RequestBody))
            {
                try
                {
                    var json = JsonDocument.Parse(entry.RequestBody);
                    if (json.RootElement.TryGetProperty("query", out _) ||
                        json.RootElement.TryGetProperty("mutation", out _) ||
                        json.RootElement.TryGetProperty("operationName", out _))
                    {
                        return true;
                    }
                }
                catch { }

                // Check for GraphQL query syntax
                if (Regex.IsMatch(entry.RequestBody, @"query\s+\w+\s*\{") ||
                    Regex.IsMatch(entry.RequestBody, @"mutation\s+\w+\s*\{") ||
                    Regex.IsMatch(entry.RequestBody, @"subscription\s+\w+\s*\{"))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Extract GraphQL schema via introspection query
        /// </summary>
        public async Task<GraphQLSchema> DiscoverSchema(string url)
        {
            var introspectionQuery = @"
            {
              __schema {
                queryType { name }
                mutationType { name }
                subscriptionType { name }
                types {
                  name
                  kind
                  description
                  fields {
                    name
                    description
                    args {
                      name
                      type { name kind ofType { name kind } }
                    }
                    type { name kind ofType { name kind } }
                  }
                }
                directives {
                  name
                  description
                  locations
                  args {
                    name
                    type { name kind }
                  }
                }
              }
            }";

            try
            {
                using var client = new System.Net.Http.HttpClient();
                var requestBody = new
                {
                    query = introspectionQuery
                };

                var content = new System.Net.Http.StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                var schema = ParseSchemaFromIntrospection(responseBody);
                _schema = schema;

                return schema;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to discover GraphQL schema: {ex.Message}", ex);
            }
        }

        private GraphQLSchema ParseSchemaFromIntrospection(string responseBody)
        {
            var schema = new GraphQLSchema
            {
                Types = new List<GraphQLType>(),
                Queries = new List<string>(),
                Mutations = new List<string>(),
                Subscriptions = new List<string>()
            };

            try
            {
                var json = JsonDocument.Parse(responseBody);
                var schemaData = json.RootElement.GetProperty("data").GetProperty("__schema");

                // Extract query type
                if (schemaData.TryGetProperty("queryType", out var queryType) &&
                    queryType.TryGetProperty("name", out var queryTypeName))
                {
                    schema.QueryTypeName = queryTypeName.GetString();
                }

                // Extract mutation type
                if (schemaData.TryGetProperty("mutationType", out var mutationType) &&
                    mutationType.TryGetProperty("name", out var mutationTypeName))
                {
                    schema.MutationTypeName = mutationTypeName.GetString();
                }

                // Extract types
                if (schemaData.TryGetProperty("types", out var types))
                {
                    foreach (var type in types.EnumerateArray())
                    {
                        var graphQLType = new GraphQLType
                        {
                            Name = type.TryGetProperty("name", out var name) ? name.GetString() : null,
                            Kind = type.TryGetProperty("kind", out var kind) ? kind.GetString() : null,
                            Description = type.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                            Fields = new List<GraphQLField>()
                        };

                        if (type.TryGetProperty("fields", out var fields) && fields.ValueKind != JsonValueKind.Null)
                        {
                            foreach (var field in fields.EnumerateArray())
                            {
                                var graphQLField = new GraphQLField
                                {
                                    Name = field.TryGetProperty("name", out var fieldName) ? fieldName.GetString() : null,
                                    Description = field.TryGetProperty("description", out var fieldDesc) ? fieldDesc.GetString() : null,
                                    Arguments = new List<GraphQLArgument>()
                                };

                                if (field.TryGetProperty("args", out var args))
                                {
                                    foreach (var arg in args.EnumerateArray())
                                    {
                                        graphQLField.Arguments.Add(new GraphQLArgument
                                        {
                                            Name = arg.TryGetProperty("name", out var argName) ? argName.GetString() : null
                                        });
                                    }
                                }

                                graphQLType.Fields.Add(graphQLField);

                                // Categorize queries and mutations
                                if (graphQLType.Name == schema.QueryTypeName)
                                {
                                    schema.Queries.Add(graphQLField.Name);
                                }
                                else if (graphQLType.Name == schema.MutationTypeName)
                                {
                                    schema.Mutations.Add(graphQLField.Name);
                                }
                            }
                        }

                        schema.Types.Add(graphQLType);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse schema: {ex.Message}", ex);
            }

            return schema;
        }

        /// <summary>
        /// Analyze GraphQL traffic entry
        /// </summary>
        public GraphQLAnalysisResult AnalyzeEntry(TrafficEntry entry)
        {
            var result = new GraphQLAnalysisResult
            {
                IsGraphQL = IsGraphQLRequest(entry),
                Url = entry.Url,
                Issues = new List<GraphQLIssue>()
            };

            if (!result.IsGraphQL) return result;

            try
            {
                // Parse the GraphQL query
                var queryData = ExtractQueryData(entry.RequestBody);
                result.Query = queryData.Query;
                result.Variables = queryData.Variables;
                result.OperationName = queryData.OperationName;

                // Check for common security issues
                CheckIntrospectionEnabled(result, queryData.Query);
                CheckDepthComplexity(result, queryData.Query);
                CheckBatchingAbuse(result, entry.RequestBody);
                CheckSensitiveDataExposure(result, entry.ResponseBody);
                CheckAuthorizationBypass(result, queryData.Query);
                CheckInjectionVulnerabilities(result, queryData);
                CheckFieldSuggestions(result, entry.ResponseBody);
                CheckRateLimiting(result, entry);
            }
            catch (Exception ex)
            {
                result.Issues.Add(new GraphQLIssue
                {
                    Severity = "Info",
                    Type = "Parsing Error",
                    Description = $"Failed to analyze GraphQL request: {ex.Message}"
                });
            }

            return result;
        }

        private (string Query, Dictionary<string, object> Variables, string OperationName) ExtractQueryData(string requestBody)
        {
            try
            {
                var json = JsonDocument.Parse(requestBody);
                var root = json.RootElement;

                var query = root.TryGetProperty("query", out var q) ? q.GetString() : null;
                var operationName = root.TryGetProperty("operationName", out var op) ? op.GetString() : null;
                var variables = new Dictionary<string, object>();

                if (root.TryGetProperty("variables", out var vars) && vars.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in vars.EnumerateObject())
                    {
                        variables[property.Name] = property.Value.ToString();
                    }
                }

                return (query, variables, operationName);
            }
            catch
            {
                // Try to extract query directly if not JSON
                var query = requestBody;
                return (query, new Dictionary<string, object>(), null);
            }
        }

        private void CheckIntrospectionEnabled(GraphQLAnalysisResult result, string query)
        {
            if (query != null && query.Contains("__schema"))
            {
                result.Issues.Add(new GraphQLIssue
                {
                    Severity = "Medium",
                    Type = "Introspection Enabled",
                    Description = "GraphQL introspection is enabled, allowing attackers to discover the entire schema"
                });
            }
        }

        private void CheckDepthComplexity(GraphQLAnalysisResult result, string query)
        {
            if (query == null) return;

            var depth = CalculateQueryDepth(query);
            if (depth > 10)
            {
                result.Issues.Add(new GraphQLIssue
                {
                    Severity = "High",
                    Type = "Deep Query",
                    Description = $"Query depth is {depth}, which may cause DoS or performance issues"
                });
            }

            var complexity = CalculateQueryComplexity(query);
            if (complexity > 100)
            {
                result.Issues.Add(new GraphQLIssue
                {
                    Severity = "High",
                    Type = "Complex Query",
                    Description = $"Query complexity is {complexity}, which may cause DoS or performance issues"
                });
            }
        }

        private int CalculateQueryDepth(string query)
        {
            int maxDepth = 0;
            int currentDepth = 0;

            foreach (char c in query)
            {
                if (c == '{')
                {
                    currentDepth++;
                    if (currentDepth > maxDepth)
                        maxDepth = currentDepth;
                }
                else if (c == '}')
                {
                    currentDepth--;
                }
            }

            return maxDepth;
        }

        private int CalculateQueryComplexity(string query)
        {
            // Simple complexity calculation based on field count
            var fieldCount = Regex.Matches(query, @"\w+\s*\{|\w+\s*\(").Count;
            return fieldCount;
        }

        private void CheckBatchingAbuse(GraphQLAnalysisResult result, string requestBody)
        {
            try
            {
                var json = JsonDocument.Parse(requestBody);
                if (json.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var batchSize = json.RootElement.GetArrayLength();
                    if (batchSize > 10)
                    {
                        result.Issues.Add(new GraphQLIssue
                        {
                            Severity = "Medium",
                            Type = "Batch Query Abuse",
                            Description = $"Batch query contains {batchSize} operations, which may bypass rate limiting"
                        });
                    }
                }
            }
            catch { }
        }

        private void CheckSensitiveDataExposure(GraphQLAnalysisResult result, string responseBody)
        {
            if (string.IsNullOrEmpty(responseBody)) return;

            var sensitivePatterns = new Dictionary<string, string>
            {
                { @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", "Email addresses" },
                { @"\b\d{3}[-.]?\d{2}[-.]?\d{4}\b", "SSN numbers" },
                { @"\b\d{4}[-\s]?\d{4}[-\s]?\d{4}[-\s]?\d{4}\b", "Credit card numbers" },
                { @"password[\""]?\s*:\s*[\""]([^\""\s]+)", "Passwords" },
                { @"api[_-]?key[\""]?\s*:\s*[\""]([^\""\s]+)", "API keys" },
                { @"token[\""]?\s*:\s*[\""]([^\""\s]+)", "Tokens" }
            };

            foreach (var pattern in sensitivePatterns)
            {
                if (Regex.IsMatch(responseBody, pattern.Key, RegexOptions.IgnoreCase))
                {
                    result.Issues.Add(new GraphQLIssue
                    {
                        Severity = "Critical",
                        Type = "Sensitive Data Exposure",
                        Description = $"{pattern.Value} found in GraphQL response"
                    });
                }
            }
        }

        private void CheckAuthorizationBypass(GraphQLAnalysisResult result, string query)
        {
            if (query == null) return;

            // Check for common fields that might bypass authorization
            var sensitiveFields = new[] { "admin", "user", "password", "secret", "private", "internal" };
            foreach (var field in sensitiveFields)
            {
                if (Regex.IsMatch(query, $@"\b{field}\b", RegexOptions.IgnoreCase))
                {
                    result.Issues.Add(new GraphQLIssue
                    {
                        Severity = "High",
                        Type = "Potential Authorization Bypass",
                        Description = $"Query accesses potentially sensitive field: {field}"
                    });
                }
            }
        }

        private void CheckInjectionVulnerabilities(GraphQLAnalysisResult result, (string Query, Dictionary<string, object> Variables, string OperationName) queryData)
        {
            if (queryData.Variables == null) return;

            var injectionPatterns = new[]
            {
                @"[';]--",
                @"union\s+select",
                @"<script",
                @"javascript:",
                @"\$\{",
                @"`.*`"
            };

            foreach (var variable in queryData.Variables)
            {
                var value = variable.Value?.ToString() ?? "";
                foreach (var pattern in injectionPatterns)
                {
                    if (Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase))
                    {
                        result.Issues.Add(new GraphQLIssue
                        {
                            Severity = "High",
                            Type = "Potential Injection",
                            Description = $"Variable '{variable.Key}' contains potential injection pattern: {pattern}"
                        });
                    }
                }
            }
        }

        private void CheckFieldSuggestions(GraphQLAnalysisResult result, string responseBody)
        {
            if (string.IsNullOrEmpty(responseBody)) return;

            if (responseBody.Contains("\"suggestions\"") || responseBody.Contains("Did you mean"))
            {
                result.Issues.Add(new GraphQLIssue
                {
                    Severity = "Low",
                    Type = "Field Suggestions Enabled",
                    Description = "GraphQL provides field suggestions, which can help attackers enumerate valid fields"
                });
            }
        }

        private void CheckRateLimiting(GraphQLAnalysisResult result, TrafficEntry entry)
        {
            // This would need to track multiple requests to detect rate limiting
            // For now, just check response headers
            var headers = entry.ResponseHeaders?.ToLower() ?? "";
            if (!headers.Contains("x-ratelimit") && !headers.Contains("x-rate-limit"))
            {
                result.Issues.Add(new GraphQLIssue
                {
                    Severity = "Medium",
                    Type = "No Rate Limiting",
                    Description = "GraphQL endpoint does not appear to implement rate limiting"
                });
            }
        }

        /// <summary>
        /// Generate attack queries for testing
        /// </summary>
        public List<GraphQLAttackQuery> GenerateAttackQueries(GraphQLSchema schema)
        {
            var attacks = new List<GraphQLAttackQuery>();

            // Introspection attack
            attacks.Add(new GraphQLAttackQuery
            {
                Name = "Full Schema Introspection",
                Type = "Introspection",
                Query = GenerateIntrospectionQuery(),
                Description = "Attempts to extract the complete GraphQL schema"
            });

            // Deep recursion attack
            attacks.Add(new GraphQLAttackQuery
            {
                Name = "Deep Recursion DoS",
                Type = "DoS",
                Query = GenerateDeepRecursionQuery(20),
                Description = "Tests for DoS via deeply nested queries"
            });

            // Batch attack
            attacks.Add(new GraphQLAttackQuery
            {
                Name = "Batch Query DoS",
                Type = "DoS",
                Query = GenerateBatchQuery(100),
                Description = "Tests for DoS via batch query abuse"
            });

            // Field enumeration
            if (schema != null && schema.Queries.Any())
            {
                foreach (var query in schema.Queries.Take(10))
                {
                    attacks.Add(new GraphQLAttackQuery
                    {
                        Name = $"Enumerate {query} fields",
                        Type = "Enumeration",
                        Query = GenerateFieldEnumerationQuery(query),
                        Description = $"Attempts to enumerate all fields of {query}"
                    });
                }
            }

            return attacks;
        }

        private string GenerateIntrospectionQuery()
        {
            return @"{""query"":""{ __schema { queryType { name } mutationType { name } types { name kind fields { name } } } }""}";
        }

        private string GenerateDeepRecursionQuery(int depth)
        {
            var sb = new StringBuilder();
            sb.Append(@"{""query"":""{ user { ");
            for (int i = 0; i < depth; i++)
            {
                sb.Append("friends { ");
            }
            sb.Append("id");
            for (int i = 0; i < depth; i++)
            {
                sb.Append(" }");
            }
            sb.Append(" } }\"}");
            return sb.ToString();
        }

        private string GenerateBatchQuery(int count)
        {
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append(@"{""query"":""{ __typename }""}");
            }
            sb.Append("]");
            return sb.ToString();
        }

        private string GenerateFieldEnumerationQuery(string queryName)
        {
            return $@"{{""query"":""{{ {queryName} {{ __typename }} }}""}}";
        }

        public string GenerateReport(GraphQLAnalysisResult analysis)
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine("          GraphQL Security Analysis Report");
            sb.AppendLine("═══════════════════════════════════════════════════════════");
            sb.AppendLine($"URL: {analysis.Url}");
            sb.AppendLine($"Is GraphQL: {analysis.IsGraphQL}");
            sb.AppendLine();

            if (!string.IsNullOrEmpty(analysis.OperationName))
            {
                sb.AppendLine($"Operation: {analysis.OperationName}");
            }

            if (analysis.Issues.Any())
            {
                sb.AppendLine("SECURITY ISSUES:");
                sb.AppendLine("─────────────────────────────────────────────────────────");

                var critical = analysis.Issues.Where(i => i.Severity == "Critical").ToList();
                var high = analysis.Issues.Where(i => i.Severity == "High").ToList();
                var medium = analysis.Issues.Where(i => i.Severity == "Medium").ToList();
                var low = analysis.Issues.Where(i => i.Severity == "Low").ToList();

                if (critical.Any())
                {
                    sb.AppendLine($"\nCRITICAL ({critical.Count}):");
                    foreach (var issue in critical)
                    {
                        sb.AppendLine($"  • {issue.Type}: {issue.Description}");
                    }
                }

                if (high.Any())
                {
                    sb.AppendLine($"\nHIGH ({high.Count}):");
                    foreach (var issue in high)
                    {
                        sb.AppendLine($"  • {issue.Type}: {issue.Description}");
                    }
                }

                if (medium.Any())
                {
                    sb.AppendLine($"\nMEDIUM ({medium.Count}):");
                    foreach (var issue in medium)
                    {
                        sb.AppendLine($"  • {issue.Type}: {issue.Description}");
                    }
                }

                if (low.Any())
                {
                    sb.AppendLine($"\nLOW ({low.Count}):");
                    foreach (var issue in low)
                    {
                        sb.AppendLine($"  • {issue.Type}: {issue.Description}");
                    }
                }
            }
            else
            {
                sb.AppendLine("No security issues detected.");
            }

            sb.AppendLine("\n═══════════════════════════════════════════════════════════");

            return sb.ToString();
        }
    }

    #region Models

    public class GraphQLSchema
    {
        public string QueryTypeName { get; set; }
        public string MutationTypeName { get; set; }
        public string SubscriptionTypeName { get; set; }
        public List<GraphQLType> Types { get; set; }
        public List<string> Queries { get; set; }
        public List<string> Mutations { get; set; }
        public List<string> Subscriptions { get; set; }
    }

    public class GraphQLType
    {
        public string Name { get; set; }
        public string Kind { get; set; }
        public string Description { get; set; }
        public List<GraphQLField> Fields { get; set; }
    }

    public class GraphQLField
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<GraphQLArgument> Arguments { get; set; }
    }

    public class GraphQLArgument
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class GraphQLQuery
    {
        public string Name { get; set; }
        public string Query { get; set; }
        public Dictionary<string, object> Variables { get; set; }
    }

    public class GraphQLMutation
    {
        public string Name { get; set; }
        public string Mutation { get; set; }
        public Dictionary<string, object> Variables { get; set; }
    }

    public class GraphQLAnalysisResult
    {
        public bool IsGraphQL { get; set; }
        public string Url { get; set; }
        public string Query { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public string OperationName { get; set; }
        public List<GraphQLIssue> Issues { get; set; }
    }

    public class GraphQLIssue
    {
        public string Severity { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }

    public class GraphQLAttackQuery
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Query { get; set; }
        public string Description { get; set; }
    }

    #endregion
}
