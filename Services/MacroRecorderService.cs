using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebTrafficInspector.Models;

namespace WebTrafficInspector.Services
{
    public class MacroRecorderService
    {
        private List<Macro> _macros = new List<Macro>();
        private Macro _currentRecording = null;
        private bool _isRecording = false;

        public bool IsRecording => _isRecording;
        public Macro CurrentRecording => _currentRecording;

        public void StartRecording(string macroName, string description = "")
        {
            _currentRecording = new Macro
            {
                Id = Guid.NewGuid().ToString(),
                Name = macroName,
                Description = description,
                CreatedAt = DateTime.Now,
                Steps = new List<MacroStep>()
            };
            _isRecording = true;
        }

        public void StopRecording()
        {
            if (_isRecording && _currentRecording != null)
            {
                _currentRecording.UpdatedAt = DateTime.Now;
                _macros.Add(_currentRecording);
                _isRecording = false;
                _currentRecording = null;
            }
        }

        public void RecordStep(TrafficEntry entry)
        {
            if (!_isRecording || _currentRecording == null)
                return;

            var step = new MacroStep
            {
                Id = Guid.NewGuid().ToString(),
                StepNumber = _currentRecording.Steps.Count + 1,
                Method = entry.Method,
                Url = entry.Url,
                Host = entry.Host,
                Path = entry.Path,
                RequestBody = ExtractBody(entry.RawRequest),
                Headers = ExtractHeaders(entry.RawRequest),
                Timestamp = DateTime.Now,
                Variables = new Dictionary<string, string>()
            };

            _currentRecording.Steps.Add(step);
        }

        public void CancelRecording()
        {
            _isRecording = false;
            _currentRecording = null;
        }

        public List<Macro> GetAllMacros() => _macros.ToList();

        public Macro GetMacro(string id) => _macros.FirstOrDefault(m => m.Id == id);

        public void DeleteMacro(string id)
        {
            _macros.RemoveAll(m => m.Id == id);
        }

        public void UpdateMacro(Macro macro)
        {
            var existing = _macros.FirstOrDefault(m => m.Id == macro.Id);
            if (existing != null)
            {
                int index = _macros.IndexOf(existing);
                macro.UpdatedAt = DateTime.Now;
                _macros[index] = macro;
            }
        }

        public async Task<MacroPlaybackResult> PlayMacro(string macroId, MacroPlaybackOptions options = null)
        {
            var macro = GetMacro(macroId);
            if (macro == null)
                return new MacroPlaybackResult { Success = false, Error = "Macro not found" };

            options = options ?? new MacroPlaybackOptions();

            var result = new MacroPlaybackResult
            {
                MacroId = macroId,
                MacroName = macro.Name,
                StartTime = DateTime.Now,
                StepResults = new List<MacroStepResult>()
            };

            var variables = new Dictionary<string, string>(options.Variables ?? new Dictionary<string, string>());

            try
            {
                for (int i = 0; i < macro.Steps.Count; i++)
                {
                    var step = macro.Steps[i];

                    // Apply variable substitution
                    var processedStep = SubstituteVariables(step, variables);

                    // Execute step
                    var stepResult = await ExecuteStep(processedStep, options);
                    result.StepResults.Add(stepResult);

                    // Extract variables from response
                    if (stepResult.Success && !string.IsNullOrEmpty(stepResult.ResponseBody))
                    {
                        ExtractVariablesFromResponse(stepResult.ResponseBody, step.VariableExtractions, variables);
                    }

                    // Check if step failed and should stop
                    if (!stepResult.Success && options.StopOnError)
                    {
                        result.Success = false;
                        result.Error = $"Step {i + 1} failed: {stepResult.Error}";
                        break;
                    }

                    // Delay between steps
                    if (i < macro.Steps.Count - 1 && options.DelayBetweenSteps > 0)
                    {
                        await Task.Delay(options.DelayBetweenSteps);
                    }
                }

                result.EndTime = DateTime.Now;
                result.Duration = (result.EndTime - result.StartTime).TotalMilliseconds;
                result.Success = result.StepResults.All(sr => sr.Success);

                macro.LastRun = DateTime.Now;
                macro.RunCount++;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                result.EndTime = DateTime.Now;
                result.Duration = (result.EndTime - result.StartTime).TotalMilliseconds;
            }

            return result;
        }

