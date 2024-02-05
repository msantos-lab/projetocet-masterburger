using Microsoft.AspNetCore.Mvc;

namespace MasterBurger.Controllers {
	public class ErrorController : Controller {

    [Route("/Error/NotFound")]
    public IActionResult NotFound() {
      return View("NotFound"); 
    }


    [Route("/Error/{statusCode}")]
    public IActionResult HandleErrorCode(int statusCode) {
      return View("Error", statusCode);
    }
  }
}
