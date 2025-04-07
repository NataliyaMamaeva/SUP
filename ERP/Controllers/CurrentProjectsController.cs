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
using System.Linq;
using Microsoft.Build.Evaluation;
//using ERP.Data.Migrations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;



namespace ERP.Controllers
{
    [Authorize]

    public class CurrentProjectsController : Controller
    {
        private static readonly SemaphoreSlim _outputLock = new SemaphoreSlim(1, 1);
        private readonly ErpContext _context;
        private readonly UserManager<ErpUser> _userManager;
        private readonly IDataProtector _protector;
        public CurrentProjectsController(UserManager<ErpUser> userManager, ErpContext context, IDataProtectionProvider provider)
        {
            _userManager = userManager;
            _context = context;
            _protector = provider.CreateProtector("");
        }

        //------------------------------------------------------view без модели--------------------------------------------------//
        public ActionResult Index() { return View(); }
        public IActionResult CreateEmployeePartial() {

            var bosses = _context.Employees.Where(e => e.IsFired != true).ToList();      
            var positions = new List<string> { "Boss", "Master", "Designer", "PhoneManager" };
            ViewBag.Bosses = bosses;
            ViewBag.Positions = positions;

            return PartialView("_CreateEmployeePartial");
        }
        public IActionResult CreateClientPartial() { return PartialView("_CreateClientPartial"); }
       
        public IActionResult AddPhotosPartial() { return PartialView("_AddPhotosPartial"); }
        public IActionResult TaskViewPartial() { return PartialView("_TaskViewPartial"); }

        [HttpGet]
        [Route("/CurrentProjects/CreateProjectPartial")]
        public IActionResult CreateProjectPartial() { return PartialView("_CreateProjectPartial"); }

        //------------------------------------------------------Который юзер--------------------------------------------------//

