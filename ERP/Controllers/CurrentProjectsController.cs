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
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.CodeAnalysis;



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
        public IActionResult AddPhotosPartial() { return PartialView("_AddPhotosPartial"); }
        public IActionResult TaskViewPartial() { return PartialView("_TaskViewPartial"); }
        public IActionResult CreateProjectPartial() { return PartialView("_CreateProjectPartial"); }
        //public IActionResult ProjectCardPartial() { return PartialView("_ProjectCardPartial"); }

        [HttpGet("GetEmployeesWithProjects")]
        public IActionResult GetEmployeesWithProjects()
        {

            var employees = _context.Employees
                .Include(e => e.Projects)
                    .ThenInclude(p => p.Items).ToList()
                .Select(e => new
                {
                    e.EmployeeId,
                    EmployeeName = e.EmployeeName.Contains(' ') ? $"{e.EmployeeName.Split(' ')[0]}  {e.EmployeeName.Split(' ')[2][0]}."
                        : e.EmployeeName,
                    e.Position,
                    Projects = e.Projects.Select(p => new
                    {
                        p.ProjectId,
                        p.ProjectName,
                        p.Deadline,
                        Quantity = p.Items.Sum(i => i.Amount),
                        SketchPath = p.Items.FirstOrDefault().SketchPath
                    })
                }).ToList();


            Console.WriteLine("=== Employees with Projects ===");
            foreach (var employee in employees)
            {
                Console.WriteLine($"EmployeeId: {employee.EmployeeId}, Name: {employee.EmployeeName}, Position: {employee.Position}");
                Console.WriteLine("Projects:");
                foreach (var project in employee.Projects)
                {
                    Console.WriteLine($"\tProjectId: {project.ProjectId}, Name: {project.ProjectName}, Deadline: {project.Deadline}, SketchPath: {project.SketchPath}");
                }
                Console.WriteLine("===============================");
            }

            return Ok(employees);
        }

        [HttpGet("GetProjectsQuery")]
        public IActionResult GetProjectsQuery()
        {
            var projectsQuery = _context.Projects
                .Include(e => e.Items)
                .Where(p => p.EmployeeId == null)
                .Select(p => new
                {
                    p.ProjectId,
                    p.ProjectName,
                    p.Deadline,
                    p.EmployeeId,
                    Quantity = p.Items.Sum(i => i.Amount),
                    SketchPath = p.Items.FirstOrDefault().SketchPath
                }).ToList();
            return Ok(projectsQuery);
        }
        [HttpGet("LoadProjectCard")]
        public async Task<IActionResult> LoadProjectCard(int id)
        {
            Console.WriteLine("------------------------LoadProjectCard--------------------------");
            var project = await _context.Projects
             .Include(p => p.Client)
             .Include(p => p.Employee)
             .Include(p => p.Items)
             .Include(p => p.ProjectFiles)
             .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
            {
                return NotFound();
            }

            var viewModel = new ProjectCardViewModel
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                ProjectColor = project.Color,
                PaymentDate = project.PaymentDate,
                EventDate = project.EventDate,
                Deadline = project.Deadline,
                ClientName = project.Client?.Title,
                EmployeeName = project.Employee?.EmployeeName,
                PaymentTotal = project.PaymentTotal,
                AdvanceRate = project.AdvanceRate,
                Description = project.Description,
                Journal = project.Journal,
                Items = project.Items.Select(i => new ItemCardViewModel
                {
                    ItemType = i.ItemType,
                    ItemName = i.ItemName,
                    SketchPath = i.SketchPath,
                    Amount = i.Amount,
                    ItemDescription = i.ItemDescription
                }).ToList(),
                Layouts = project.ProjectFiles.Where(f => f.FileType == "макет").Select(f => new FileCardViewModel
                {
                    FileTitle = f.FileTitle,
                    FilePath = f.FilePath,
                    UploadedAt = f.UploadedAt
                }).ToList(),
                Documents = project.ProjectFiles.Where(f => f.FileType == "документ").Select(f => new FileCardViewModel
                {
                    FileTitle = f.FileTitle,
                    FilePath = f.FilePath,
                    UploadedAt = f.UploadedAt
                }).ToList(),
                Gallery = project.ProjectFiles.Where(f => f.FileType == "фото").Select(f => new FileCardViewModel
                {
                    FileTitle = f.FileTitle,
                    FilePath = f.FilePath,
                    UploadedAt = f.UploadedAt
                }).ToList(),
                JournalPhotos = project.ProjectFiles.Where(f => f.FileType == "журналФото").Select(f => new FileCardViewModel
                {
                    FileTitle = f.FileTitle,
                    FilePath = f.FilePath,
                    UploadedAt = f.UploadedAt
                }).ToList()
            };

            Console.WriteLine(viewModel);

            return PartialView("_ProjectCardPartial", viewModel); // Верните представление как PartialView
        }
        [HttpGet("GetItemsByProjectId")]
        public IActionResult GetItemsByProjectId()
        {
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            var items = _context.Items.Where(i => i.ProjectId == id)
                                        .Select(item => new
                                        {
                                            id = item.ItemId,
                                            title = item.ItemName
                                        });

            foreach (var item in items)
            {
                Console.WriteLine(item.title + item.id);
            }

            return Json(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotos([FromForm] List<AddPhotosViewModel> photos)
        {
            Console.WriteLine("AddPhotos; " + photos.Count);
            Console.WriteLine();

            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == photos[0].projectId);
            var client = _context.Clients.FirstOrDefault(c => c.ClientId == project.ClientId);
            var sanitizedProjectName = project.ProjectName.Replace(" ", "_").Replace(":", "_");
            var sanitizedClientName = client.Title.Replace(" ", "_").Replace(":", "_");

            foreach (var item in photos)
            {
                Console.WriteLine(item);

                if (item.photo != null)
                {
                    var fileName = item.photo.FileName.Replace(" ", "_").Replace(":", "_");
                    var uploadsFolder = Path.Combine(sanitizedClientName, sanitizedProjectName, item.fileType);
                    Directory.CreateDirectory(Path.Combine("wwwroot", uploadsFolder));
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(Path.Combine("wwwroot", filePath), FileMode.Create))
                    {
                        await item.photo.CopyToAsync(stream);
                    }
                    ProjectFile projectFile = new ProjectFile();
                    projectFile.ProjectId = item.projectId;
                    projectFile.ItemId = item.itemPointer == 0 ? null : item.itemPointer;
                    projectFile.FileType = item.fileType;
                    projectFile.FilePath = "\\" + filePath;
                    projectFile.FileTitle = fileName;
                    _context.ProjectFiles.Add(projectFile);
                }

            }

            _context.SaveChanges();
            return Ok();
        }

        [HttpPut("PutProjectDescription")]
        public IActionResult PutProjectDescription([FromBody] JsonElement data)
        {
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            data.TryGetProperty("description", out JsonElement descriptionElement);
            var description = descriptionElement.GetString();
            //  Console.WriteLine("PutProjectDescription: " + description + ", ID: " + id);
            var project = _context.Projects.Find(id);
            if (project == null)
            {
                return NotFound("Проект не найден.");
            }
            project.Description = description;
            _context.SaveChanges();
            return Ok(new { message = "Описание обновлено успешно." });
        }

        [HttpPut("PutProjectJournal")]
        public IActionResult PutProjectJournal([FromBody] JsonElement data)
        {
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            data.TryGetProperty("description", out JsonElement descriptionElement);
            var description = descriptionElement.GetString();
            var project = _context.Projects.Find(id);
            if (project == null)
            {
                return NotFound("Проект не найден.");
            }
            project.Journal = description;
            _context.SaveChanges();
            return Ok(new { message = "Описание обновлено успешно." });
        }

        [HttpPut("SetProjectColor")]
        public IActionResult SetProjectColor([FromBody] JsonElement data)
        {
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            data.TryGetProperty("color", out JsonElement descriptionElement);
            var color = descriptionElement.GetString();

            var project = _context.Projects.Find(id);
            if (project == null)
            {
                return NotFound("Проект не найден.");
            }
            project.Color = color;
            _context.SaveChanges();
            return Ok(new { message = "color обновлено успешно." });
        }

        public async Task<IActionResult> Details(int id)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Employee)
                .Include(p => p.Items)
                .Include(p => p.ProjectFiles)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
            {
                return NotFound();
            }

            var viewModel = new ProjectCardViewModel
            {
                ProjectId = project.ProjectId,
                ProjectName = project.ProjectName,
                PaymentDate = project.PaymentDate,
                EventDate = project.EventDate,
                Deadline = project.Deadline,
                ClientName = project.Client?.Title,
                EmployeeName = project.Employee?.EmployeeName,
                PaymentTotal = project.PaymentTotal,
                AdvanceRate = project.AdvanceRate,
                Description = project.Description,
                Journal = project.Journal,
                Items = project.Items.Select(i => new ItemCardViewModel
                {
                    ItemType = i.ItemType,
                    ItemName = i.ItemName,
                    SketchPath = i.SketchPath,
                    Amount = i.Amount,
                    ItemDescription = i.ItemDescription
                }).ToList(),
                Layouts = project.ProjectFiles.Where(f => f.FileType == "макет").Select(f => new FileCardViewModel
                {
                    FileTitle = f.FileTitle,
                    FilePath = f.FilePath,
                    UploadedAt = f.UploadedAt
                }).ToList(),
                Documents = project.ProjectFiles.Where(f => f.FileType == "документ").Select(f => new FileCardViewModel
                {
                    FileTitle = f.FileTitle,
                    FilePath = f.FilePath,
                    UploadedAt = f.UploadedAt
                }).ToList(),
                Gallery = project.ProjectFiles.Where(f => f.FileType == "фото").Select(f => new FileCardViewModel
                {
                    FileTitle = f.FileTitle,
                    FilePath = f.FilePath,
                    UploadedAt = f.UploadedAt
                }).ToList(),
                JournalPhotos = project.ProjectFiles.Where(f => f.FileType == "журналФото").Select(f => new FileCardViewModel
                {
                    FileTitle = f.FileTitle,
                    FilePath = f.FilePath,
                    UploadedAt = f.UploadedAt
                }).ToList()
            };

            return View(viewModel);
        }


        //----------------------------------контроллеры обработчики---------------------------------------//

        [HttpPost]
        public async Task<IActionResult> AddEmployee(CreateEmployeeViewModel model)
        {
            Console.WriteLine(model);
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
                    { Console.WriteLine($"Поле: {field}, Ошибка: {error.ErrorMessage}"); }
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
                int clientId = _context.Clients.FirstOrDefault(c => c.Title == model.ClientTitle).ClientId;

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
                    DeliveryAddress address = new();
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
                var sanitizedClientName = rekvizits[0].ClientTitle.Replace(" ", "_").Replace(":", "_");
                var clientRekvizitFolder = Path.Combine(sanitizedClientName, "rekvizits");

                Directory.CreateDirectory(Path.Combine("wwwroot", clientRekvizitFolder));

                foreach (var item in rekvizits)
                {
                    Console.WriteLine($"проект: {item.ClientTitle}, Имя файла: {item.FilePath?.FileName}");
                    if (item.FilePath != null)
                    {
                        var fileName = item.FilePath.FileName;

                        var filePath = Path.Combine(clientRekvizitFolder, fileName);
                        using (var stream = new FileStream(Path.Combine("wwwroot", filePath), FileMode.Create))
                        {
                            await item.FilePath.CopyToAsync(stream);
                        }
                        Requisite requisite = new Requisite();
                        requisite.FilePath = "\\" + filePath;
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

                var project = new ERP.Models.Project
                {
                    ProjectName = model.ProjectName,
                    Deadline = model.Deadline,
                    ClientId = model.ClientId,
                    EmployeeId = model.EmployeeId != 0 ? model.EmployeeId : null,
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
            Console.WriteLine("-------------Method: AddItems----------------");
            await _outputLock.WaitAsync(); // Блокируем доступ к коду
            try
            {
                if (items == null || !items.Any())
                {
                    return BadRequest("Не были переданы данные для добавления.");
                }

                var project = _context.Projects.FirstOrDefault(p => p.ProjectName == items[0].ProjectName);

                if (project == null)
                {
                    return NotFound();
                }
                int projectId = project.ProjectId;
                string clientTitle = _context.Clients.FirstOrDefault(c => c.ClientId == project.ClientId).Title;
                string client = clientTitle.Replace(" ", "_").Replace(":", "_");



                var savedItems = new List<Item>();
                var sanitizedProjectName = items[0].ProjectName.Replace(" ", "_").Replace(":", "_");
                decimal? projectPrice = 0;
                foreach (var itemDto in items)
                {
                    if (itemDto.Sketch != null)
                    {
                        var sanitizedItemName = itemDto.Sketch.FileName.Replace(" ", "_").Replace(":", "_");
                        var uploadsFolder = Path.Combine(client, sanitizedProjectName, "Sketches");
                        Directory.CreateDirectory(Path.Combine("wwwroot", uploadsFolder)); // Создаём папку, если её ещё нет

                        var filePath = Path.Combine(uploadsFolder, sanitizedItemName);
                        using (var stream = new FileStream(Path.Combine("wwwroot", filePath), FileMode.Create))
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
                            SketchPath = "\\" + filePath,
                        };
                        savedItems.Add(item);
                        projectPrice += item.Price * item.Amount;
                    }
                }
                foreach (var savedItem in savedItems)
                {
                    Console.WriteLine(savedItem.ToString());
                    _context.Items.Add(savedItem);
                }
                Console.WriteLine($"project {project.ProjectName} payment total: " + projectPrice);
                project.PaymentTotal = projectPrice;
                Console.WriteLine(" -----------------project payment total: " + project.PaymentTotal);
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

                if (files.Count == 0)
                {
                    return Ok("нет файлов проекта");
                }

                var project = _context.Projects.FirstOrDefault(p => p.ProjectName == files[0].ProjectName);
                int projectId = project.ProjectId;
                if (project == null)
                {
                    return BadRequest("нет проекта");
                }
                string clientTitle = _context.Clients.FirstOrDefault(c => c.ClientId == project.ClientId).Title;
                string client = clientTitle.Replace(" ", "_").Replace(":", "_");


                var sanitizedProjectName = files[0].ProjectName.Replace(" ", "_").Replace(":", "_");
                foreach (var item in files)
                {
                    Console.WriteLine($"проект: {item.ProjectName}, Имя файла: {item.FilePath?.FileName}");
                    if (item.FilePath != null)
                    {
                        var fileName = item.FilePath.FileName.Replace(" ", "_").Replace(":", "_");
                        var uploadsFolder = Path.Combine(client, sanitizedProjectName, "files");
                        Directory.CreateDirectory(Path.Combine("wwwroot", uploadsFolder));
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(Path.Combine("wwwroot", filePath), FileMode.Create))
                        {
                            await item.FilePath.CopyToAsync(stream);
                        }
                        ProjectFile projectFile = new ProjectFile();
                        projectFile.ProjectId = projectId;
                        projectFile.FileType = "макет";
                        projectFile.FilePath = "\\" + filePath;
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
                Console.WriteLine("docs^ " + docs.Count);

                if (docs.Count == 0)
                {
                    return Ok("нет документов проекта");
                }

                var project = _context.Projects.FirstOrDefault(p => p.ProjectName == docs[0].ProjectName);
                if (project == null)
                {
                    return BadRequest("нет проекта");
                }
                int projectId = project.ProjectId;

                string clientTitle = _context.Clients.FirstOrDefault(c => c.ClientId == project.ClientId).Title;
                string client = clientTitle.Replace(" ", "_").Replace(":", "_");

                var sanitizedProjectName = docs[0].ProjectName.Replace(" ", "_").Replace(":", "_");
                foreach (var item in docs)
                {
                    Console.WriteLine($"проект: {item.ProjectName}, Имя файла: {item.FilePath?.FileName}");
                    if (item.FilePath != null)
                    {
                        var fileName = item.FilePath.FileName.Replace(" ", "_").Replace(":", "_");
                        var uploadsFolder = Path.Combine(client, sanitizedProjectName, "documents");
                        Directory.CreateDirectory(Path.Combine("wwwroot", uploadsFolder));
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var stream = new FileStream(Path.Combine("wwwroot", filePath), FileMode.Create))
                        {
                            await item.FilePath.CopyToAsync(stream);
                        }

                        ProjectFile projectDoc = new ProjectFile();
                        projectDoc.ProjectId = projectId;
                        projectDoc.FilePath = "\\" + filePath;
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

        [HttpGet]
        [Route("GetTasksByEmployeeId")]
        public IActionResult GetTasksByEmployeeId()
        {
            Console.WriteLine("GetTasksByEmployeeId");
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            var tasks = _context.Tasks.Where(t => t.EmployeeId == id)
                                       .Include(p => p.Project)
                                        .Select(t => new
                                        {
                                            t.TaskId,
                                            t.ProjectId,
                                            t.Deadline,
                                            t.Description,
                                            projectColor = t.Project.Color
                                        });
            return Json(tasks);
        }

        [HttpGet]
        [Route("GetCurrentProjectsByEmployeeId")]
        public IActionResult GetCurrentProjectsByEmployeeId()
        {
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            var projects = _context.Projects.Where(p => p.EmployeeId == id && (p.IsArchived == false || p.IsArchived == null));
            return Json(projects);
        }

       // [HttpDelete]
        [Route("/CurrentProjects/DeleteTask")]
        public IActionResult DeleteTask()
        {
            Console.WriteLine("DeleteTask");
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            TaskItem task = _context.Tasks.First(t => t.TaskId == id);
            if (task == null) {
                return BadRequest("task not found");
            }
            _context.Tasks.Remove(task);
            _context.SaveChanges();
            return Ok();
        }


        [HttpPost]
        [Route("/CurrentProjects/AddTask")]
        public IActionResult AddTask([FromForm] TaskItem data)
        {
            Console.WriteLine(data);
            Console.WriteLine("AddTask");
            TaskItem task;
            if (data.TaskId == 0)
            {
                task = new TaskItem();
                task.ProjectId = data.ProjectId == 0 ? null : data.ProjectId;
                task.EmployeeId = data.EmployeeId;
                task.Deadline = data.Deadline;
                task.Description = data.Description;
                _context.Tasks.Add(task);
                _context.SaveChanges();
            }
            else
            {
                task = _context.Tasks.First(t => t.TaskId == data.TaskId);
                task.ProjectId = data.ProjectId == 0 ? null : data.ProjectId;
                task.Description = data.Description;
            }
            _context.SaveChanges();
            var taskItem = _context.Tasks.Where(t => t.TaskId == task.TaskId)
                                        .Include(p => p.Project)
                                        .Select(t => new
                                        {
                                            t.TaskId,
                                            t.ProjectId,
                                            t.Deadline,
                                            t.Description,
                                            projectColor = t.Project.Color 
                                        }).FirstOrDefault();

            return Json(taskItem);
        }

    }
}