        public async Task<List<MacroPlaybackResult>> PlayMacroBatch(string macroId, int iterations, MacroPlaybackOptions options = null)
        {
            var results = new List<MacroPlaybackResult>();

            for (int i = 0; i < iterations; i++)
            {
                var result = await PlayMacro(macroId, options);
                results.Add(result);

                if (!result.Success && options?.StopOnError == true)
                    break;
            }

            return results;
        }

        private MacroStep SubstituteVariables(MacroStep step, Dictionary<string, string> variables)
        {
            var processed = new MacroStep
            {
                Id = step.Id,
                StepNumber = step.StepNumber,
                Method = step.Method,
                Url = SubstituteText(step.Url, variables),
                Host = SubstituteText(step.Host, variables),
                Path = SubstituteText(step.Path, variables),
                RequestBody = SubstituteText(step.RequestBody, variables),
                Headers = step.Headers?.ToDictionary(h => h.Key, h => SubstituteText(h.Value, variables)),
                Timestamp = step.Timestamp,
                Variables = step.Variables,
                VariableExtractions = step.VariableExtractions
            };

            return processed;
        }

        private string SubstituteText(string text, Dictionary<string, string> variables)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            foreach (var variable in variables)
            {
                text = text.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            }

            // Built-in variables
            text = text.Replace("{{timestamp}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            text = text.Replace("{{date}}", DateTime.Now.ToString("yyyy-MM-dd"));
            text = text.Replace("{{time}}", DateTime.Now.ToString("HH:mm:ss"));
            text = text.Replace("{{guid}}", Guid.NewGuid().ToString());
            text = text.Replace("{{random}}", new Random().Next(1000, 9999).ToString());

            return text;
        }

        private void ExtractVariablesFromResponse(string response, List<VariableExtraction> extractions, Dictionary<string, string> variables)
        {
            if (extractions == null || !extractions.Any())
                return;

            foreach (var extraction in extractions)
            {
                try
                {
                    if (extraction.ExtractionType == ExtractionType.Regex)
                    {
                        var match = Regex.Match(response, extraction.Pattern);
                        if (match.Success)
                        {
                            variables[extraction.VariableName] = match.Groups[1].Value;
                        }
                    }
                    else if (extraction.ExtractionType == ExtractionType.JsonPath)
                    {
                        // Simple JSON path extraction (basic implementation)
                        var value = ExtractJsonValue(response, extraction.Pattern);
                        if (value != null)
                        {
                            variables[extraction.VariableName] = value;
                        }
                    }
                    else if (extraction.ExtractionType == ExtractionType.Header)
                    {
                        var headerMatch = Regex.Match(response, $"{extraction.Pattern}:\\s*([^\\r\\n]+)", RegexOptions.IgnoreCase);
                        if (headerMatch.Success)
                        {
                            variables[extraction.VariableName] = headerMatch.Groups[1].Value;
                        }
                    }
                }
                catch
                {
                    // Ignore extraction errors
                }
            }
        }

        private string ExtractJsonValue(string json, string path)
        {
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(json);
                var keys = path.Split('.');
                System.Text.Json.JsonElement current = doc.RootElement;

                foreach (var key in keys)
                {
                    if (current.ValueKind == System.Text.Json.JsonValueKind.Object)
                    {
                        if (!current.TryGetProperty(key, out current))
                            return null;
                    }
                    else if (current.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        if (!int.TryParse(key, out int index))
                            return null;
                        current = current[index];
                    }
                }

                return current.ToString();
            }
            catch
            {
                return null;
            }
        }

        private async Task<MacroStepResult> ExecuteStep(MacroStep step, MacroPlaybackOptions options)
        {
            var result = new MacroStepResult
            {
                StepNumber = step.StepNumber,
                Method = step.Method,
                Url = step.Url,
                StartTime = DateTime.Now
            };

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    if (options.Timeout > 0)
                    {
                        client.Timeout = TimeSpan.FromMilliseconds(options.Timeout);
                    }

                    // Build request
                    var request = new System.Net.Http.HttpRequestMessage(
                        new System.Net.Http.HttpMethod(step.Method),
                        step.Url);

                    // Add headers
                    if (step.Headers != null)
                    {
                        foreach (var header in step.Headers)
                        {
                            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }
                    }

                    // Add body
                    if (!string.IsNullOrEmpty(step.RequestBody))
                    {
                        request.Content = new System.Net.Http.StringContent(
                            step.RequestBody,
                            System.Text.Encoding.UTF8,
                            "application/json");
                    }

                    // Send request
                    var response = await client.SendAsync(request);

                    result.StatusCode = (int)response.StatusCode;
                    result.ResponseBody = await response.Content.ReadAsStringAsync();
                    result.ResponseHeaders = response.Headers.ToDictionary(
                        h => h.Key,
                        h => string.Join(", ", h.Value));

                    result.Success = response.IsSuccessStatusCode;
                    result.EndTime = DateTime.Now;
                    result.Duration = (result.EndTime - result.StartTime).TotalMilliseconds;
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Error = ex.Message;
                result.EndTime = DateTime.Now;
                result.Duration = (result.EndTime - result.StartTime).TotalMilliseconds;
            }

