using Microsoft.AspNetCore.Mvc;

namespace ERP.Controllers
{
    public class Statistics : Controller
    {
        public IActionResult StatisticsView()
        {
            return View();
        }
    }
}