        [HttpGet]
        [Route("api/GetCurrentUser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            var isBoss = await _userManager.IsInRoleAsync(user, "Boss");
            Console.WriteLine("is Boss: " + isBoss + "; user.EmployeeId: " + user.EmployeeId);
            return Ok(new
            {
                CurrentUserId = user.EmployeeId,
                IsBoss = isBoss
            });
        }

        //===============================================начальная загрузка проекты=======================================================//

        [HttpGet("GetEmployeesWithProjects")]
        public IActionResult GetEmployeesWithProjects()
        {

            var employees = _context.Employees
                .Where(e => e.IsFired != true)
                    .Include(e => e.Projects)
                        .ThenInclude(p => p.Items)
                        .AsSplitQuery()
                        .ToList()
                .Select(e => new
                {
                    e.EmployeeId,
                    EmployeeName = e.EmployeeName.Contains(' ') ? $"{e.EmployeeName.Split(' ')[0]}  {e.EmployeeName.Split(' ')[2][0]}."
                        : e.EmployeeName,
                    e.Position,
                    Projects = e.Projects
                            .Where(p => p.IsArchived != true)
                            .Select(p => new
                            {
                                p.ProjectId,
                                p.ProjectName,
                                p.Deadline,
                                Quantity = p.Items?.Sum(i => i.Amount),
                                SketchPath = p.Items?.FirstOrDefault()?.SketchPath
                            }) }).ToList();


            //Console.WriteLine("=== Employees with Projects ===");
            //foreach (var employee in employees)
            //{
            //    Console.WriteLine($"EmployeeId: {employee.EmployeeId}, Name: {employee.EmployeeName}, Position: {employee.Position}");
            //    Console.WriteLine("Projects:");
            //    foreach (var project in employee.Projects)
            //    {
            //        Console.WriteLine($"\tProjectId: {project.ProjectId}, Name: {project.ProjectName}, Deadline: {project.Deadline}, SketchPath: {project.SketchPath}");
            //    }
            //    Console.WriteLine("===============================");
            //}

            return Ok(employees);
        }

        [HttpGet("GetProjectsQuery")]
        public IActionResult GetProjectsQuery()
        {
            var projectsQuery = _context.Projects
                .Include(e => e.Items)
                .AsSplitQuery()
                .Where(p => p.EmployeeId == null && p.IsArchived != true)
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

        //====================================================================Employees=======================================================//


        [HttpGet]
        [Route("CurrentProjects/GetEmployees")]
        public IActionResult GetEmployees()
        {
            return Json(_context.Employees.Where(e => e.IsFired != true));
        }

        //[HttpPost]
        //public async Task<IActionResult> AddEmployee(CreateEmployeeViewModel model)
        //{
        //    Console.WriteLine(model);
        //    if (!ModelState.IsValid)
        //    {
        //       // return BadRequest("Некорректные данные. Проверьте форму.");
        //    }

        //    var existingUser = await _userManager.FindByEmailAsync(model.Email);
        //    if (existingUser != null)
        //    {
        //        return Conflict(new { Message = "Пользователь с таким email уже существует." });
        //    }
        //    var employee = new Employee
        //    {
        //        EmployeeName = model.EmployeeName,
        //        PhoneNumber = model.PhoneNumber,
        //        BossId = model.BossId ?? null,
        //        Passport = model.Passport,
        //        Position = model.Position ?? "Master",
        //        Email = model.Email
        //    };
        //    _context.Employees.Add(employee);
        //    await _context.SaveChangesAsync();
        //    var user = new ErpUser
        //    {
        //        UserName = model.Email,
        //        Email = model.Email,
        //        EmployeeId = employee.EmployeeId
        //    };
        //    var result = await _userManager.CreateAsync(user, model.Password);


        //    if (!result.Succeeded)
        //    {
        //        _context.Employees.Remove(employee);
        //        await _context.SaveChangesAsync();
        //        return BadRequest(new { Message = "Не удалось создать пользователя.", Errors = result.Errors });
        //    }
        //    return RedirectToAction("Index");
        //}



        public IActionResult LoadEmployeeCard(int id)
        {
            var employee = _context.Employees
                .Include(e => e.Boss)
                .FirstOrDefault(e => e.EmployeeId == id);

            Console.WriteLine(employee);

            if (employee == null)
            {
                return NotFound();
            }

            string decryptedPassport;
            try
            {
                decryptedPassport = _protector.Unprotect(employee.Passport);
            }
            catch (CryptographicException)
            {
                Console.WriteLine("Ошибка расшифровки");
                decryptedPassport = employee.Passport;
            }

            var model = new Employee
            {
                EmployeeId = id,
                PhoneNumber = employee.PhoneNumber,
                Passport = decryptedPassport,
                Email = employee.Email,
                EmployeeName = employee.EmployeeName
            };

           
            var bosses = _context.Employees.ToList();           
            var positions = new List<string> { "Boss", "Master", "Designer", "PhoneManager" };
            ViewBag.Bosses = bosses;
            ViewBag.Positions = positions;

            return PartialView("_EmployeeCardPartial", model);
        }

        [HttpPost]
        [Route("CurrentProjects/UpdateEmployee")]
        public async Task <IActionResult> UpdateEmployee([FromForm] Employee updatedEmployee)
        {
            Console.WriteLine(updatedEmployee);
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeId == updatedEmployee.EmployeeId);
            if (employee == null)
                return Json(new { message = "Сотрудник не найден" });

            int? exBossId = employee.BossId;
            employee.PhoneNumber = updatedEmployee.PhoneNumber;
            employee.Passport = _protector.Protect(updatedEmployee.Passport);
            employee.Email = updatedEmployee.Email;
            if(updatedEmployee.Position != null)
                employee.Position = updatedEmployee.Position;
            if (updatedEmployee.BossId != null)
                employee.BossId = updatedEmployee.BossId;
            await _context.SaveChangesAsync();
            
            if (updatedEmployee.BossId != exBossId)
            {
                var projects = await _context.Projects.Where(p => p.EmployeeId == employee.EmployeeId).ToListAsync();
                if (projects.Any())
                    foreach (var project in projects)
                    {
                        Console.WriteLine("updatedEmployee.BossId != exBossId");
                        Console.WriteLine(project.ProjectName);
                        await ProjectSalaryRecount(project.ProjectId, employee.EmployeeId);
                    }                 
            }
            return Redirect("Index");
        }
       
        [HttpGet]
        [Route("CurrentProjects/GetPositions")]
        public IActionResult GetPositions()
        {
            List<string> positions = new List<string>() { "Boss", "Master", "Designer", "PhoneManager" };
            return Json(positions);
        }

        public async Task<IActionResult> FireEmployee(int id)
        {
            Console.WriteLine("FireEmployee");
            Console.WriteLine(id);

            var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == id);
            if (employee == null)
            {
                return NotFound("Сотрудник не найден");
            }

            // Помечаем сотрудника как уволенного
            employee.IsFired = true;

            // Находим пользователя в ASP.NET Identity
            var user = await _userManager.FindByEmailAsync(employee.Email);
            if (user != null)
            {
                // Удаляем пользователя из системы Identity
                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest("Ошибка при удалении пользователя.");
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Сотрудник успешно уволен.");
        }


        //============================================================Salary================================================================//

        public IActionResult  LoadSalary() {
            Console.WriteLine("LoadSalary");
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            var currentProjects = _context.Projects.Where(p => p.EmployeeId == id && p.IsArchived != true).ToList();
            var currentSalaries = _context.SalaryEmployeeMonths.Include(s => s.ProjectPayments).ThenInclude(pp => pp.Project)
                                        .Where(s => s.EmployeeId == id && s.isClosed != true).ToList();
            var projectIds = currentProjects.Select(p => p.ProjectId).ToHashSet();

            foreach (var salary in currentSalaries)
            {
                var removedPayments = salary.ProjectPayments
                    .Where(pp => pp.ProjectId.HasValue && !projectIds.Contains(pp.ProjectId.Value))
                    .ToList();

                foreach (var removedPayment in removedPayments)
                {                
                    if (removedPayment.isStagerAdd != true)                    
                        removedPayment.SalaryId = null;
                }
                _context.Update(salary);
            }

            var newPayments = _context.ProjectPayments
                 .Where(pp => pp.SalaryId == null && pp.ProjectId.HasValue && projectIds.Contains(pp.ProjectId.Value))
                 .ToList();

            foreach (var newPayment in newPayments)
            {
                var salary = _context.SalaryEmployeeMonths.FirstOrDefault(s => s.EmployeeId == id && s.MonthPointer == newPayment.MonthPointer);
                if (salary == null)
                {
                    SalaryEmployeeMonth s = new SalaryEmployeeMonth()
                    {
                        EmployeeId = id,
                        FixSalary = 20000,
                        MonthPointer = newPayment.MonthPointer,
                        isClosed = false
                    };
                    _context.SalaryEmployeeMonths.Add(s);
                    _context.SaveChanges();
                    newPayment.SalaryId = s.SalaryId;
                }
                else if (salary.isClosed != true)
                {
                    newPayment.SalaryId = salary.SalaryId;
                }            
            }

            var currentSalariesCount = _context.SalaryEmployeeMonths.Include(s => s.ProjectPayments).Where(s => s.EmployeeId == id && s.isClosed != true).ToList();
            foreach (var salary in currentSalariesCount)
            {
                decimal totalAmount = salary.ProjectPayments.Sum(pp => pp.Amount ?? 0);
                totalAmount -= salary.ProjectPayments.Sum(pp => pp.Punishment ?? 0);
                Console.WriteLine("totalAmount: " + totalAmount);
                salary.FinallyAmount = totalAmount + salary.FixSalary;
            }
            _context.SaveChanges();

            var model = _context.SalaryEmployeeMonths
                                     .Where(s => s.EmployeeId == id)
                                     .Include(p => p.ProjectPayments)
                                     .ThenInclude(pr => pr.Project)
                                     .Select(s => new SalaryViewModel
                                     {
                                         SalaryId = s.SalaryId,
                                         EmployeeId = s.EmployeeId ?? 0,
                                         MonthPointer = s.MonthPointer,
                                         IsClosed = s.isClosed,
                                         FixSalary = s.FixSalary,
                                         FinallyAmount = s.FinallyAmount ?? 0, 
                                         ProjectPayments = s.ProjectPayments.Select(pp => new ProjectPaymentViewModel
                                         {
                                             projectPaymentId = pp.ProjectPaymentId,
                                             isStagerAdd = pp.isStagerAdd,
                                             projectId = pp.Project.ProjectId,
                                             projectTitle = pp.Project.ProjectName,
                                             partNumber = pp.PartNumber,
                                             partsCount = pp.PartsCount,
                                             amount = pp.Amount ?? 0,
                                             punishment = pp.Punishment,
                                             punishmentDescription = pp.PunishmentDescription
                                         }).ToList()
                                     })
                                     .ToList();

            return PartialView("_SalaryPartial", model);           
        }

        [HttpPost]
        [Route("/CurrentProjects/SavePunishments")]
        public IActionResult SavePunishments([FromBody] List<ProjectPaymentUpdateModel> updates)
        {
            if (!ModelState.IsValid)
            {
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        Console.WriteLine($"Key: {modelStateKey}, Error: {error.ErrorMessage}");
                    }
                }
            }
            // Console.WriteLine("updates.Count: " + updates.Count);
            //var employee = _context.Employees.FirstOrDefault(e => e.EmployeeId == updates[0].EmployeeId);
            //bool hasBoss = employee != null && employee.BossId != null;
          
            foreach (var update in updates)
            {
                var projectPayment = _context.ProjectPayments.Find(update.ProjectPaymentId);
                //Console.WriteLine(projectPayment);
                if (projectPayment != null)
                {
                    if (update.Punishment != null)
                    {                 
                        projectPayment.Punishment = update.Punishment;
                        projectPayment.PunishmentDescription = update.PunishmentDescription;        
                    }
                }
            }
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        [Route("/CurrentProjects/CloseMonth")]
        public IActionResult CloseMonth(int id)
        {
            Console.WriteLine("CloseMonth: " + id);
            SalaryEmployeeMonth month = _context.SalaryEmployeeMonths.FirstOrDefault(s => s.SalaryId  == id);
            if (month == null)
            {
                return BadRequest("нету");
            }
            month.isClosed = true;
            _context.SaveChanges();
            return Ok();
        }

