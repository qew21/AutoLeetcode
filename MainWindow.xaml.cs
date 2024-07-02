using CefSharp;
using CefSharp.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.IO;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using CefSharp.Web;
using System.Threading;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Threading;
using AutoLeetcode.Properties;
using System.Runtime;
using OpenAI_API.Models;
using CefSharp.DevTools.Network;
using OpenAI_API.Chat;
using System.Runtime.Remoting.Messaging;
using static System.Net.Mime.MediaTypeNames;
using OpenAI_API.Moderation;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace AutoLeetcode
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        string title = string.Empty;
        string problem = string.Empty;
        string code = string.Empty;
        string _abstract = string.Empty;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private DispatcherTimer _timer;
        int totalCount = 0;
        int correctCount = 0;
        Configuration config;



        public MainWindow()
        {

            var settings = new CefSettings()
            {
                CachePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache")
            };
            settings.CefCommandLineArgs.Add("enable-media-stream");
            settings.CefCommandLineArgs.Add("use-fake-ui-for-media-stream");
            settings.CefCommandLineArgs.Add("enable-usermedia-screen-capturing");
            Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);
            InitializeComponent();
            InitializeTimer();
            config = ConfigManager.LoadConfig();
            Model.Content = config.Model;
        }

        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
            Time.Content = _elapsedTime.ToString(@"hh\:mm\:ss");
        }

        private async Task<string> AskAI(string promt)
        {
            string response = "";
            try
            {
                var api = new OpenAI_API.OpenAIAPI(config.ApiKey)
                {
                    ApiUrlFormat = config.BaseUrl + "/{0}/{1}"
                };
                Model model = new Model(config.Model);
                ChatRequest defaultChatRequestArgs = new ChatRequest();
                defaultChatRequestArgs.Model = model;
                api.Chat.DefaultChatRequestArgs = defaultChatRequestArgs;
                ChatMessage message = new ChatMessage(ChatMessageRole.User, promt);
                List<ChatMessage> chatMessages = new List<ChatMessage>
                {
                    message
                };
                ChatRequest request = new ChatRequest(defaultChatRequestArgs)
                {
                    Messages = chatMessages
                };
                var result = await api.Chat.CreateChatCompletionAsync(request);
                response = result.Choices[0].Message.TextContent;
                Logger.Log.Debug($"response from {config.Model}: {response}");
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }

            return response;
        }

        private async void GetProblems()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string filePath = $"{timestamp}.jsonl";
            int cnt = 100;
            while (cnt > 0)
            {
                browser.LoadUrl($"https://leetcode.cn/problemset/?page={StartNumber.Text}");
                await Task.Delay(5000);
                string compareValue = "==";
                string start = "0";
                if (config.Skip != "1")
                {
                    compareValue = ">=";
                    start = "1";
                }
                string titleScript = @"(async function(){var rowGroupElementAll = document.querySelectorAll('[role=""rowgroup""]');
            var rowGroupElement = null;
            for (var i = 0; i < rowGroupElementAll.length; i++) {
                if (rowGroupElementAll[i].childElementCount > 0) {
                    rowGroupElement = rowGroupElementAll[i];
                    break;
                }
            }
            if (rowGroupElement) {
                var rows = rowGroupElement.children;
                var result = {};
                for (var i = " + start + @"; i < rows.length; i++) {
                    var link = rows[i].querySelector('a');
                    var difficulty = rows[i].childNodes[4].innerText.trim();
                    var level = getLevelFromString(difficulty);
                    if (link && rows[i].childNodes[0].childElementCount " + compareValue + @" 0) {
                        result[link.getAttribute('href')] = level;
                    }
                }
                return JSON.stringify(result); // 直接返回结果对象
            } else {
                return """"; // 或者返回一个表示未找到的默认值
            }

            function getLevelFromString(difficulty) {
                switch (difficulty) {
                    case '简单': return 0;
                    case '中等': return 1;
                    case '困难': return 2;
                    default: return -1;
                }
            }})();";
                var titleResponse = await browser.EvaluateScriptAsync(titleScript);
                try
                {
                    string titleText = titleResponse.Result.ToString();
                    Dictionary<string, string> result = JsonConvert.DeserializeObject<Dictionary<string, string>>(titleText);

                    foreach (var item in result)
                    {
                        cnt -= 1;
                        if (config.Level != "hard" && item.Value == "2")
                        {
                            continue;
                        }
                        if (config.Level == "easy" && item.Value == "1")
                        {
                            continue;
                        }
                        string url = $"https://leetcode.cn{item.Key}";
                        browser.LoadUrl(url);
                        Status.Content = "读题";
                        await Task.Delay(5000);
                        string solution = await Solve();
                        bool res = false;
                        if (!String.IsNullOrEmpty(solution))
                        {
                            Status.Content = "解答提交";
                            for (int i = 3; i > 0; i--)
                            {
                                await Execute();
                                await Task.Delay(3000);
                                res = await GetStatus();
                                if (res)
                                {
                                    await Sumbit();
                                    break;
                                }
                            }
                        }
                        else
                        {
                            Status.Content = "解答失败";
                        }
                        totalCount++;
                        Count.Content = $"{correctCount}/{totalCount}";
                        string id = string.Empty;
                        Match match = Regex.Match(title, @"^\d+");

                        if (match.Success)
                        {
                            id = match.Value;
                        }
                        else
                        {
                            Logger.Log.Debug("No number found at the beginning of the string.");
                        }
                        Dictionary<string, string> record = new Dictionary<string, string>
                        {
                            ["Page"] = StartNumber.Text,
                            ["ID"] = id,
                            ["Url"] = url,
                            ["Difficulty"] = item.Value,
                            ["Title"] = title,
                            ["Problem"] = problem,
                            ["Solution"] = solution,
                            ["Abstract"] = _abstract,
                            ["Solve"] = res.ToString()
                        };
                        using (StreamWriter writer = new StreamWriter(filePath, true)) // 注意这里的第三个参数为true
                        {

                            string line = JsonConvert.SerializeObject(record);
                            writer.WriteLine(line);
                        }

                        if (cnt == 0)
                        {
                            break;
                        }
                    }
                    StartNumber.Text = (Convert.ToInt16(StartNumber.Text) + 1).ToString();
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex.Message);
                }
            }

        }

        private async Task<string> GetCodes()
        {
            string titleScript = @"(async function(){let viewLines = document.querySelectorAll('.view-lines.monaco-mouse-cursor-text');
            let codeContent = [];

            viewLines[0].childNodes.forEach(line => {
                let lineContent = '';
                let spans = line.querySelectorAll('span'); // 收集该行内的所有<span>
    
                spans.forEach(span => {
                    lineContent += span.textContent.trim(); // 累加每个span的文本内容
                });
                if(lineContent[0] != ""#"")
                codeContent.push(lineContent); // 将整行内容添加到结果数组
            });

            return codeContent.join('\n'); // 将收集到的每一行代码以换行分隔输出
            })();";
            var titleResponse = await browser.EvaluateScriptAsync(titleScript);
            try
            {
                string titleText = titleResponse.Result.ToString();
                Logger.Log.Debug(titleText);
                return titleText;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
            return "";
        }
        private async void GetProblem()
        {
            string titleScript = "document.querySelector('.text-title-large a').innerText";
            var titleResponse = await browser.EvaluateScriptAsync(titleScript);
            title = titleResponse.Result.ToString();
            Logger.Log.Debug("Title: " + title);
            string descScript = "document.querySelector('.elfjS').innerText.trim()";
            var descResponse = await browser.EvaluateScriptAsync(descScript);
            problem = descResponse.Result.ToString();
            Logger.Log.Debug("Description:\n" + problem);
        }

        private async Task<bool> GetStatus()
        {
            await Task.Delay(1000);
            string redCountScript = "document.querySelectorAll('.rounded-full.bg-red-s').length";
            var redCountResponse = await browser.EvaluateScriptAsync(redCountScript);
            int redCount = (int)redCountResponse.Result;

            string greenCountScript = "document.querySelectorAll('.rounded-full.bg-green-s').length";
            var greenCountResponse = await browser.EvaluateScriptAsync(greenCountScript);
            int greenCount = (int)greenCountResponse.Result;

            Logger.Log.Debug($"Red elements: {redCount}, Green elements: {greenCount}");
            return (greenCount > 0 && redCount == 0);
        }
        private void CleanRegion()
        {
            int x = Convert.ToInt16(this.Left + this.ActualWidth + 300);
            int y = Convert.ToInt16(this.Top + 500);
            Thread.Sleep(500);
            MouseHookHelper.SetCursorPos(x, y);
            MouseHookHelper.mouse_event(MouseHookHelper.MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            MouseHookHelper.mouse_event(MouseHookHelper.MOUSEEVENTF_LEFTUP, x, y, 0, 0);

            KeyboardSimulator.SendCtrlA();
            KeyboardSimulator.SendCtrlX();
            KeyboardSimulator.ReleaseCtrl();
        }
        private async Task<string> Solve()
        {
            string solution = "";
            _abstract = "";
            await Task.Delay(1000);
            GetProblem();
            ChoosePython3();
            code = await GetCodes();
            for (int i = 0; i < 5; i++)
            {
                CleanRegion();
                string current = await GetCodes();
                if (string.IsNullOrEmpty(current))
                {
                    break;
                }
            }

            try
            {
                Status.Content = "AI处理中";
                string prompt = $"{title} {problem}请使用python3解答，包含给出的前置代码{code}，示例格式为```json\r\n{{\r\n  \"abstract\": \"使用动态规划和最小堆 ...\",\r\n  \"solution\": \"\\nclass Solution:\\n   ...\\n\"\r\n}}\r\n```， 包含两个key，分别是abstract介绍方法，solution给出纯python代码的答案，用于提交到leetcode，请验算一遍确保结果正确，只要一个答案，请勿返回json格式以外的内容干扰解析";
                string solutions = await AskAI(prompt);
                Status.Content = "AI处理完成";
                string pattern = @"(?<=\`\`\`json)[\s\S]*?(?=\`\`\`)";
                MatchCollection matches = Regex.Matches(solutions, pattern, RegexOptions.Multiline);

                foreach (Match match in matches)
                {
                    solution = match.Value;
                    Logger.Log.Debug($"solution = {solution}");
                    Dictionary<string, string> response;
                    try
                    {
                        solution = solution.Replace(":\\ ", ":\\n ");
                        response = JsonConvert.DeserializeObject<Dictionary<string, string>>(solution);
                    }
                    catch
                    {
                        string repaired = JsonRepair.RepairJson(solution);
                        Logger.Log.Debug($"repaired = {repaired}");
                        response = JsonConvert.DeserializeObject<Dictionary<string, string>>(repaired);
                    }

                    solution = response["solution"];
                    if (!solution.Contains("class Solution"))
                    {
                        solution = "class Solution:\n" + solution;
                    }
                    solution = Regex.Replace(solution, @"[\u4e00-\u9fa5]", "");
                    _abstract = response["abstract"];
                }
                try
                {
                    Clipboard.SetData(DataFormats.Text, (Object)solution);
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex.Message);
                    try
                    {
                        Clipboard.SetText(solution);
                    }
                    catch (Exception ex1)
                    {
                        Logger.Log.Error(ex1.Message);
                    }
                }

                KeyboardSimulator.SendCtrlV();
                KeyboardSimulator.ReleaseCtrl();

            }
            catch (Exception ex1)
            {
                Logger.Log.Error(ex1.Message);
            }
            return solution;
        }

        private async void ChoosePython3()
        {
            string descScript = @"(function() {
                let containsDemo = false;
                document.querySelectorAll('.popover-wrapper').forEach(i => {
                    if (i.innerText.includes('Python3')) {
                        containsDemo = true;
                        return; 
                    }
                });
                return containsDemo ? 1 : 0; 
            })();";
            var titleResponse = await browser.EvaluateScriptAsync(descScript);
            title = titleResponse.Result.ToString();
            if (title == "0")
            {
                descScript = "document.querySelectorAll('.bg-transparent.whitespace-nowrap.text-text-secondary')[0].click()";
                await browser.EvaluateScriptAsync(descScript);
                descScript = "document.querySelectorAll('.border-border-tertiary')[1].childNodes[3].click()";
                await browser.EvaluateScriptAsync(descScript);
            }
        }

        private async Task<bool> Execute()
        {
            await Task.Delay(1000);
            string descScript = "document.querySelector('button[data-e2e-locator=\"console-run-button\"]').click()";
            await browser.EvaluateScriptAsync(descScript);
            return true;
        }

        private async Task<bool> Sumbit()
        {
            string descScript = "document.querySelector('button[data-e2e-locator=\"console-submit-button\"]').click()";
            await browser.EvaluateScriptAsync(descScript);
            try
            {
                descScript = "document.querySelector('.text-green-s').innerText";
                var descResponse = await browser.EvaluateScriptAsync(descScript);
                string descText = descResponse.Result.ToString();
                if (descText == "通过")
                {
                    correctCount++;
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.Message);
            }
            return true;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            GetProblems();
        }

    }

    public static class Logger
    {

        public static log4net.ILog Log = log4net.LogManager.GetLogger("LogFileAppender");
    }
}
