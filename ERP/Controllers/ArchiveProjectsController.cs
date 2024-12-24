using ERP.Models;
using ERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers
{
    [Authorize]
    public class ArchiveProjectsController : Controller
    {
        private readonly ErpContext _context;

        public ArchiveProjectsController(ErpContext context)
        {
            _context = context;
        }

        public IActionResult ArchiveProjects()
        {
            var clients = _context.Clients
                .Select(client => new ClientProjectsViewModel
                {
                    ClientId = client.ClientId,
                    ClientTitle = client.Title,
                    City = client.City,
                    FirstRequestDate = client.FirstRequstDate,
                    Projects = client.Projects.Select(project => new ClientProjectViewModel
                    {
                        ProjectId = project.ProjectId,
                        ProjectName = project.ProjectName,
                        EventDate = project.EventDate,
                        Deadline = project.Deadline,
                        PaymentTotal = project.PaymentTotal,
                        AdvanceRate = project.AdvanceRate,
                        ImagePath = project.Items.FirstOrDefault().SketchPath
                    }).ToList(),
                    Requisites = client.Requisites.Select(requisite => new ClientRequisiteViewModel
                    {
                        RequisiteId = requisite.RequisiteId,
                        FileTitle = requisite.FileTitle,
                        FilePath = requisite.FilePath,
                        Comment = requisite.Comment,
                        UploadedAt = requisite.UploadedAt
                    }).ToList(),
                    DeliveryAddresses = client.DeliveryAddresses.Select(address => new ClientDeliveryAddressViewModel
                    {
                        AddressId = address.AddressId,
                        DeliveryAddress = address.DeliveryAddress1
                    }).ToList(),
                    Contacts = client.Contacts.Select(contact => new ClientContactViewModel
                    {
                        ContactId = contact.ContactId,
                        ContactName = contact.ContactName,
                        PhoneNumber = contact.PhoneNumber,
                        Email = contact.Email,
                        Passport = contact.Passport
                    }).ToList()
                })
                .ToList();

            foreach(var c in clients)
            {
                foreach (var p in c.Projects)
                    Console.WriteLine("imagePath: " + p.ImagePath);
            }

            return View(clients);
        }

        public ActionResult Gallery()
        {
            return View();
        }

        // GET: ArchiveProjectsController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ArchiveProjectsController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ArchiveProjectsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ArchiveProjectsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ArchiveProjectsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ArchiveProjectsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ArchiveProjectsController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