        public async Task ProjectSalaryRecount(int projectId, int employeeId)
        {
            Console.WriteLine("?????зашли в ProjectSalaryRecount с проектом :" + projectId + "и сотрудничком: " + employeeId);

            var employee = await _context.Employees.FirstOrDefaultAsync(i => i.EmployeeId == employeeId);
            if (employee == null)
            {
               // Console.WriteLine("просто обнуляем стажёрскую оплату ");
                var StagerPP1 = await _context.ProjectPayments.Where(pp => pp.ProjectId == projectId && pp.isStagerAdd == true).ToListAsync();
                if (StagerPP1.Any())
                {
                    foreach (var p in StagerPP1)
                    {
                       // Console.WriteLine("000==========удаляю стажерскую оплату для пректа " + p.ProjectId);
                        _context.ProjectPayments.Remove(p);
                    }
                        
                    await _context.SaveChangesAsync();
                }
                return;
            }
            

            var StagerPP2 = await _context.ProjectPayments.Where(pp => pp.ProjectId == projectId && pp.isStagerAdd == true).ToListAsync();
            if (StagerPP2.Any())
            {
                foreach (var p in StagerPP2)
                {
                   // Console.WriteLine("-----------удаляю стажерскую оплату для пректа " + p.ProjectId);
                    _context.ProjectPayments.Remove(p);
                }
                   
                await _context.SaveChangesAsync();
            }

            var project = await _context.Projects.FirstOrDefaultAsync(i => i.ProjectId == projectId);
            if (project == null) {
                Console.WriteLine("ProjectSalaryRecount: Project is NULL");
                return;
            }

            await SelfSalaryProjectRecount(project, employee);
            await TaskUpdateOnProjectGive(project, employee);
            
            if (employee.BossId != null)
            {
                await StagerSalaryProjectRecount(project, employee);
            }    
        }

        async Task SelfSalaryProjectRecount(ERP.Models.Project project, Employee employee)
        {
            int projectId = project.ProjectId;
            int employeeId = employee.EmployeeId;
            Console.WriteLine($"self salary recount. employee {employee.EmployeeName} | project {project.ProjectName}");
            project.EmployeeId = employeeId;
            // var isPaymentExist = await _context.ProjectPayments.FirstOrDefaultAsync(pp => pp.ProjectId == projectId);
            var prpays = _context.ProjectPayments.Where(pp => pp.ProjectId == projectId);
            _context.ProjectPayments.RemoveRange(prpays);

          //  if (isPaymentExist == null)
           // {
                var monthCount = ((project.Deadline.Year - project.PaymentDate.Year) * 12) + (project.Deadline.Month - project.PaymentDate.Month);
                Console.WriteLine("monthCount: " + monthCount);
                if (monthCount == 0)
                {
                    ProjectPayment projectPayment = new ProjectPayment();
                    projectPayment.PartsCount = null;
                    projectPayment.PartNumber = null;
                    projectPayment.ProjectId = projectId;
                    projectPayment.Amount = project.EmployeePayment;
                    projectPayment.MonthPointer = new DateOnly(project.Deadline.Year, project.Deadline.Month, 1);
                    var salary = await _context.SalaryEmployeeMonths.FirstOrDefaultAsync(s => s.EmployeeId == employeeId
                                                                    && s.MonthPointer == new DateOnly(project.Deadline.Year, project.Deadline.Month, 1));

                    if (salary == null)
                    {
                        SalaryEmployeeMonth s = new SalaryEmployeeMonth()
                        {
                            EmployeeId = employeeId,
                            FixSalary = 20000,
                            MonthPointer = new DateOnly(project.Deadline.Year, project.Deadline.Month, 1),
                            isClosed = false
                        };
                        _context.SalaryEmployeeMonths.Add(s);
                        _context.SaveChanges();
                        projectPayment.SalaryId = s.SalaryId;
                    }
                    else if (!salary.isClosed)
                    {
                        projectPayment.SalaryId = salary.SalaryId;
                    }
                    _context.ProjectPayments.Add(projectPayment);
                }
                else
                {
                    monthCount++;
                    for (int i = 1; i <= monthCount; i++)
                    {
                        DateOnly month = new DateOnly(project.PaymentDate.Year, project.PaymentDate.Month, 1).AddMonths(i - 1);
                        ProjectPayment projectPayment = new ProjectPayment();
                        projectPayment.PartsCount = monthCount;
                        projectPayment.ProjectId = projectId;
                        projectPayment.PartNumber = i;
                        projectPayment.Amount = project.EmployeePayment / monthCount;
                        projectPayment.MonthPointer = month;
                        var salary = await _context.SalaryEmployeeMonths.FirstOrDefaultAsync(s => s.EmployeeId == employeeId
                                                                                    && s.MonthPointer == month);
                        if (salary == null)
                        {
                            SalaryEmployeeMonth s = new SalaryEmployeeMonth()
                            {
                                EmployeeId = employeeId,
                                FixSalary = 20000,
                                MonthPointer = month,
                                isClosed = false
                            };
                            _context.SalaryEmployeeMonths.Add(s);
                           _context.SaveChanges();
                            projectPayment.SalaryId = s.SalaryId;
                            Console.WriteLine("создали зарплатомесяц и присвоили в проджект пэймент");
                        }
                        else if (!salary.isClosed)
                        {
                            projectPayment.SalaryId = salary.SalaryId;
                        }
                        _context.ProjectPayments.Add(projectPayment);
                    }
                }
           // }
            await _context.SaveChangesAsync();
        }

        async Task TaskUpdateOnProjectGive(ERP.Models.Project project, Employee employee)
        {
            int projectId = project.ProjectId;
            int employeeId = employee.EmployeeId;
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.ProjectId == projectId && t.Description == project.ProjectName + " ОТПРАВКА");
            if (task == null)
            {
                TaskItem newTask = new TaskItem()
                {
                    EmployeeId = employeeId,
                    ProjectId = projectId,
                    Description = project.ProjectName + " ОТПРАВКА",
                    Deadline = project.Deadline
                };
                _context.Tasks.Add(newTask);
            }
            else
            {
                task.EmployeeId = employeeId;
            }

            var tasks =  await _context.Tasks.Where(t => t.ProjectId == projectId && t.Description != project.ProjectName + " ОТПРАВКА").ToListAsync();
            if (tasks.Any())
            {
                _context.Tasks.RemoveRange(tasks);
            }
            await _context.SaveChangesAsync();
        }
        
