using LLama;
using LLama.Common;
using LLama.Sampling;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MeebookConnector
{
    public class AiService : IDisposable
    {
        private const string _modelPath = @"C:\LLMs\gemma-3n-E4B-it-IQ4_XS.gguf";
        private string _systemInstruction = "";

        private StatelessExecutor? _executor { get; set; }
        private DefaultSamplingPipeline? _samplingPipeline;
        private InferenceParams? _inferenceParams;
        private LLamaWeights? _model;
        private CancellationTokenSource cts;

        public AiService(CancellationToken cancellationToken = default)
        {
            this.cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        public async Task LoadModel()
        {
            using var stream = Assembly.GetEntryAssembly()?.GetManifestResourceStream($"MeebookConnector.Resources.SystemInstruction.txt")!;
            using var streamReader = new StreamReader(stream, Encoding.UTF8);
            _systemInstruction = await streamReader.ReadToEndAsync(cancellationToken: cts.Token);

            var parameters = new ModelParams(_modelPath)
            {
                ContextSize = 2048,
                GpuLayerCount = 50,
                
            };

            _model = await LLamaWeights.LoadFromFileAsync(parameters);
            //context = model.CreateContext(parameters);

            _executor = new StatelessExecutor(_model, parameters)
            {
                ApplyTemplate = true,
                SystemMessage = _systemInstruction,
            };
            
            _samplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = 0.7f
            };

            _inferenceParams = new InferenceParams()
            {
                SamplingPipeline = _samplingPipeline,
                AntiPrompts = new List<string> { "\n\n\n>" },
                MaxTokens = -1,
            };

            //await Executor.PrefillPromptAsync(SystemInstruction);            
        }

        public async Task<string> SummarizeWeekPlanItems(List<AulaPlanItem> items)
        {
            try
            {
                if (_executor == null)
                {
                    throw new Exception("Executor is not initialized. Please load the model first.");
                }

                StringBuilder input = new StringBuilder();
                foreach (var item in items.OrderBy(d => d.Date))
                {
                    
                    input.AppendLine($"- {DateTimeFormatInfo.CurrentInfo.GetDayName(item.Date.DayOfWeek)} - {item.Categories.FirstOrDefault()}: {item.Text}");
                }

                StringBuilder stringBuilder = new();
                
                Console.ForegroundColor = ConsoleColor.Green;
                await foreach (var text in _executor.InferAsync(input.ToString() + Environment.NewLine, _inferenceParams, cts.Token))
                {
                    //Console.Write(text);
                    stringBuilder.Append(text);
                }
                Console.ResetColor();

                string result = stringBuilder.ToString();

                // Create HTML encoded string
                string htmlEncoded = HttpUtility.HtmlEncode(result.Replace(">", "").Trim());

                // Remove tailing \n \r
                htmlEncoded = htmlEncoded.TrimEnd('\n', '\r').Trim();

                // Replace newlines with <br />
                string withLineBreaks = htmlEncoded.Replace("\n", "<br />").Replace("\r", "");

                // Replace tabs with four non-breaking spaces
                string withTabs = withLineBreaks.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");

                // Replace consecutive spaces with non-breaking spaces
                string withSpaces = withTabs.Replace("  ", "&nbsp;&nbsp;");

                return withSpaces;

            }
            catch (OperationCanceledException)
            {
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return "";
        }

        public void Dispose()
        {
            _model?.Dispose();
            _samplingPipeline?.Dispose();            
            _executor = null;            
        }
    }
}
