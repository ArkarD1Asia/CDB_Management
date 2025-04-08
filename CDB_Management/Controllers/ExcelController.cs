using CDB_Management.Commons;
using Microsoft.AspNetCore.Mvc;

namespace CDB_Management.Controllers
{
    public class ExcelController : Controller
    {
        private readonly ExcelToMySqlService _excelService;
        public ExcelController(IConfiguration configuration)
        {
            string? connectionString = configuration.GetSection("ConnectString_CDB").GetValue<string>("FullNameConnection_cdb");
            _excelService = new ExcelToMySqlService(connectionString);
        }
        #region Issue Excel
        [HttpPost]
        public async Task<IActionResult> UploadIssueExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload a valid Excel file.");
            }

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                try
                {
                    await _excelService.ImportIssuelDataAsync(stream);
                    return Ok("Data imported successfully.");
                }
                catch (Exception ex)
                {
                    // Log the error if necessary
                    return StatusCode(500, $"An error occurred: {ex.Message}");
                }
            }
        }
        #endregion
    }
}