        async Task StagerSalaryProjectRecount(ERP.Models.Project project, Employee employee)
        {
            int projectId = project.ProjectId;
            int employeeId = employee.EmployeeId;
           // Console.WriteLine("++++++++++++++++++создаю стажёрскоу оплату для проекта: "+ projectId);
            var monthCount = ((project.Deadline.Year - project.PaymentDate.Year) * 12) + (project.Deadline.Month - project.PaymentDate.Month);
            Console.WriteLine("monthCount forStager payments: " + monthCount);
            if (monthCount == 0)
            {
                ProjectPayment projectPayment = new ProjectPayment();
                projectPayment.PartsCount = null;
                projectPayment.PartNumber = null;
                projectPayment.ProjectId = projectId;
                projectPayment.isStagerAdd = true;
                projectPayment.Amount = project.EmployeePayment.HasValue ? project.EmployeePayment * 0.375m : null;
                projectPayment.MonthPointer = new DateOnly(project.Deadline.Year, project.Deadline.Month, 1);
                var salary = await _context.SalaryEmployeeMonths.FirstOrDefaultAsync(s => s.EmployeeId == employee.BossId
                                                        && s.MonthPointer == new DateOnly(project.Deadline.Year, project.Deadline.Month, 1));
                if (salary == null)
                {
                    SalaryEmployeeMonth s = new SalaryEmployeeMonth()
                    {
                        EmployeeId = employee.BossId,
                        FixSalary = 20000,
                        MonthPointer = new DateOnly(project.Deadline.Year, project.Deadline.Month, 1),
                        isClosed = false
                    };
                    _context.SalaryEmployeeMonths.Add(s); 
                    _context.SaveChanges();
                    projectPayment.SalaryId = s.SalaryId;
                }
                else if (!salary.isClosed)
                {
                    projectPayment.SalaryId = salary.SalaryId;
                }
                _context.ProjectPayments.Add(projectPayment);
            }
            else
            {
                monthCount++;
                for (int i = 1; i <= monthCount; i++)
                {
                    DateOnly month = new DateOnly(project.PaymentDate.Year, project.PaymentDate.Month, 1).AddMonths(i - 1);
                    ProjectPayment projectPayment = new ProjectPayment();
                    projectPayment.PartsCount = monthCount;
                    projectPayment.ProjectId = projectId;
                    projectPayment.isStagerAdd = true;
                    projectPayment.PartNumber = i;
                    projectPayment.Amount = project.EmployeePayment.HasValue ? project.EmployeePayment * 0.375m / monthCount : null;

                    projectPayment.MonthPointer = month;
                    var salary = await _context.SalaryEmployeeMonths.FirstOrDefaultAsync(s => s.EmployeeId == employee.BossId
                                                                                && s.MonthPointer == month);
                    if (salary == null)
                    {
                        SalaryEmployeeMonth s = new SalaryEmployeeMonth()
                        {
                            EmployeeId = employee.BossId,
                            FixSalary = 20000,
                            MonthPointer = month,
                            isClosed = false
                        };
                        _context.SalaryEmployeeMonths.Add(s);
                        _context.SaveChanges();
                        projectPayment.SalaryId = s.SalaryId;
                    }
                    else if (!salary.isClosed)
                    {
                        projectPayment.SalaryId = salary.SalaryId;
                    }
                    _context.ProjectPayments.Add(projectPayment);     
                    _context.SaveChanges();
                }
            }
            await _context.SaveChangesAsync();
        }

        //====================================================Projects==========================================================//

        //-----------------------------------------------добавление---------------------------------------//

        [HttpPost]
        [Route("CurrentProjects/AddProject")]
        public async Task<IActionResult> AddProject([FromForm] ProjectViewModel model)
        {
            //   Console.WriteLine("----------AddProject-------------");
            Console.WriteLine(model);
            
            if (!ModelState.IsValid)
            {
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var value = ModelState[modelStateKey];
                    foreach (var error in value.Errors)
                    {
                        Console.WriteLine($"Key: {modelStateKey}, Error: {error.ErrorMessage}");
                    }
                }
                return BadRequest(new { Message = "Ошибка при добавлении проекта.", Errors = ModelState });
            }

            ERP.Models.Project project;

            //создание проекта
            if (model.ProjectId == 0)
            {
                project = new ERP.Models.Project
                {
                    ProjectName = model.ProjectName,
                    Deadline = model.Deadline,
                    EventDate = model.EventDate,
                    ClientId = model.ClientId,
                    EmployeeId = model.EmployeeId != 0 ? model.EmployeeId : null,
                    EmployeePayment = model.EmployeePayment,
                    PaymentDate = model.PaymentDate ?? DateOnly.FromDateTime(DateTime.Today),
                    IsDocumentsComleted = model.IsDocumentsComleted,
                    LayoutsRequired = model.LayoutsRequired
                };
                _context.Projects.Add(project);
                _context.SaveChanges();
                if (model.EmployeeId != 0)
                    await ProjectSalaryRecount(project.ProjectId, (int)model.EmployeeId);
               
            }

            //редактирование проекта
            else
            {
                project = await _context.Projects.FirstAsync(p => p.ProjectId == model.ProjectId);
                project.ProjectName = model.ProjectName;
                project.Deadline = model.Deadline;
                project.EventDate = model.EventDate;
                project.IsDocumentsComleted = model.IsDocumentsComleted;
                project.LayoutsRequired = model.LayoutsRequired;
                if (project.ClientId != model.ClientId)
                {
                    // Формируем старый и новый путь
                    var oldClient = (await _context.Clients.FindAsync(project.ClientId))?.Title?.Replace(" ", "_").Replace(":", "_");
                    var newClient = (await _context.Clients.FindAsync(model.ClientId))?.Title?.Replace(" ", "_").Replace(":", "_");

                    if (!string.IsNullOrEmpty(oldClient) && !string.IsNullOrEmpty(newClient))
                    {
                        var sanitizedProjectName = project.ProjectName.Replace(" ", "_").Replace(":", "_");
                        var oldPath = Path.Combine("wwwroot", oldClient, sanitizedProjectName);
                        var newPath = Path.Combine("wwwroot", newClient, sanitizedProjectName);

                        try
                        {
                            if (Directory.Exists(oldPath))
                            {
                                // Создаём папку нового клиента, если её нет
                                Directory.CreateDirectory(Path.Combine("wwwroot", newClient));

                                // Перемещаем папку проекта
                                Directory.Move(oldPath, newPath);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при перемещении папки: {ex.Message}");
                            // Логирование ex.StackTrace или другой обработчик ошибок
                        }
                    }
                }

                project.ClientId = model.ClientId;
                project.PaymentDate = (DateOnly)model.PaymentDate;

              //  Console.WriteLine("model.EmployeeId: " + model.EmployeeId + " | project.EmployeeId: " + project.EmployeeId);

                if (model.EmployeeId != project.EmployeeId && model.EmployeeId != 0)
                    await ProjectSalaryRecount(project.ProjectId, (int)model.EmployeeId);

                project.EmployeeId = model.EmployeeId != 0 ? model.EmployeeId : null;
                project.EmployeePayment = model.EmployeePayment;
               
            }
            await _context.SaveChangesAsync();
        
            return Ok();
        }

