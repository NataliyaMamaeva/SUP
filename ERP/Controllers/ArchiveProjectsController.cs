using ERP.Models;
using ERP.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Newtonsoft.Json;

using ERP.Services;
using System.IO.Pipes;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using NuGet.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Text.Json;

using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using SkiaSharp;
//using ERP.Data.Migrations;
using Azure.Core;
using Microsoft.CodeAnalysis.Elfie.Model.Tree;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.DotNet.Scaffolding.Shared;
using System.Runtime;
using System.Security.Cryptography;


namespace ERP.Controllers
{

    public class ArchiveProjectsController : Controller
    {
        private readonly ErpContext _context;
      
        private readonly YandexDiskSettings _yandexSettings;
        private readonly HttpClient _httpClient;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IDataProtector _protector;

        public ArchiveProjectsController(ErpContext context, IOptions<YandexDiskSettings> yandexSettings, HttpClient httpClient, IDataProtectionProvider provider)
        {
            _context = context;
            _yandexSettings = yandexSettings.Value;
            _httpClient = httpClient;
            _protector = provider.CreateProtector("");

        }

        [Authorize]
        public IActionResult ArchiveProjects()
        {

            var accounts = _context.YandexAccounts.Select(acc => new YandexAccountViewModel
            {
                AccountId = acc.Id,
                Email = acc.Email,
                IsCurrent = acc.IsCurrent
            }).ToList();

            var clients = _context.Clients
                    .Include(c => c.Projects)
                        .ThenInclude(p => p.Items)
                    .Include(c => c.Projects)
                        .ThenInclude(p => p.Employee)
                    .Include(c => c.Requisites)
                    .Include(c => c.DeliveryAddresses)
                    .Include(c => c.Contacts)
                    .ToList() // Загружаем данные из БД перед обработкой
                    .Select(client => new ClientProjectsViewModel
                    {
                        ClientId = client.ClientId,
                        ClientTitle = client.Title,
                        City = client.City,
                        FirstRequestDate = client.FirstRequstDate,
                        Projects = client.Projects != null
                            ? client.Projects.Select(project => new ClientProjectViewModel
                            {
                                ProjectId = project.ProjectId,
                                IsArchived = project.IsArchived,
                                ProjectName = project.ProjectName,
                                EventDate = project.EventDate,
                                Deadline = project.Deadline,
                                PaymentTotal = project.PaymentTotal,
                                AdvanceRate = project.AdvanceRate,
                                ImagePath = project.IsArchived != true
                                   ?  project.Items?.FirstOrDefault()?.SketchPath ?? "" :
                                    $"\\{client.Title.Replace(" ", "_").Replace(":", "_")}\\{project.ProjectName?.Replace(" ", "_").Replace(":", "_")}_previewImage.jpg"
                                    , // Проверяем на null
                                EmployeeName = project.Employee?.EmployeeName ?? "Не назначен" // Проверяем на null
                            }).ToList()
                            : new List<ClientProjectViewModel>(), // Если null, создаём пустой список
                        Requisites = client.Requisites?
                            .Select(requisite => new ClientRequisiteViewModel
                            {
                                RequisiteId = requisite.RequisiteId,
                                FileTitle = requisite.FileTitle,
                                FilePath = requisite.FilePath,
                                Comment = requisite.Comment,
                                UploadedAt = requisite.UploadedAt
                            }).ToList() ?? new List<ClientRequisiteViewModel>(), // Если null, создаём пустой список
                        DeliveryAddresses = client.DeliveryAddresses?
                            .Select(address => new ClientDeliveryAddressViewModel
                            {
                                AddressId = address.AddressId,
                                DeliveryAddress = address.DeliveryAddress1
                            }).ToList() ?? new List<ClientDeliveryAddressViewModel>(), // Проверяем на null
                        Contacts = client.Contacts?
                            .Select(contact => new ClientContactViewModel
                            {
                                ContactId = contact.ContactId,
                                ContactName = contact.ContactName,
                                PhoneNumber = contact.PhoneNumber,
                                Email = contact.Email,
                                Passport = contact.Passport
                            }).ToList() ?? new List<ClientContactViewModel>(), // Проверяем на null
                        Accounts = accounts // Проверяем на null
                    })
                    .ToList();

            return View("ArchiveProjects", clients);
        }
        [Authorize]
        public ActionResult Gallery()
        {
            return View();
        }
       
