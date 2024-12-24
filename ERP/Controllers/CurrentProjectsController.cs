using ERP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using Newtonsoft.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ERP.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;


namespace ERP.Controllers
{
    //[Authorize]
    
    public class CurrentProjectsController : Controller
    {
        private static readonly SemaphoreSlim _outputLock = new SemaphoreSlim(1, 1);
        private readonly ErpContext _context;
        private readonly UserManager<ErpUser> _userManager;
        public CurrentProjectsController(UserManager<ErpUser> userManager, ErpContext context)
        {
            _userManager = userManager;
            _context = context;
        }
        public ActionResult Index() { return View(); }
        public IActionResult CreateEmployeePartial() { return PartialView("_CreateEmployeePartial"); } 
        public IActionResult CreateClientPartial() { return PartialView("_CreateClientPartial"); }
        public IActionResult CreateProjectPartial() { return PartialView("_CreateProjectPartial"); }
        public IActionResult ProjectCardPartial() { return PartialView("_ProjectCardPartial"); }

        // POST: CurrentProjectsController/Create
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

        // GET: CurrentProjectsController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CurrentProjectsController/Edit/5
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

        // GET: CurrentProjectsController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CurrentProjectsController/Delete/5
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

        //----------------------------------контроллеры обработчики---------------------------------------//

