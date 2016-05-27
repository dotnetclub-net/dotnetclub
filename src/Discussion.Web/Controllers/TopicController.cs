using Discussion.Web.Models;
using Microsoft.AspNet.Mvc;
using System.Linq;
using Discussion.Web.Data;
using Discussion.Web.ViewModels;
using System;
using MarkdownSharp;
using System.Text.RegularExpressions;
using System.Text;
using MarkdownSharp.Extensions;
using System.Reflection;

namespace Discussion.Web.Controllers
{
    public class TopicController : Controller
    {

        private readonly IDataRepository<Topic> _topicRepo;
        public TopicController(IDataRepository<Topic> topicRepo)
        {
            _topicRepo = topicRepo;
        }


        [Route("/Topic/{id}")]
        public ActionResult Index(int id)
        {
            var topic = _topicRepo.Retrive(id);
            if(topic == null)
            {
                return HttpNotFound();
            }


            // Create new markdown instance
            var markdownConverter = new Markdown { AllowEmptyLinkText = true };
            markdownConverter.AddExtension(new GfmCodeBlocks(markdownConverter));
            var showModel = new TopicShowModel
            {
                Id = topic.Id,
                Title = topic.Title,
                MarkdownContent = topic.Content,
                HtmlContent = markdownConverter.Transform(topic.Content)
            };

            return View(showModel);
        }


        [Route("/")]
        [Route("/Topic/List")]
        public ActionResult List()
        {
            var topicList = _topicRepo.All.ToList();

            return View(topicList);
        }


        [Route("/Topic/Create")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("/Topic/CreateTopic")]
        public ActionResult CreateTopic(TopicCreationModel model)
        {
            if (!ModelState.IsValid)
            {
                return HttpBadRequest();
            }

            var topic = new Topic
            {
                Title = model.Title,
                Content = model.Content,
                TopicType = TopicType.Sharing,
                CreatedAt = DateTime.UtcNow
            };

            _topicRepo.Create(topic);
            return RedirectToAction("Index", new { topic.Id });
        }
    }

    public class GfmCodeBlocks : IExtensionInterface
    {

        private static Regex _codeBlock = new Regex(@"(?:\r?\n|^)(`{3,}|~{3,})([\u0020\t]*(?<lang>\S+))?[\u0020\t]*\r?\n
	(?<code>[^\r^\n]*\r?\n)*?
	\1(?:\r?\n|$)",
        RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        private readonly Markdown _converterIntance;

        public GfmCodeBlocks(Markdown converterIntance)
        {
            _converterIntance = converterIntance;
        }

        public string Transform(string markdown)
        {
            var transformed = _codeBlock.Replace(markdown, new MatchEvaluator(CodeBlockEvaluator));

            Type converterType = typeof(Markdown);
            var hashHTMLBlocks = converterType.GetMethod("HashHTMLBlocks", BindingFlags.NonPublic | BindingFlags.Instance);
            var afterHashed = hashHTMLBlocks.Invoke(_converterIntance, new[] { transformed }) as string;

            return afterHashed;
        }


        private string CodeBlockEvaluator(Match match)
        {
            var preBuilder = new StringBuilder();
            preBuilder.AppendLine();
            preBuilder.Append("<pre");

            var lang = match.Groups["lang"].Value;
            if (!string.IsNullOrWhiteSpace(lang))
            {
                preBuilder.AppendFormat(@" class=""language-{0}""", lang);
            }

            preBuilder.Append("><code>");
            foreach (Capture line in match.Groups["code"].Captures)
            {
                preBuilder.Append(line.Value);
            }

            var pre = preBuilder.ToString();
            pre = Regex.Replace(pre, @"\r?\n$", string.Empty);
            return string.Concat(pre, "</code></pre>\n");
        }
    }
}