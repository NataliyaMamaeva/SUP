using ERP.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FuzzySharp;

namespace ERP.Controllers
{
    public class JournalController : Controller
    {
        private static readonly SemaphoreSlim _outputLock = new SemaphoreSlim(1, 1);
        private readonly ErpContext _context;
        private readonly UserManager<ErpUser> _userManager;
        private readonly IDataProtector _protector;
        public JournalController(UserManager<ErpUser> userManager, ErpContext context, IDataProtectionProvider provider)
        {
            _userManager = userManager;
            _context = context;
            _protector = provider.CreateProtector("PassportShifre");
        }
        public ActionResult Topics()
        {
            var model = _context.JournalTopics
                            .Include(t  => t.JournalNotes)
                                .ThenInclude(n => n.Photos)
                            .Include(t => t.JournalNotes)
                                .ThenInclude(n => n.Project)
                                    .ToList();



            var topics = _context.JournalTopics.ToList();
            ViewBag.topics = topics;

            return View(model);
        }

        public IActionResult AddTopicPartial() { return PartialView("_AddTopicPartial"); }



        [HttpPost]
        [Route("Journal/AddTopic")]
        public IActionResult AddTopic([FromForm] JournalTopic topic )
        {
            Console.WriteLine("--------------Method: AddTopic-----------");
            //string materialName = Request.Query["Materials"];
            Console.WriteLine(  topic );

            //var isExist = _context.JournalTopics
            //  .AsEnumerable() // Загружаем в память (иначе EF не поддержит FuzzySharp)
            //  .FirstOrDefault(m => Fuzz.PartialRatio(m.JournalTopicName, topic.JournalTopicName) >= 95);

            //var isExist = _context.JournalTopics
            //    .FirstOrDefault(m => m.JournalTopicName.Trim().ToLower() == topic.JournalTopicName.Trim().ToLower());

            var isExist = _context.JournalTopics
                .FirstOrDefault(m => m.JournalTopicName == topic.JournalTopicName);
            if (isExist != null)
                return BadRequest("уже существует");

            JournalTopic topicNew = new();
            topicNew.JournalTopicName = topic.JournalTopicName;

            _context.JournalTopics.Add(topicNew);
            _context.SaveChanges();

            return Ok("Topics успешно загружены.");
        }
    }
}