        [HttpPost]
        public async Task<IActionResult> AddEmployee(CreateEmployeeViewModel model)
        {
            Console.WriteLine(  model );
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные. Проверьте форму.");
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return Conflict(new { Message = "Пользователь с таким email уже существует." });
            }
            var employee = new Employee
            {
                EmployeeName = model.EmployeeName,
                PhoneNumber = model.PhoneNumber,
                Passport = model.Passport,
                Position = model.Position,
                Email = model.Email
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            var user = new ErpUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmployeeId = employee.EmployeeId
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                return BadRequest(new { Message = "Не удалось создать пользователя.", Errors = result.Errors });         
            }
            return RedirectToAction("Index");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("CurrentProjects/AddClient")]
        public async Task<IActionResult> AddClient([FromForm] ClientViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    var field = state.Key; // Название поля
                    var errors = state.Value.Errors; // Ошибки для этого поля
                    foreach (var error in errors)
                    {  Console.WriteLine($"Поле: {field}, Ошибка: {error.ErrorMessage}"); }
                }
            }
            await _outputLock.WaitAsync();
            try
            {
                if (_context.Clients.FirstOrDefault(c => c.Title == model.ClientTitle) != null)
                    { return BadRequest("Клиент с таким названием уже существует"); }

                Console.WriteLine(model);

                Client client = new();
                client.Title = model.ClientTitle;
                client.City = model.City;
                client.FirstRequstDate = model.FirstContact;
                _context.Clients.Add(client);
                _context.SaveChanges();
                int clientId =  _context.Clients.FirstOrDefault(c => c.Title == model.ClientTitle).ClientId;

                if (client == null)
                {
                    return BadRequest("Не удалось найти клиента после добавления.");
                }

                foreach (var c in model.contacts)
                {
                    Contact contact = new Contact();
                    contact.ContactName = c.Name;
                    contact.PhoneNumber = c.Phone;
                    contact.Email = c.Email;
                    contact.ClientId = clientId;
                    _context.Contacts.Add(contact);
                }
                _context.SaveChanges();
                foreach (var c in model.address)
                {
                    DeliveryAddress address = new ();
                    address.DeliveryAddress1 = c.Address;
                    address.ClientId = clientId;
                    _context.DeliveryAddresses.Add(address);
                }
                _context.SaveChanges();
               
                return Ok("client успешно загружены.");
            }
            finally
            {
                _outputLock.Release();
            }
        }


        [HttpPost]
        [Route("/CurrentProjects/AddRervizit")]
        public async Task<IActionResult> AddRervizit([FromForm] List<RekvizitViewModel> rekvizits)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            await _outputLock.WaitAsync();
            try
            {
                Console.WriteLine("---Method: AddRervizit---");
                Console.WriteLine("files^ " + rekvizits.Count);
                foreach (var item in rekvizits)
                {
                    Console.WriteLine($"проект: {item.ClientTitle}, Имя файла: {item.FilePath?.FileName}");
                    if (item.FilePath != null)
                    {
                        var fileName = item.FilePath.FileName;
                        var uploadsFolder = Path.Combine("wwwroot", item.ClientTitle + "REKVIZITS");
                        Directory.CreateDirectory(uploadsFolder); // Создаём папку, если её ещё нет

                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await item.FilePath.CopyToAsync(stream);
                        }
                        Requisite requisite = new Requisite();
                        requisite.FilePath = filePath;
                        requisite.FileTitle = fileName;
                        requisite.ClientId = _context.Clients.First(c => c.Title == rekvizits[0].ClientTitle).ClientId;
                        _context.Requisites.Add(requisite);
                        _context.SaveChanges();
                    }
                }
                return Ok(new { message = "Файлы загружены" });
            }
            finally
            {
                _outputLock.Release();
            }
        }

        //----------------------------------actions формы создания прооекта---------------------------------------//

        [HttpPost]
        [Route("CurrentProjects/AddProject")]
        public async Task<IActionResult> AddProject([FromForm] ProjectViewModel model)
        {
            Console.WriteLine("----------AddProject-------------");
            await _outputLock.WaitAsync(); // Блокируем доступ к коду
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Message = "Ошибка при добавлении проекта." });
                }

                var project = new Project
                {
                    ProjectName = model.ProjectName,
                    Deadline = model.Deadline,
                    ClientId = model.ClientId,
                    EmployeeId = model.EmployeeId
                };

                _context.Projects.Add(project);
                _context.SaveChanges();

                Console.WriteLine(project);

                return Ok("project data uploaded");
            }
            finally
            {
                _outputLock.Release(); // Разблокируем
            }
        }

        [HttpPost]
        [Route("CurrentProjects/AddItems")]
        public async Task<IActionResult> AddItems([FromForm] List<ItemUploadViewModel> items)
        {
            Console.WriteLine("---Method: AddItems---");
            await _outputLock.WaitAsync(); // Блокируем доступ к коду
            try
            {
                if (items == null || !items.Any())
                {
                    return BadRequest("Не были переданы данные для добавления.");
                }

                int projectId = _context.Projects.FirstOrDefault(p => p.ProjectName == items[0].ProjectName).ProjectId;
                if(projectId == 0)
                {
                    return NotFound();
                }

                var savedItems = new List<Item>();
                foreach (var itemDto in items)
                {
                    if (itemDto.Sketch != null)
                    {
                        //var sanitizedProjectName = itemDto.ProjectName.Replace(" ", "_").Replace(":", "_");
                        //var sanitizedItemName = itemDto.ItemName.Replace(" ", "_").Replace(":", "_");
                        //var fileName = $"{sanitizedProjectName}_{sanitizedItemName}{Path.GetExtension(itemDto.Sketch.FileName)}";
                        var fileName = itemDto.Sketch.FileName;

                        var uploadsFolder = Path.Combine("wwwroot", itemDto.ProjectName + "TestUpload");
                        Directory.CreateDirectory(uploadsFolder); // Создаём папку, если её ещё нет

                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await itemDto.Sketch.CopyToAsync(stream);
                        }
                        var item = new Item
                        {
                            ItemType = itemDto.ItemType,
                            ItemName = itemDto.ItemName,
                            Amount = itemDto.Amount,
                            Price = itemDto.Price,
                            ItemDescription = itemDto.ItemDescription,
                            Materials = itemDto.Materials,
                            Colors = itemDto.Colors,
                            Deadline = itemDto.Deadline,
                            ProjectId = projectId,
                            SketchPath = filePath,                          
                        };
                        savedItems.Add(item);
                    }
                 
                }
                foreach (var savedItem in savedItems)
                {
                    Console.WriteLine(savedItem.ToString());
                    _context.Items.Add(savedItem);
                }
                _context.SaveChanges();
                return Ok("Данные успешно загружены.");
            }

            finally
            {
                _outputLock.Release(); 
            }
        }

        [HttpPost]
        [Route("CurrentProjects/AddProjectFiles")]
        public async Task<IActionResult> AddProjectFiles([FromForm] List<ProjectFileUploadViewModel> files)
        {
           if (!ModelState.IsValid) { return BadRequest(ModelState); }
            await _outputLock.WaitAsync(); // Блокируем доступ к коду
            try
            {
                Console.WriteLine("---Method: AddProjectFiles---");
                Console.WriteLine("files^ " + files.Count);

                int projectId = _context.Projects.FirstOrDefault(p => p.ProjectName == files[0].ProjectName).ProjectId;
                if (projectId == 0)
                {
                    return NotFound();
                }

                foreach (var item in files)
                {                 
                    Console.WriteLine($"проект: {item.ProjectName}, Имя файла: {item.FilePath?.FileName}");
                    if (item.FilePath != null)
                    {                     
                        var fileName = item.FilePath.FileName;
                        var uploadsFolder = Path.Combine("wwwroot", item.ProjectName + "TestFilesUpload");
                        Directory.CreateDirectory(uploadsFolder);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await item.FilePath.CopyToAsync(stream);
                        }
                        ProjectFile projectFile = new ProjectFile();
                        projectFile.ProjectId = projectId;
                        projectFile.FileType = "макет";
                        projectFile.FilePath = filePath;
                        projectFile.FileTitle = fileName;
                        _context.ProjectFiles.Add(projectFile);
                    }
                }
                _context.SaveChanges();
                return Ok(new { message = "Файлы загружены" });
            }
            finally
            {
                _outputLock.Release(); 
            }
        }

        [HttpPost]
        [Route("CurrentProjects/AddProjectDocuments")]
        public async Task<IActionResult> AddProjectDocuments([FromForm] List<ProjectFileUploadViewModel> docs)
        {
            if (!ModelState.IsValid) { return BadRequest(ModelState); }
            await _outputLock.WaitAsync(); // Блокируем доступ к коду
            try
            {
                Console.WriteLine("---Method: AddProjectDocuments---");
                Console.WriteLine("files^ " + docs.Count);

                int projectId = _context.Projects.FirstOrDefault(p => p.ProjectName == docs[0].ProjectName).ProjectId;
                if (projectId == 0)
                {
                    return NotFound();
                }
                foreach (var item in docs)
                {
                    Console.WriteLine($"проект: {item.ProjectName}, Имя файла: {item.FilePath?.FileName}");
                    if (item.FilePath != null)
                    {
                         var fileName = item.FilePath.FileName;
                        var uploadsFolder = Path.Combine("wwwroot", item.ProjectName + "TestDocsUpload");
                        Directory.CreateDirectory(uploadsFolder); 
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await item.FilePath.CopyToAsync(stream);
                        }

                        ProjectFile projectDoc = new ProjectFile();
                        projectDoc.ProjectId = projectId;
                        projectDoc.FilePath = filePath;
                        projectDoc.FileType = "документ";
                        projectDoc.FileTitle = fileName;
                        _context.ProjectFiles.Add(projectDoc);
                    }       
                }
                _context.SaveChanges();
                return Ok(new { message = "Файлы загружены" });
            }
            finally
            {
                _outputLock.Release();
            }
        }

       
      





        [HttpPost]
        [Route("CurrentProjects/AddMaterials")]
        public IActionResult AddMaterials()
        {
            Console.WriteLine("--------------Method: AddMaterials-----------");
            string materialName = Request.Query["Materials"];
            Material material = new();
            material.MaterialName = materialName;
            Console.WriteLine(material.MaterialName);
            _context.Materials.Add(material);
            _context.SaveChanges();

            return Ok("Material успешно загружены.");
        }

        [HttpPost]
        [Route("CurrentProjects/AddColors")]
        public IActionResult AddColors()
        {
            Console.WriteLine("--------------Method: AddColors-----------");
            string colorName = Request.Query["Colors"];
            Color color = new();
            color.ColorName = colorName;
            Console.WriteLine(color.ColorName);
            _context.Colors.Add(color);
            _context.SaveChanges();

            return Ok("Color успешно загружены.");
        }

        [HttpGet]
        [Route("CurrentProjects/GetMaterials")]
        public IActionResult GetMaterials() { 
        return Json(_context.Materials);
        }
        [HttpGet]
        [Route("CurrentProjects/GetColors")]
        public IActionResult GetColors()
        {
            return Json(_context.Colors);
        }

        [HttpGet]
        [Route("CurrentProjects/GetClients")]
        public IActionResult GetClients()
        {
            return Json(_context.Clients);
        }

        [HttpGet]
        [Route("CurrentProjects/GetEmployees")]
        public IActionResult GetEmployees()
        {
            return Json(_context.Employees);
        }
    }
}