        public async Task<IActionResult> AddAccountYandex(string code, [FromServices] IOptions<YandexDiskSettings> options)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Код авторизации не получен");

            var settings = options.Value;

            using var client = new HttpClient();

            string clientId = _yandexSettings.ClientId;
            string clientSecret = _yandexSettings.ClientSecret;
           
            var values = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "client_id", clientId},
                { "client_secret", clientSecret }
            };


            foreach (var value in values) {
                Console.WriteLine("key:" + value.Key);
                Console.WriteLine("value:" + value.Value);
            }

            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://oauth.yandex.ru/token", content);
            var responseString = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Yandex OAuth Response: {responseString}");

            var json = JObject.Parse(responseString);
            var accessToken = json["access_token"]?.ToString();
            var refreshToken = json["refresh_token"]?.ToString();

            if (accessToken == null)
                return BadRequest(responseString);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("OAuth", accessToken);
            var userResponse = await client.GetAsync("https://login.yandex.ru/info");
            var userResponseString = await userResponse.Content.ReadAsStringAsync();


     

            var userJson = JObject.Parse(userResponseString);
            var email = userJson["default_email"]?.ToString() ?? "Неизвестный аккаунт";

            foreach (var acc in _context.YandexAccounts)
            {
                acc.IsCurrent = false;
            }
          
            var account = new YandexAccount
            {
                Email = email,
                AccessToken = _protector.Protect(accessToken),
                RefreshToken = _protector.Protect(refreshToken),
                IsCurrent = true,
                ExpiryDate = DateTime.UtcNow.AddYears(1)
            };

            _context.YandexAccounts.Add(account);
            await _context.SaveChangesAsync();



            using var folderClient = new HttpClient();
            folderClient.Timeout = TimeSpan.FromMinutes(5);
            folderClient.DefaultRequestHeaders.ExpectContinue = false;
            folderClient.DefaultRequestHeaders.Add("Authorization", $"OAuth {accessToken}");
            string SUPpath = $"SUPArchive";

            var createFolderResponse = await client.PutAsync(
                $"https://cloud-api.yandex.net/v1/disk/resources?path={Uri.EscapeDataString(SUPpath)}",
                null
            );

            if (createFolderResponse.StatusCode != System.Net.HttpStatusCode.Created &&
                createFolderResponse.StatusCode != System.Net.HttpStatusCode.Conflict) // 409 - уже существует
            {
                Console.WriteLine($"Ошибка при создании папки: {createFolderResponse.StatusCode}");
                // return;
            }
            else
                Console.WriteLine("Папка проверена/создана.");
            return Redirect("/ArchiveProjects/ArchiveProjects");
        }

        [HttpPost]
        [Authorize]
        public IActionResult SetActiveAccount(int accountId)
        {
            Console.WriteLine("--------------------SetActiveAccount:- " + accountId);
            var account = _context.YandexAccounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null)
            {
                return BadRequest(new { success = false, message = "Аккаунт не найден" });
            }

            // Сбрасываем текущий активный аккаунт
            foreach (var acc in _context.YandexAccounts)
            {
                acc.IsCurrent = false;
            }

            // Назначаем новый активный аккаунт
            account.IsCurrent = true;
            _context.SaveChanges();

            return Ok(new { success = true });
        }
   
        public IActionResult GetConfig()
        {
            Console.WriteLine("GetConfig");
            return Ok(new
            {
                clientId = _yandexSettings.ClientId,
                redirectUri = _yandexSettings.RedirectUri
            });
        }


        [Authorize]
        public async Task<ActionResult> ArchiveProject()
        {
            int projectId = 0;
            Int32.TryParse(Request.Query["projectId"], out projectId);
            Console.WriteLine($"archive project id [{projectId}]"); 
            var account = _context.YandexAccounts.FirstOrDefault(a => a.IsCurrent == true);

            //string tempToken = "y0__xD99rw4GNuWAyDZzeGUElm_sjQdyELFBpQFzBw3aSCDJeqi";
            Console.WriteLine($"YandexAccountId [{account.Email}]");

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return NotFound("Проект не найден");
            var client = _context.Clients.FirstOrDefault(c => c.ClientId == project.ClientId);
            if (client == null)
                return NotFound("client not found");

            string clientTitle = client.Title.Replace(" ", "_").Replace(":", "_");
            string projectName = project.ProjectName.Replace(" ", "_").Replace(":", "_");
            string clientPath = Path.Combine("wwwroot", clientTitle);
            string projectPath = Path.Combine("wwwroot", clientTitle, projectName);

            try
            {  
                string sketchesDir = Path.Combine(projectPath, "Sketches");
                string previewImagePath = Path.Combine(clientPath, $"{projectName}_previewImage.jpg");

                if (!Directory.Exists(sketchesDir))
                {
                    Console.WriteLine("Папка sketches не найдена.");
                }

                else
                {
                    string firstImage = Directory.GetFiles(sketchesDir)
                                                 .FirstOrDefault(f => f.EndsWith(".jpg") || f.EndsWith(".png") || f.EndsWith(".JPG") || f.EndsWith(".PNG") || f.EndsWith(".JPEG") || f.EndsWith(".jpeg"));

                    if (firstImage == null)
                    {
                        Console.WriteLine("Изображение в sketches не найдено.");  
                    }
                    else 
                    {
                        Console.WriteLine("картинку пишем в " + previewImagePath);
                        Console.WriteLine("из " + firstImage);
                        ResizeImage(firstImage, previewImagePath, 300);
                    }
                    Console.WriteLine("прошли создание превью");
                }

                string archivePath = $"{projectName}.zip";
                if (System.IO.File.Exists(archivePath))
                {
                    try
                    {
                        System.IO.File.Delete(archivePath);
                        await Task.Delay(500); 
                    }
                    catch (IOException)
                    {
                        Console.WriteLine("Файл project.zip занят, повторная попытка удаления...");
                        await Task.Delay(1000); 
                        System.IO.File.Delete(archivePath);
                    }
                }
                Console.WriteLine("прошли удаление папки если она существует");

                ZipFile.CreateFromDirectory(projectPath, archivePath, CompressionLevel.Optimal, false);
                Console.WriteLine("прошли запись в архив");
                await Task.Delay(500);

                string decryptedToken;
                try
                {
                    decryptedToken = _protector.Unprotect(account.AccessToken);
                }
                catch (CryptographicException)
                {
                    Console.WriteLine("Ошибка расшифровки");
                    decryptedToken = account.AccessToken;
                }

                string uploadResult = await UploadToYandexDiskAsync(archivePath, clientTitle, decryptedToken);
                if (!uploadResult.StartsWith("Файл загружен на Яндекс.Диск.") && !uploadResult.Contains("Файл уже существует"))
                {
                    Console.WriteLine(uploadResult);
                    return BadRequest(uploadResult);
                }
                Console.WriteLine("прошли удаление работу с яндекс диском");

                System.IO.File.Delete(archivePath);
                Console.WriteLine("прошли удаление архива");

                await Task.Delay(2000);           
                DeleteDirectory(projectPath);
                Console.WriteLine("прошли удаление папки проекта");

                project.IsArchived = true;
                project.ArchiveAccountEmail = account.Email;
                _context.SaveChanges();
                Console.WriteLine("Проект успешно загружен на Яндекс.Диск. фсё");

                return Ok(uploadResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return BadRequest(ex.Message);
            }          
        }
        [Authorize]
        private async Task<string> UploadToYandexDiskAsync(string filePath, string clientName, string accessToken)
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromMinutes(5);
            client.DefaultRequestHeaders.ExpectContinue = false;
            client.DefaultRequestHeaders.Add("Authorization", $"OAuth {accessToken}");
            string SUPpath = $"SUPArchive/" + clientName;
    
            var createFolderResponse = await client.PutAsync(
                $"https://cloud-api.yandex.net/v1/disk/resources?path={Uri.EscapeDataString(SUPpath)}",
                null
            );

            if (createFolderResponse.StatusCode != System.Net.HttpStatusCode.Created &&
                createFolderResponse.StatusCode != System.Net.HttpStatusCode.Conflict) // 409 - уже существует
            {
                Console.WriteLine($"Ошибка при создании папки: {createFolderResponse.StatusCode}");
                // return;
            }
            else
                Console.WriteLine("Папка проверена/создана.");
          //  SUPArchive /

            string diskFilePath = $"{SUPpath}/{Path.GetFileName(filePath)}";
            Console.WriteLine($"download to disk/diskFilePath: {diskFilePath} ");
            var uploadUrlResponse = await client.GetAsync(
                $"https://cloud-api.yandex.net/v1/disk/resources/upload?path={Uri.EscapeDataString(diskFilePath)}");


            var content = await uploadUrlResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"Yandex Response: {content}");

            var json = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(content);
            if (!uploadUrlResponse.IsSuccessStatusCode)
            {      
                if (json.TryGetProperty("error", out var errorCode) &&
                    errorCode.GetString() == "DiskResourceAlreadyExistsError")
                {
                    Console.WriteLine("Файл уже существует, загрузка пропущена.");
                    return $"Файл уже существует: {diskFilePath}";
                }
                else
                {
                    Console.WriteLine($"Ошибка при получении ссылки загрузки: {content}");
                    return $"Ошибка при получении ссылки загрузки: {content}";
                }  
            }

            json.TryGetProperty("href", out var uploadUrl);   
            using var fileStream = System.IO.File.OpenRead(filePath);
            var uploadResponse = await client.PutAsync(uploadUrl.GetString(), new StreamContent(fileStream));

            //var uploadResponse = await client.PutAsync(uploadUrl.GetString(), new StreamContent(fileStream),
            //                               HttpCompletionOption.ResponseHeadersRead);

            if (uploadResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("Файл загружен на Яндекс.Диск.");
                return "Файл загружен на Яндекс.Диск.";
            }
            else
            {
                Console.WriteLine($"Ошибка загрузки: {uploadResponse.StatusCode}");
                return $"Ошибка загрузки: {uploadResponse.StatusCode}";
            }
        }



        [Authorize]
        [HttpGet("/ArchiveProjects/DownloadAndExtractProjectAsync")]
        public async Task<ActionResult> DownloadAndExtractProjectAsync()
        {
            int projectId = 0;
            Int32.TryParse(Request.Query["projectId"], out projectId);
            Console.WriteLine($"download from archive project id [{projectId}]");

            var project =  _context.Projects.FirstOrDefault(p => p.ProjectId == projectId);
            if (project == null)
                return NotFound("Проект не найден");

            //надо брать аккаунт по айди из проекта!!!------------------------------------------------------------------------

            var account = _context.YandexAccounts.FirstOrDefault(a => a.Email == project.ArchiveAccountEmail);
            if(account == null)
            {
                return NotFound("аккаунт яндекса не записан в проекте, либо проект не был архивирован");
            }
           
            var client = _context.Clients.FirstOrDefault(c => c.ClientId == project.ClientId);

            if (client == null)
                return NotFound("client not found");

            string clientTitle = client.Title.Replace(" ", "_").Replace(":", "_");
            string projectName = project.ProjectName.Replace(" ", "_").Replace(":", "_");

            string projectPath = Path.Combine("wwwroot", clientTitle, projectName);

            string diskPath = $"SUPArchive/{clientTitle}/{Path.GetFileName(projectName)}.zip";

           // string SUPpath = Path.Combine("SUPArchive", diskPath);

            //try
            //{

            if (!Directory.Exists(projectPath))
                {
                    Directory.CreateDirectory(projectPath);
                    Console.WriteLine($"Создана папка: {projectPath}");
                }

                string extractPath = Path.Combine(projectPath, "project.zip");


            string downloadResult = await DownloadFromYandexDiskAsync(diskPath, extractPath, _protector.Unprotect(account.AccessToken));
            //string downloadResult = await DownloadFromYandexDiskAsync(diskPath, extractPath, account.AccessToken);

            if (!downloadResult.StartsWith("Файл успешно скачан"))
                {
                    Console.WriteLine(downloadResult);
                    return BadRequest(downloadResult);
                }

                
                if (System.IO.File.Exists(extractPath))
                {
                    Console.WriteLine("Файл скачан, проверяем доступность...");

                    // Сбрасываем атрибуты, если они есть
                    System.IO.File.SetAttributes(extractPath, FileAttributes.Normal);

                    // Принудительно закрываем доступ к файлу
                    await Task.Delay(1000);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();


                // Теперь разархивируем
                //не разархивируется иногда: The process cannot access the file 'C:\Учёба\.NET\ERP\ERP\wwwroot\DAB_running\даб_с_кораблями\project.zip'
                //because it is being used by another process.

                ZipFile.ExtractToDirectory(extractPath, projectPath, true);
                    System.IO.File.Delete(extractPath);
                    Console.WriteLine("Проект разархивирован.");
                }
                else
                {
                    Console.WriteLine("Ошибка: архив не найден.");
                    return BadRequest("Ошибка: архив не найден.");
                }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Ошибка: {ex.Message}");
            //    return StatusCode(500, "Ошибка сервера: " + ex.Message);
            //}
            project.IsArchived = false;
            _context.SaveChanges();
            return Ok();
        }
        [Authorize]
        private async Task<string> DownloadFromYandexDiskAsync(string diskPath, string localPath, string accessToken)
        {

           
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(5);
                client.DefaultRequestHeaders.ExpectContinue = false;

                client.DefaultRequestHeaders.Add("Authorization", $"OAuth {accessToken}");

                // Получаем ссылку на скачивание
                Console.WriteLine("DownloadFromYandexDiskAsync");
                Console.WriteLine($"яндекс -- diskPath: {diskPath}");

                var response = await client.GetAsync($"https://cloud-api.yandex.net/v1/disk/resources/download?path={diskPath}");
                var content = await response.Content.ReadAsStringAsync();
                var json = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(content);

                if (!json.TryGetProperty("href", out var downloadUrl))
                {
                    Console.WriteLine("Не удалось получить ссылку для скачивания.");
                    return "Не удалось получить ссылку для скачивания.";
                }

                Console.WriteLine("ссыль получили ");
                var fileBytes = await client.GetByteArrayAsync(downloadUrl.GetString());
                Console.WriteLine("получили байты");
                await System.IO.File.WriteAllBytesAsync(localPath, fileBytes);

                Console.WriteLine("переписали байты на сервер ");
                return "Файл успешно скачан.";
            }
            catch (Exception ex) 
            {
                return $"Ошибка: {ex.Message}";
            }
 
        }
        [Authorize]
        public static void ResizeImage(string firstImage, string previewImagePath, int newHeight)
        {
            using (var image = System.Drawing.Image.FromFile(firstImage))
            {
                int newWidth = (int)((double)newHeight / image.Height * image.Width);
                using (var resized = new Bitmap(newWidth, newHeight))
                {
                    using (var graphics = Graphics.FromImage(resized))
                    {
                        graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                        graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                    }
                    resized.Save(previewImagePath, ImageFormat.Jpeg);
                }
            }
        }
        [Authorize]
        private void DeleteDirectory(string targetDir)
        {
            if (!Directory.Exists(targetDir))
                return;
            foreach (var file in Directory.GetFiles(targetDir))
            {
                try
                {
                    System.IO.File.SetAttributes(file, FileAttributes.Normal);
                    System.IO.File.Delete(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при удалении файла {file}: {ex.Message}");
                }
            }
            foreach (var dir in Directory.GetDirectories(targetDir))
            {
                DeleteDirectory(dir);
            }
            try
            {
                Directory.Delete(targetDir, true); // true - удаляет даже если есть файлы
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении папки {targetDir}: {ex.Message}");
            }
        }



        //private void DeleteDirectory(string targetDir)
        //{
        //    if (!Directory.Exists(targetDir))
        //        return;

        //    foreach (var file in Directory.GetFiles(targetDir, "*", SearchOption.AllDirectories))
        //    {
        //        try
        //        {
        //            System.IO.File.SetAttributes(file, FileAttributes.Normal);
        //            System.IO.File.Delete(file);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Ошибка при удалении файла {file}: {ex.Message}");
        //        }
        //    }

        //    foreach (var dir in Directory.GetDirectories(targetDir))
        //    {
        //        DeleteDirectory(dir);
        //    }

        //    try
        //    {
        //        Directory.Delete(targetDir, true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Ошибка при удалении папки {targetDir}: {ex.Message}");
        //    }
        //}



        //public void ArchiveFile(string filePath, string archivePath)
        //{
        //    // Создаём архив и добавляем в него файл
        //    using (FileStream zipToCreate = new FileStream(archivePath, FileMode.Create))
        //    {
        //        using (ZipArchive archive = new ZipArchive(zipToCreate, ZipArchiveMode.Create))
        //        {
        //            ZipArchiveEntry readmeEntry = archive.CreateEntry(Path.GetFileName(filePath));
        //            using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
        //            {
        //                using (FileStream fs = new FileStream(filePath, FileMode.Open))
        //                {
        //                    fs.CopyTo(writer.BaseStream);
        //                }
        //            }
        //        }
        //    }
        //}

        //public async Task UploadFileToYandexDisk(string filePath, string accessToken)
        //{
        //    var fileInfo = new FileInfo(filePath);
        //    var fileName = fileInfo.Name;

        //    // Получаем ссылку для загрузки
        //    var getUploadUrlResponse = await GetUploadUrl(accessToken);
        //    var uploadUrl = getUploadUrlResponse.UploadUrl;

        //    using (var client = new HttpClient())
        //    {
        //        var content = new MultipartFormDataContent
        //{
        //    { new StreamContent(new FileStream(filePath, FileMode.Open)), "file", fileName }
        //};

        //        var response = await client.PutAsync(uploadUrl, content);
        //        response.EnsureSuccessStatusCode();  // Выбрасывает исключение, если запрос не удался
        //    }
        //}

        //private async Task<dynamic> GetUploadUrl(string accessToken)
        //{
        //    using (var client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Add("Authorization", $"OAuth {accessToken}");

        //        var response = await client.GetAsync("https://cloud-api.yandex.net/v1/disk/resources/upload?path=/path/to/your/file.zip");
        //        response.EnsureSuccessStatusCode();

        //        var responseBody = await response.Content.ReadAsStringAsync();
        //        return JsonConvert.DeserializeObject<dynamic>(responseBody);
        //    }
        //}

        //public void ExtractFileFromArchive(string archivePath, string extractPath)
        //{
        //    // Открываем архив
        //    using (FileStream zipToOpen = new FileStream(archivePath, FileMode.Open))
        //    {
        //        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
        //        {
        //            foreach (ZipArchiveEntry entry in archive.Entries)
        //            {
        //                // Извлекаем файл в указанную директорию
        //                string destinationPath = Path.Combine(extractPath, entry.FullName);
        //                entry.ExtractToFile(destinationPath, overwrite: true);
        //            }
        //        }
        //    }
        //}

        //private void ResizeImage(string inputPath, string outputPath)
        //{
        //    using var input = System.IO.File.OpenRead(inputPath);
        //    using var bitmap = SKBitmap.Decode(input);

        //    int newHeight = 300;
        //    int newWidth = bitmap.Width * newHeight / bitmap.Height;

        //    using var resized = bitmap.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
        //    using var image = SKImage.FromBitmap(resized);
        //    using var output = System.IO.File.OpenWrite(outputPath);

        //    image.Encode(SKEncodedImageFormat.Jpeg, 90).SaveTo(output);
        //    Console.WriteLine("Создано превью изображение.");
        //}

    }
}
    