            return result;
        }

        private string ExtractBody(string rawRequest)
        {
            if (string.IsNullOrEmpty(rawRequest))
                return "";

            var bodyStartIndex = rawRequest.IndexOf("\r\n\r\n");
            if (bodyStartIndex == -1)
                bodyStartIndex = rawRequest.IndexOf("\n\n");

            if (bodyStartIndex == -1)
                return "";

            return rawRequest.Substring(bodyStartIndex + 4);
        }

        private Dictionary<string, string> ExtractHeaders(string rawRequest)
        {
            var headers = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(rawRequest))
                return headers;

            var lines = rawRequest.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in lines.Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line))
                    break;

                var colonIndex = line.IndexOf(':');
                if (colonIndex > 0)
                {
                    var key = line.Substring(0, colonIndex).Trim();
                    var value = line.Substring(colonIndex + 1).Trim();
                    headers[key] = value;
                }
            }

            return headers;
        }

        public MacroStatistics GetStatistics(string macroId)
        {
            var macro = GetMacro(macroId);
            if (macro == null)
                return null;

            return new MacroStatistics
            {
                MacroId = macro.Id,
                MacroName = macro.Name,
                TotalSteps = macro.Steps.Count,
                TotalRuns = macro.RunCount,
                LastRun = macro.LastRun,
                CreatedAt = macro.CreatedAt,
                UniqueEndpoints = macro.Steps.Select(s => s.Path).Distinct().Count(),
                MethodDistribution = macro.Steps.GroupBy(s => s.Method)
                    .ToDictionary(g => g.Key, g => g.Count())
            };
        }

        public void ExportMacro(string macroId, string filePath)
        {
            var macro = GetMacro(macroId);
            if (macro == null)
                return;

            var json = System.Text.Json.JsonSerializer.Serialize(macro, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            System.IO.File.WriteAllText(filePath, json);
        }

        public void ImportMacro(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return;

            var json = System.IO.File.ReadAllText(filePath);
            var macro = System.Text.Json.JsonSerializer.Deserialize<Macro>(json);

            if (macro != null)
            {
                // Generate new ID to avoid conflicts
                macro.Id = Guid.NewGuid().ToString();
                macro.CreatedAt = DateTime.Now;
                macro.UpdatedAt = DateTime.Now;
                _macros.Add(macro);
            }
        }
    }

    public class Macro
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? LastRun { get; set; }
        public int RunCount { get; set; }
        public List<MacroStep> Steps { get; set; } = new List<MacroStep>();
    }

    public class MacroStep
    {
        public string Id { get; set; }
        public int StepNumber { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public string Host { get; set; }
        public string Path { get; set; }
        public string RequestBody { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Variables { get; set; }
        public List<VariableExtraction> VariableExtractions { get; set; }
    }

    public class VariableExtraction
    {
        public string VariableName { get; set; }
        public ExtractionType ExtractionType { get; set; }
        public string Pattern { get; set; }
    }

    public class MacroPlaybackOptions
    {
        public bool StopOnError { get; set; } = true;
        public int DelayBetweenSteps { get; set; } = 100; // milliseconds
        public int Timeout { get; set; } = 30000; // milliseconds
        public Dictionary<string, string> Variables { get; set; }
    }

    public class MacroPlaybackResult
    {
        public string MacroId { get; set; }
        public string MacroName { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public List<MacroStepResult> StepResults { get; set; }
    }

    public class MacroStepResult
    {
        public int StepNumber { get; set; }
        public string Method { get; set; }
        public string Url { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
        public int StatusCode { get; set; }
        public string ResponseBody { get; set; }
        public Dictionary<string, string> ResponseHeaders { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
    }

    public class MacroStatistics
    {
        public string MacroId { get; set; }
        public string MacroName { get; set; }
        public int TotalSteps { get; set; }
        public int TotalRuns { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UniqueEndpoints { get; set; }
        public Dictionary<string, int> MethodDistribution { get; set; }
    }

    public enum ExtractionType
    {
        Regex,
        JsonPath,
        Header
    }
}
