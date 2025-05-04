using System.Diagnostics;
using FRONT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace FRONT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> LoginAjax([FromForm] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, error = "Datos inválidos." });

            string token = null;
            string soapServiceUrl = _configuration["SoapServiceUrl"] ?? "";
            try
            {
                var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <ValidarYObtenerCedula xmlns=""http://tempuri.org/"">
      <email>{model.Email}</email>
      <password>{model.Password}</password>
    </ValidarYObtenerCedula>
  </soap:Body>
</soap:Envelope>";

                using var httpClient = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, soapServiceUrl);
                request.Content = new StringContent(soapEnvelope, System.Text.Encoding.UTF8, "text/xml");
                request.Headers.Add("SOAPAction", "http://tempuri.org/ValidarYObtenerCedula");

                var response = await httpClient.SendAsync(request);
                var responseContent = await response.Content.ReadAsStringAsync();

                var tokenStart = responseContent.IndexOf("<ValidarYObtenerCedulaResult>") + "<ValidarYObtenerCedulaResult>".Length;
                var tokenEnd = responseContent.IndexOf("</ValidarYObtenerCedulaResult>");
                if (tokenStart > 0 && tokenEnd > tokenStart)
                {
                    token = responseContent.Substring(tokenStart, tokenEnd - tokenStart);
                }
            }
            catch
            {
                return Json(new { success = false, error = "Error al conectar con el servicio de autenticación." });
            }

            if (string.IsNullOrEmpty(token))
            {
                return Json(new { success = false, error = "Credenciales inválidas." });
            }

            HttpContext.Session.SetString("JwtToken", token);
            return Json(new { success = true });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