        [HttpPost]
        [Route("CurrentProjects/addItems")]
        public async Task<IActionResult> addItems([FromForm] List<ItemUploadViewModel> items)
        {
         //   Console.WriteLine("-------------Method: AddItems----------------");
            string? method = Request.Query["method"];
           // Console.WriteLine("-------------Method:" + method);

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

                Console.WriteLine("itemDTO==============  " + itemDto);

                //Item itemExist = _context.Items.FirstOrDefault(i => i.ItemId == itemDto.ItemId);
                //if(itemExist != null)
                //{

                //}
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

                    //защита от дубликата
                    Item itemExist = _context.Items.FirstOrDefault(i => i.SketchPath == "\\" + filePath);
                    if (itemExist != null)
                        continue;

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

            if (method == "add")
                project.EmployeePayment = projectPrice * 0.14m;

            Console.WriteLine(" -----------------project payment total: " + project.PaymentTotal);
            _context.SaveChanges();
            return Ok("Данные успешно загружены.");

        }

        [HttpPost]
        [Route("CurrentProjects/editItems")]
        public async Task<IActionResult> editItems([FromForm] List<ItemUploadViewModel> items)
        {
           // Console.WriteLine("-------------Method: editItems----------------");
            string? method = Request.Query["method"];
            //Console.WriteLine("-------------Method:" + method);

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

              //Console.WriteLine("itemDTO==============  " + itemDto);

                Item itemExist = _context.Items.FirstOrDefault(i => i.ItemId == itemDto.ItemId);
                if (itemExist != null)
                {
                    if (itemDto.SketchPath != itemExist.SketchPath)
                    {
                        var sanitizedItemName = itemDto.Sketch.FileName.Replace(" ", "_").Replace(":", "_");
                        var uploadsFolder = Path.Combine(client, sanitizedProjectName, "Sketches");
                        Directory.CreateDirectory(Path.Combine("wwwroot", uploadsFolder)); // Создаём папку, если её ещё нет

                        var filePath = Path.Combine(uploadsFolder, sanitizedItemName);
                        using (var stream = new FileStream(Path.Combine("wwwroot", filePath), FileMode.Create))
                        {
                            await itemDto.Sketch.CopyToAsync(stream);
                        }
                        System.IO.File.Delete(itemExist.SketchPath);
                        itemExist.SketchPath = "\\" + filePath;
                    }

                    itemExist.ItemType = itemDto.ItemType;
                    itemExist.ItemName = itemDto.ItemName;
                    itemExist.Amount = itemDto.Amount;
                    itemExist.Price = itemDto.Price;
                    itemExist.ItemDescription = itemDto.ItemDescription;
                    itemExist.Materials = itemDto.Materials;
                    itemExist.Colors = itemDto.Colors;
                    itemExist.Deadline = itemDto.Deadline;
                    itemExist.ProjectId = projectId;
                    //Console.WriteLine("correct item");
                    //Console.WriteLine( $"itemPrice: {itemExist.Price}  itemAmount: {itemExist.Amount}--" );
                                    
                    projectPrice += itemExist.Price * itemExist.Amount;
                }
                else
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
                  //  Console.WriteLine("create item");
                  //  Console.WriteLine($"itemPrice: {item.Price}  itemAmount: {item.Amount}--");

                    projectPrice += item.Price * item.Amount;
                }
             //   Console.WriteLine("projectPrice: " + projectPrice);

            }
            foreach (var savedItem in savedItems)
            {
               // Console.WriteLine(savedItem.ToString());
                _context.Items.Add(savedItem);
            }
          //  Console.WriteLine($"project {project.ProjectName} payment total: " + projectPrice);
            project.PaymentTotal = projectPrice;

            if (method == "add")
                project.EmployeePayment = projectPrice * 0.14m;

          //  Console.WriteLine(" -----------------project payment total: " + project.PaymentTotal);
            _context.SaveChanges();
            return Ok("Данные успешно загружены.");

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

                        //защита от дубликата
                        var fileExist = _context.ProjectFiles.FirstOrDefault(f => f.FileTitle ==  fileName);
                        if (fileExist != null) continue;


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

                        //защита от дубликата
                        var fileExist = _context.ProjectFiles.FirstOrDefault(f => f.FileTitle == fileName);
                        if (fileExist != null) continue;

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

        //----------------------------------------карточка проекта----------------------------------------//

        [HttpGet("LoadProjectCard")]
        public async Task<IActionResult> LoadProjectCard(int id)
        {
          //  Console.WriteLine("------------------------LoadProjectCard--------------------------");
            var project = await _context.Projects
             .Include(p => p.Client)
             .Include(p => p.Employee)
             .Include(p => p.Items)
             .Include(p => p.ProjectFiles)
             .Include(p =>p.JournalNotes)
                .ThenInclude(n => n.JournalTopic)
             .Include(p => p.JournalNotes)
                 .ThenInclude(n => n.Photos)
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
                EmployeePayment = project.EmployeePayment,
                Description = project.Description,
                IsDocumentsComleted = project.IsDocumentsComleted ?? false,
                LayoutsRequired = project.LayoutsRequired,
                JournalNotes = project.JournalNotes?.Select(i => new JournalNote
                {
                    JournalNoteDescription = i.JournalNoteDescription,
                    JournalNoteId = i.JournalNoteId,
                    JournalTopicId = i.JournalTopic.JournalTopicId,
                    JournalTopicName = i.JournalTopic.JournalTopicName,
                    Photos = i.Photos.Select(p => new ProjectFile
                    { 
                        FileId = p.FileId,
                        FileTitle = p.FileTitle,
                        FilePath = p.FilePath,
                        UploadedAt = p.UploadedAt
                    }).ToList(),
                }).ToList(),
                Items = project.Items.Select(i => new ItemCardViewModel
                {
                    ItemType = i.ItemType,
                    ItemName = i.ItemName,
                    SketchPath = i.SketchPath,
                    SelectedColors = i.Colors,
                    SelectedMaterials = i.Materials,
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
                    FileId = f.FileId,
                    FileTitle = f.FileTitle,
                    FilePath = f.FilePath,
                    UploadedAt = f.UploadedAt
                }).ToList(),
               
            };

          //  Console.WriteLine(viewModel);


            var topics = _context.JournalTopics.ToList();
           
            ViewBag.topics = topics;


            return PartialView("_ProjectCardPartial", viewModel); 
        }

        [Route("/CurrentProjects/EditProject")]
        public async Task<IActionResult> EditProject(int id)
        {
          
         //   Console.WriteLine("------------------------EditProject--------------------------");
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
                EmployeePayment = project.EmployeePayment,
                Description = project.Description,
                SelectedClientId = project.ClientId,
                SelectedEmployeeId = project.EmployeeId,
                IsDocumentsComleted = project.IsDocumentsComleted ?? false,
                LayoutsRequired = project.LayoutsRequired,
                Items = project.Items.Select(i => new ItemCardViewModel
                {
                    ItemId = i.ItemId,
                    ItemType = i.ItemType,
                    ItemName = i.ItemName,
                    SketchPath = i.SketchPath,
                    Amount = i.Amount,
                    Price = i.Price,
                    Deadline = i.Deadline,
                    ItemDescription = i.ItemDescription,
                    SelectedMaterials = i.Materials,
                    SelectedColors = i.Colors,
                }).ToList(),
                Layouts = project.ProjectFiles.Where(f => f.FileType == "макет").Select(f => new FileCardViewModel
                {
                    FileId = f.FileId,
                    FileTitle = f.FileTitle,
                    FilePath = f.FilePath,
                    UploadedAt = f.UploadedAt
                }).ToList(),
                Documents = project.ProjectFiles.Where(f => f.FileType == "документ").Select(f => new FileCardViewModel
                {
                    FileId = f.FileId,
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

         // Console.WriteLine(viewModel);

            return PartialView("_EditProjectPartial", viewModel);
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

            //foreach (var item in items)
            //{
            //    Console.WriteLine(item.title + item.id);
            //}

            return Json(items);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotos([FromForm] List<AddPhotosViewModel> photos)
        {
            Console.WriteLine("AddPhotos; " + photos.Count);
            foreach (var p in photos)
            {
                Console.WriteLine(p);
            }
            //Console.WriteLine();

            var project = _context.Projects.FirstOrDefault(p => p.ProjectId == photos[0].projectId);
            var note = _context.JournalNotes.FirstOrDefault(n => n.JournalNoteId == photos[0].noteId);

            string uploadsFolder;
            if (project != null)
            {
                Console.WriteLine(project.ProjectName);
                var client = _context.Clients.FirstOrDefault(c => c.ClientId == project.ClientId);
                var sanitizedProjectName = project.ProjectName.Replace(" ", "_").Replace(":", "_");
                var sanitizedClientName = client.Title.Replace(" ", "_").Replace(":", "_");
                uploadsFolder = Path.Combine(sanitizedClientName, sanitizedProjectName, photos[0].fileType);
            }
            else
            {
                Console.WriteLine("project is null");
                uploadsFolder = note.JournalTopicName.Replace(" ", "_").Replace(":", "_");
            }
           

            foreach (var item in photos)
            {
              //  Console.WriteLine(item);

                if (item.photo != null)
                {
                    var fileName = item.photo.FileName.Replace(" ", "_").Replace(":", "_");

                    Directory.CreateDirectory(Path.Combine("wwwroot", uploadsFolder));
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var stream = new FileStream(Path.Combine("wwwroot", filePath), FileMode.Create))
                    {
                        await item.photo.CopyToAsync(stream);
                    }
                    ProjectFile projectFile = new ProjectFile();
                    projectFile.ProjectId = item.projectId;
                    projectFile.JournalNoteId = item.noteId;
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

        [HttpPost]
        public async Task<IActionResult> DeletePhoto()
        {
           // Console.WriteLine("DeletePhoto ищем айди в проке запроса. ");
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            Console.WriteLine("DeletePhoto: " + id);
            try
            {
                ProjectFile photo = _context.ProjectFiles.First(i => i.FileId == id);

                //string filePath = Path.Combine("wwwroot", photo.FilePath.TrimStart('/'));
                //Console.WriteLine(filePath);
                string fullpath = "wwwroot\\" + photo.FilePath;
                Console.WriteLine(fullpath);

                if (System.IO.File.Exists(fullpath))
                {
                    System.IO.File.Delete(fullpath);
                }
                //Console.WriteLine("fullpath");
                _context.ProjectFiles.Remove(photo);
                await _context.SaveChangesAsync();
                
                return Ok(new { success = true });  
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "Ошибка удаления файла: " + ex.Message });
            }
        }

      
        [HttpPut("/CurrentProjects/PutProjectDescription")]
        public IActionResult PutProjectDescription([FromBody] JsonElement data)
        {
            int id = 0;
            Int32.TryParse(Request.Query["projectId"], out id);
            data.TryGetProperty("description", out JsonElement descriptionElement);
            var description = descriptionElement.GetString();

        //    Console.WriteLine("PutProjectDescription: " + description + ", ID: " + id);

            var project = _context.Projects.Find(id);
            if (project == null)
            {
                return NotFound("Проект не найден.");
            }
            project.Description = description;
            _context.SaveChanges();
            return Ok(new { message = "Описание обновлено успешно." });
        }


        [HttpPut("CurrentProjects/PutProjectNote")]
        public IActionResult PutProjectNote([FromBody] JsonElement data)
        {
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            int topicId = 0;
            Int32.TryParse(Request.Query["topicId"], out topicId);

            int projectId = 0;
            Int32.TryParse(Request.Query["projectId"], out projectId);

            data.TryGetProperty("description", out JsonElement descriptionElement);
            var description = descriptionElement.GetString();
            Console.WriteLine("PutProjectNote");
            Console.WriteLine(id);
            Console.WriteLine(topicId);
            Console.WriteLine(description);


            var note = _context.JournalNotes.FirstOrDefault(n => n.JournalNoteId == id);
            var topic = _context.JournalTopics.Find(topicId);


            if (topic == null)
            {
                return NotFound(new { message = "Topic not found." });
            }
            if (note == null)
            {
                var isRepeat = _context.JournalNotes.FirstOrDefault(n => n.ProjectId == projectId && n.JournalTopicId == topic.JournalTopicId);
                if (isRepeat != null)
                {
                    isRepeat.JournalNoteDescription += " | " + description;
                    note = isRepeat;
                }

                else
                {
                    note = new JournalNote();
                    note.JournalNoteDescription = description;
                    note.JournalTopicId = topicId;
                    note.JournalTopicName = topic.JournalTopicName;
                    note.ProjectId = projectId == 0 ? null : projectId;
                    _context.JournalNotes.Add(note);
                }
            }
            else
            {
                note.JournalNoteDescription = description;
                note.JournalTopicId = topicId;
                note.JournalTopicName = topic.JournalTopicName;
                note.ProjectId = projectId == 0 ? null : projectId;
            }
            _context.SaveChanges();


            return Ok(new { noteId = note.JournalNoteId });
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

        //------------------------------------------удалить части проекта-------------------------------------------//

        [HttpDelete]
        public async Task<IActionResult> DeleteProject()
        {                  
            int projectId = 0;
            Int32.TryParse(Request.Query["projectId"], out projectId);
            try
            {
                var project = await _context.Projects.FirstOrDefaultAsync(i => i.ProjectId == projectId);
                Console.WriteLine($"delete project {project.ProjectName}");
                var prpays = _context.ProjectPayments.Where(pp => pp.ProjectId == projectId);
                _context.ProjectPayments.RemoveRange(prpays);
                var tasks = _context.Tasks.Where(pp => pp.ProjectId == projectId);
                _context.Tasks.RemoveRange(tasks);
                var files = _context.ProjectFiles.Where(pp => pp.ProjectId == projectId).ToList();
                var items = _context.Items.Where(pp => pp.ProjectId == projectId).ToList();

                for(int i = 0; i < files.Count; i++)
                {
                    try
                    {
                        DeleteFile(files[i].FileId);
                    }
                    catch { }
                }
                for (int i = 0; i < items.Count; i++)
                {
                    try
                    { 
                        DeleteItem(items[i].ItemId);
                    }
                    catch { }
                }
                _context.Projects.Remove(project);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "Ошибка  удаления project: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteFile()
        {

           // Console.WriteLine("DeleteFile ищем айди в проке запроса. ");
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            //Console.WriteLine("DeleteFile" + id);
            try
            {
                ProjectFile file = _context.ProjectFiles.First(i => i.FileId == id);
                string fullpath = "wwwroot\\" + file.FilePath;
                // Console.WriteLine(fullpath);
                if (System.IO.File.Exists(fullpath))
                    System.IO.File.Delete(fullpath);

                _context.ProjectFiles.Remove(file);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "Ошибка удаления файла: " + ex.Message });
            }

        }
        public IActionResult DeleteFile(int id)
        {
            Console.WriteLine("DeleteFile ищем айди в боди запроса. ");
            try
            {
                ProjectFile file = _context.ProjectFiles.First(i => i.FileId == id);
                string fullpath = "wwwroot\\" + file.FilePath;
                //Console.WriteLine(fullpath);
                if (System.IO.File.Exists(fullpath))
                    System.IO.File.Delete(fullpath);

                _context.ProjectFiles.Remove(file);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "Ошибка удаления файла: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteItem()
        {
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            Console.WriteLine("DeleteFile" + id);
            try
            {
                Item item = _context.Items.First(i => i.ItemId == id);
                string fullpath = "wwwroot\\" + item.SketchPath;
               // Console.WriteLine(fullpath);
                if(System.IO.File.Exists(fullpath))
                    System.IO.File.Delete(fullpath);

                _context.Items.Remove(item);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "Ошибка удаления item: " + ex.Message });
            }
        }

        public IActionResult DeleteItem(int id)
        {
            try
            {
                Item item = _context.Items.First(i => i.ItemId == id);
                string fullpath = "wwwroot\\" + item.SketchPath;
                //  Console.WriteLine(fullpath);
                if (System.IO.File.Exists(fullpath))
                    System.IO.File.Delete(fullpath);

                _context.Items.Remove(item);
                _context.SaveChanges();
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "Ошибка удаления item: " + ex.Message });
            }
        }

        [HttpPost]
        [Route("CurrentProjects/TakeProject")]
        //--------------------------------------------------передать проект----------------------------------------//

        public async Task<IActionResult> TakeProject()
        {
            int employeeId = 0;
            Int32.TryParse(Request.Query["employeeId"], out employeeId);
            int projectId = 0;
            Int32.TryParse(Request.Query["projectId"], out projectId);
            try
            {
                var project = await _context.Projects.FirstOrDefaultAsync(i => i.ProjectId == projectId);
                var employee = await _context.Employees.FirstOrDefaultAsync(i => i.EmployeeId == employeeId);
                Console.WriteLine($"take project. employee {employee.EmployeeName} | project {project.ProjectName}");
                project.EmployeeId = employeeId;
                await ProjectSalaryRecount(projectId, employeeId);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new { message = "Ошибка переназначения мастера: " + ex.Message });
            }
        }


        //====================================================Clients=============================================================//

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
                client.FirstRequstDate = model.FirstContact ?? DateOnly.FromDateTime(DateTime.Now);
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
            if(rekvizits.Count == 0) { return Ok("файлы реквизитов не изменились"); }
           
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
                    var isExist = _context.Requisites.FirstOrDefault(r => r.FileTitle == fileName);
                    if (isExist != null)
                        continue;

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


        [HttpGet]
        [Route("CurrentProjects/GetClients")]
        public IActionResult GetClients()
        {
            return Json(_context.Clients);
        }

        public IActionResult EditClientPartial(int clientId)
        {
            var client = _context.Clients
                .Include(c => c.Contacts)
                .Include(c => c.DeliveryAddresses)
                .Include(c => c.Requisites)
                .FirstOrDefault(c => c.ClientId == clientId);

            if (client == null)
            {
                return NotFound();
            }

            var clientVM = new ClientViewModel
            {
                ClientId = client.ClientId,
                ClientTitle = client.Title,
                City = client.City,
                FirstContact = client.FirstRequstDate,
                contacts = client.Contacts?.Select(c => new ContactViewModel
                {
                    ContactId = c.ContactId,
                    Name = c.ContactName,
                    Phone = c.PhoneNumber,
                    Email = c.Email
                }).ToList(),
                address = client.DeliveryAddresses?.Select(a => new AddressViewModel
                {
                    AddressId = a.AddressId,
                    Address = a.DeliveryAddress1
                }).ToList(),
                rekvizits = client.Requisites?.Select(r => new RekvizitViewModel
                {
                    RekvizitId = r.RequisiteId,
                    FileName = r.FileTitle
                }).ToList()
            };

            Console.WriteLine("edit client^ ");
            Console.WriteLine(clientVM);
            return PartialView("_EditClientPartial", clientVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("/CurrentProjects/EditClient")]
        public IActionResult EditClient([FromForm] ClientViewModel model)
        {
            Console.WriteLine(  "edit client-------------------- name: " +  model.ClientTitle);
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    var field = state.Key;
                    var errors = state.Value.Errors;
                    foreach (var error in errors)
                    { Console.WriteLine($"Поле: {field}, Ошибка: {error.ErrorMessage}"); }
                }
            }

            Console.WriteLine("model: " + model);

             var client = _context.Clients.FirstOrDefault(c => c.ClientId == model.ClientId);
            if(client == null)
                { return BadRequest("Клиент с таким id не существует"); }

                Console.WriteLine(model);
                client.Title = model.ClientTitle;
                client.City = model.City;             
                client.FirstRequstDate = model.FirstContact ?? client.FirstRequstDate;

            if (model.contacts.Any())
            {
                foreach (var c in model.contacts)
                {
                    //   Console.WriteLine("searchin existing contact .......");
                    //   Console.WriteLine(c.Name);
                    // foreach(var co in clien)
                    var isExist = _context.Contacts.FirstOrDefault(con => con.ContactName == c.Name);
                    if (isExist != null)
                        //   {
                        //      Console.WriteLine( isExist.ContactName + " не пересоздаём" );
                        continue;
                    //   }
                    Contact contact = new Contact();
                    contact.ContactName = c.Name;
                    contact.PhoneNumber = c.Phone;
                    contact.Email = c.Email;
                    contact.ClientId = client.ClientId;
                    _context.Contacts.Add(contact);
                }
            }
            if (model.address.Any())
            {
                foreach (var c in model.address)
                {
                    if (_context.DeliveryAddresses.FirstOrDefault(del => del.DeliveryAddress1 == c.Address) != null)
                        continue;
                    DeliveryAddress address = new();
                    address.DeliveryAddress1 = c.Address;
                    address.ClientId = client.ClientId;
                    _context.DeliveryAddresses.Add(address);
                }
            }
                _context.SaveChanges();

                return Ok();
        }

        public IActionResult DeleteContact(int id)
        {
            Console.WriteLine("delete contact id: " + id);
            var contact = _context.Contacts.FirstOrDefault(e => e.ContactId == id);
            if (contact == null)
            {
                return NotFound();
            }
            _context.Contacts.Remove(contact);  
            _context.SaveChanges();

            return Ok();
        }

        public IActionResult DeleteAddress(int id)
        {
            var address = _context.DeliveryAddresses.FirstOrDefault(e => e.AddressId == id);
            if (address == null)
            {
                return NotFound();
            }
            _context.DeliveryAddresses.Remove(address);
            _context.SaveChanges();

            return Ok();
        }

        public IActionResult DeleteRekvizit(int id)
        {
            var rekvizit = _context.Requisites.FirstOrDefault(e => e.RequisiteId == id);
            if (rekvizit == null)
            {
                return NotFound();
            }
            string fullpath = "wwwroot\\" + rekvizit.FilePath;
            Console.WriteLine("delete rekvisit: " + fullpath);
            _context.Requisites.Remove(rekvizit);
            _context.SaveChanges();
            try
            {
                System.IO.File.Delete(fullpath);
            }
            catch { }
            
            
            return Ok();
        }

        public IActionResult DeleteClient(int id)
        {
          var client = _context.Clients.FirstOrDefault(c =>c.ClientId ==  id);
            if (client == null)
                return NotFound();

            var contacts = _context.Contacts.Where(c => c.ClientId == client.ClientId);
            if (contacts.Any()) 
                _context.Contacts.RemoveRange(contacts);

            var adresses = _context.DeliveryAddresses.Where(c => c.ClientId == client.ClientId);
            if (adresses.Any())
                _context.DeliveryAddresses.RemoveRange(adresses);

            var rekvizits = _context.Requisites.Where(c => c.ClientId == client.ClientId);
            if (rekvizits.Any()) {
                for (int i = 0; i < rekvizits.Count(); i++) {
                    DeleteRekvizit(rekvizits.ToList()[i].RequisiteId);
                }
            }
            _context.Clients.Remove(client);
            _context.SaveChanges();
            return Ok();
        }


        

        //=============================================================планнер=============================================================//

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



        //=============================================================прочее=============================================================//

        [HttpGet]
        [Route("GetCurrentProjectsByEmployeeId")]
        public IActionResult GetCurrentProjectsByEmployeeId()
        {
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            var projects = _context.Projects.Where(p => p.EmployeeId == id && (p.IsArchived == false || p.IsArchived == null));
            return Json(projects);
        }

        [HttpPost]
        [Route("CurrentProjects/AddMaterials")]
        public IActionResult AddMaterials()
        {
            Console.WriteLine("--------------Method: AddMaterials-----------");
            string materialName = Request.Query["Materials"];

            var isExist = _context.Materials.FirstOrDefault(m =>m.MaterialName == materialName);
            if (isExist != null)
                return BadRequest("уже существует");

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

            var isExist = _context.Colors.FirstOrDefault(m => m.ColorName == colorName);
            if (isExist != null)
                return BadRequest("уже существует");

            Color color = new();
            color.ColorName = colorName;
            Console.WriteLine(color.ColorName);
            _context.Colors.Add(color);
            _context.SaveChanges();

            return Ok("Color успешно загружены.");
        }

        [HttpGet]
        [Route("CurrentProjects/GetMaterials")]
        public IActionResult GetMaterials()
        {
            Console.WriteLine("GetMaterials");
            return Json(_context.Materials);
        }

        [HttpGet]
        [Route("CurrentProjects/GetColors")]
        public IActionResult GetColors()
        {
            Console.WriteLine("GetColors");
            return Json(_context.Colors);
        }

        [HttpDelete]
        [Route("CurrentProjects/DeleteMaterials")]
        public IActionResult DeleteMaterials()
        {
            Console.WriteLine("DeleteMaterial");
            string name = Request.Query["name"];

            var material = _context.Materials.FirstOrDefault(m => m.MaterialName == name);
            if (material == null)
                return NotFound();

            _context.Materials.Remove(material);
            _context.SaveChanges();
            return Ok();
        }

        [HttpDelete]
        [Route("CurrentProjects/DeleteColors")]
        public IActionResult DeleteColors()
        {

            Console.WriteLine("DeleteColor");
            string name = Request.Query["name"];

            var color = _context.Colors.FirstOrDefault(c => c.ColorName == name);
            if (color == null)
                return NotFound();

            _context.Colors.Remove(color);
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        [Route("CurrentProjects/UpdateMaterials")]
        public IActionResult UpdateMaterials()
        {
            Console.WriteLine("UpdateMaterial");
            string name = Request.Query["name"];
            string newName = Request.Query["newName"];
            if (newName.Length == 0 || newName == null)
                return BadRequest("новое название не передано");

            var material = _context.Materials.FirstOrDefault(m => m.MaterialName == name);
            if (material == null)
                return NotFound();

            material.MaterialName = newName;
            _context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        [Route("CurrentProjects/UpdateColors")]
        public IActionResult UpdateColors()
        {

            Console.WriteLine("UpdateColor");
            string name = Request.Query["name"];
            string newName = Request.Query["newName"];
            if (newName.Length == 0 || newName == null)
                return BadRequest("новое название не передано");

            var color = _context.Colors.FirstOrDefault(c => c.ColorName == name);
            if (color == null)
                return NotFound();

            color.ColorName = newName;
            _context.SaveChanges();
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetJournalTopics()
        {
            Console.WriteLine("GetJournalTopics");
            var topics = await _context.JournalTopics
                .Select(t => new { t.JournalTopicId, t.JournalTopicName })
                .ToListAsync();

            foreach(var topic in topics)
                Console.WriteLine(topic.JournalTopicName);

            return Json(topics);
        }

        
        public IActionResult DeleteJournalNote()
        {

            Console.WriteLine("DeleteJournalNote");
            int id = 0;
            Int32.TryParse(Request.Query["id"], out id);
            var note = _context.JournalNotes.FirstOrDefault(n => n.JournalNoteId == id);
            if (note == null) return NotFound();
            var photos = _context.ProjectFiles.Where(pp => pp.JournalNoteId == id).ToList();

            for (int i = 0; i < photos.Count; i++)
            {
                try
                {
                    DeleteFile(photos[i].FileId);
                }
                catch { }
            }
            _context.JournalNotes.Remove(note);
            _context.SaveChanges();


            return  Ok("потрачено");
        }
    }
}
