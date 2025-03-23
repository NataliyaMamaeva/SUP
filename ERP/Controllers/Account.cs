using ERP.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ERP.ViewModels;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ERP.Controllers
{
    public class Account : Controller
    {
        // GET: Account

        private readonly SignInManager<ErpUser> _signInManager;
        private readonly UserManager<ErpUser> _userManager;
        private readonly EmailSender _emailSender;
        private readonly ErpContext _context;

        public Account(SignInManager<ErpUser> signInManager, UserManager<ErpUser> userManager, EmailSender emailSender, ErpContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
            _context = context;
        }
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [Route("Account/Login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            //Console.WriteLine("Login action________________________controller account");
          
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.email, model.password, false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                 //   Console.WriteLine("login");
                    return RedirectToAction("Index", "CurrentProjects");
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            return View(model);
        }

        [HttpPost]
        [Route("Account/Logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }


        //public async Task<IActionResult> Register(RegisterViewModel model)
        //{
        //    if (!ModelState.IsValid) return View(model);
        //    var user = new ErpUser { UserName = model.Email, Email = model.Email };
        //    var result = await _userManager.CreateAsync(user, model.Password);

        //    if (result.Succeeded)
        //    {
        //        // Генерируем токен для подтверждения email
        //        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //        var confirmationLink = Url.Action("ConfirmEmail", "Account",
        //            new { userId = user.Id, token = token }, Request.Scheme);

        //        // Отправляем email
        //        await _emailSender.SendEmailAsync(user.Email, "Подтверждение email",
        //            $"Подтвердите свою почту, перейдя по <a href='{confirmationLink}'>этой ссылке</a>.");

        //        return View("RegisterSuccess");
        //    }

        //    foreach (var error in result.Errors)
        //        ModelState.AddModelError("", error.Description);

        //    return View(model);
        //}


        //public async Task<IActionResult> ConfirmEmail(string userId, string token)
        //{
        //    if (userId == null || token == null)
        //        return BadRequest("Неверный запрос");

        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null)
        //        return NotFound("Пользователь не найден");

        //    var result = await _userManager.ConfirmEmailAsync(user, token);
        //    if (result.Succeeded)
        //        return View("ConfirmEmailSuccess");

        //    return View("Error");
        //}


        [HttpPost]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordViewModel model)
        {

            Console.WriteLine("ForgotPassword");
            Console.WriteLine(model.Email);

            if (!ModelState.IsValid) return BadRequest("Некорректный email.");

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null) // || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return Ok(new { Message = "Если почта зарегистрирована, мы отправили вам письмо." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = Url.Action("ResetPassword", "Account",
                new { token, email = user.Email }, Request.Scheme);




            await _emailSender.SendEmailAsync(model.Email, "Сброс пароля",
                $"Для сброса пароля перейдите по <a href='{resetUrl}'>этой ссылке</a>.");

            return Ok(new { Message = "Проверь почту" });
        }


        public async Task<IActionResult> ResetPassword(string token, string email)
        {
            if (token == null || email == null)
                return BadRequest("Неверный токен");

            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }



        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            Console.WriteLine("ResetPassword");
            Console.WriteLine($"Email: {model.Email}");
            Console.WriteLine($"Token: {model.Token}");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("!ModelState.IsValid");
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                Console.WriteLine("Пользователь не найден");
                return RedirectToAction("ResetPasswordConfirmation");
            }

            Console.WriteLine("Пользователь найден, пробуем сбросить пароль...");
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (result.Succeeded)
            {
                Console.WriteLine("Пароль успешно сброшен!");
                TempData["SuccessMessage"] = "Пароль успешно изменён!";
                return RedirectToAction("Login");
            }

            Console.WriteLine("Ошибка сброса пароля:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine(error.Description);
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }


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

            // Создаём сотрудника
            var employee = new Employee
            {
                EmployeeName = model.EmployeeName,
                PhoneNumber = model.PhoneNumber,
                BossId = model.BossId ?? null,
                Passport = model.Passport,
                Position = model.Position ?? "Master",
                Email = model.Email
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Создаём пользователя с временным паролем
            var user = new ErpUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmployeeId = employee.EmployeeId
            };

            var tempPassword = "DefaultPass123!"; // Задаём временный пароль
            var result = await _userManager.CreateAsync(user, tempPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Ошибка создания аккаунта.", Errors = result.Errors });
            }

            // Генерируем токен подтверждения
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmUrl = Url.Action("ConfirmEmail", "Account",
                new { email = model.Email, token }, Request.Scheme);

            // Отправляем письмо
            await _emailSender.SendEmailAsync(model.Email, "Подтверждение регистрации",
                $"<p>Подтвердите почту, перейдя по <a href='{confirmUrl}'>этой ссылке</a>.</p>");

            return Json(new { success = true, message = "Письмо с подтверждением отправлено." });

        }


        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("Пользователь не найден.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return BadRequest("Ошибка подтверждения почты.");
            }

            // Генерируем токен для смены пароля
            var passToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Перенаправляем на установку пароля
            return RedirectToAction("SetPassword", "Account", new { email, token = passToken });
        }


        public IActionResult SetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return BadRequest("Неверные параметры запроса.");

            return View("ResetPassword", new ResetPasswordViewModel { Token = token, Email = email });
        }






        //  [HttpPost]
        //public async Task<IActionResult> AddEmployee(CreateEmployeeViewModel model)
        //{
        //    Console.WriteLine(model);
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest("Некорректные данные. Проверьте форму.");
        //    }

        //    var existingUser = await _userManager.FindByEmailAsync(model.Email);
        //    if (existingUser != null)
        //    {
        //        return Conflict(new { Message = "Пользователь с таким email уже существует." });
        //    }

        //    // Создаём сотрудника, но без учётной записи
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

        //    // Генерируем токен для подтверждения email
        //    var token = Guid.NewGuid().ToString(); // Можно использовать и IdentityUser, но проще через Guid
        //    var confirmUrl = Url.Action("ConfirmEmail", "Account",
        //        new { email = model.Email, token = token }, Request.Scheme);

        //    // Сохраняем токен в БД (например, в таблице `EmailConfirmations`)
        //    var emailConfirm = new EmailConfirmation
        //    {
        //        Email = model.Email,
        //        Token = token,
        //        EmployeeId = employee.EmployeeId
        //    };
        //    _context.EmailConfirmations.Add(emailConfirm);
        //    await _context.SaveChangesAsync();

        //    // Отправляем письмо пользователю
        //    await _emailSender.SendEmailAsync(model.Email, "Подтверждение регистрации",
        //        $"<p>Для завершения регистрации перейдите по <a href='{confirmUrl}'>этой ссылке</a>.</p>");

        //    return Ok(new { Message = "Письмо с подтверждением отправлено." });
        //}




        //public async Task<IActionResult> ConfirmEmail(string email, string token)
        //{
        //    var confirmEntry = _context.EmailConfirmations
        //        .FirstOrDefault(e => e.Email == email && e.Token == token);

        //    if (confirmEntry == null)
        //    {
        //        return BadRequest("Неверный или устаревший токен.");
        //    }

        //    var employee = await _context.Employees.FindAsync(confirmEntry.EmployeeId);
        //    if (employee == null)
        //    {
        //        return NotFound("Сотрудник не найден.");
        //    }

        //    var user = new ErpUser
        //    {
        //        UserName = email,
        //        Email = email,
        //        EmployeeId = employee.EmployeeId
        //    };

        //    var result = await _userManager.CreateAsync(user);
        //    if (!result.Succeeded)
        //    {
        //        return BadRequest(new { Message = "Ошибка создания аккаунта.", Errors = result.Errors });
        //    }

        //    // Удаляем подтверждение из БД
        //    _context.EmailConfirmations.Remove(confirmEntry);
        //    await _context.SaveChangesAsync();

        //    // Генерируем токен для установки пароля
        //    var passToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        //    // Перенаправляем на установку пароля
        //    return RedirectToAction("SetPassword", "Account", new { email = user.Email, passToken });
        //}


        //public IActionResult SetPassword(string email, string token)
        //{
        //    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        //        return BadRequest("Неверные параметры запроса.");

        //    return PartialView("ResetPassword", new ResetPasswordViewModel { Token = token, Email = email });
        //} 



        //public async Task<IActionResult> ConfirmEmail(string email, string token)
        //{
        //    var confirmEntry =  _context.EmailConfirmations
        //        .FirstOrDefault(e => e.Email == email && e.Token == token);

        //    if (confirmEntry == null)
        //    {
        //        return BadRequest("Неверный или устаревший токен.");
        //    }

        //    var employee = await _context.Employees.FindAsync(confirmEntry.EmployeeId);
        //    if (employee == null)
        //    {
        //        return NotFound("Сотрудник не найден.");
        //    }

        //    var user = new ErpUser
        //    {
        //        UserName = email,
        //        Email = email,
        //        EmployeeId = employee.EmployeeId
        //    };

        //    var result = await _userManager.CreateAsync(user, "DefaultPassword123!");
        //    if (!result.Succeeded)
        //    {
        //        return BadRequest(new { Message = "Ошибка создания аккаунта.", Errors = result.Errors });
        //    }

        //    // Удаляем подтверждение из БД
        //    _context.EmailConfirmations.Remove(confirmEntry);
        //    await _context.SaveChangesAsync();

        //    return View("EmailConfirmed"); // Отображаем страницу с подтверждением
        //}



    }
}
