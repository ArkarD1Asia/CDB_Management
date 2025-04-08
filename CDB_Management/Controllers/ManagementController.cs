using CDB_Management.Models.Management;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Globalization;

namespace CDB_Management.Controllers
{
    public class ManagementController : Controller
    {
        private readonly IConfiguration _configuration;
        public ManagementController(IConfiguration configuration)
        {
               _configuration = configuration;
        }

        #region Issue
        [HttpGet]
        public async Task<IActionResult> IssueManagement()
        {
            return View(new IssueManagementViewModel());
        }
        [HttpGet]
        public async Task<IActionResult> FetchIssueData(string bankCode, string? fromDate, string? toDate, string? mainProblem, int page, int pageSize)
        {
            IssueManagementViewModel issueList = new IssueManagementViewModel();
            using (MySqlConnection conn = new MySqlConnection(_configuration.GetValue<string>("ConnectString_CDB:FullNameConnection_cdb")))
            {
                conn.Open();
                IssueManagement issue = new IssueManagement();
                string query = "SELECT * FROM IssuesCategory i JOIN ContractMaster c ON c.Contract_No = i.Contract_No WHERE 1=1";
                if (!(string.IsNullOrEmpty(bankCode)))
                {
                    query += " AND c.Bank_Name = @bankName";
                }
                if (!(string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate)))
                {
                    query += " AND i.Open_Date >=@fromDate AND i.Open_Date < @toDate";
                }
                if (!string.IsNullOrEmpty(mainProblem))
                {
                    query += " AND i.MainProblem_Name=@mainproblem";
                }
                using (var cmd = new MySqlCommand(query, conn))
                {
                    if (!(string.IsNullOrEmpty(bankCode)))
                    {
                        cmd.Parameters.AddWithValue("@bankName", bankCode.ToUpper());
                    }
                    if (!(string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate)))
                    {
                        cmd.Parameters.AddWithValue("@fromDate", fromDate + " 00:00:00");
                        cmd.Parameters.AddWithValue("@toDate", toDate + " 23:59:59");
                    }
                    if (!string.IsNullOrEmpty(mainProblem))
                    {
                        cmd.Parameters.AddWithValue("@mainproblem", mainProblem);
                    }
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["Open_Date"] != DBNull.Value)
                            {
                                DateTime xValue = Convert.ToDateTime(reader["Open_Date"]);
                                issue.open_date = xValue.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                issue.open_date = "-";
                            }
                            issueList.issues.Add(new IssueManagement()
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                open_date = issue.open_date,
                                job_id = reader["Job_ID"].ToString(),
                                contract_no = reader["Contract_No"].ToString(),
                                mainproblem_name = reader["MainProblem_Name"].ToString(),
                                problem_inform = reader["Problem_Inform"].ToString(),
                                problem_solution = reader["Problem_Solution"].ToString(),
                                mainsolution_name = reader["MainSolution_Name"].ToString(),
                                subproblem_name = reader["SubProblem_Name"].ToString(),
                                subsolution_name = reader["SubSolution_Name"].ToString(),
                                update_date = reader["Update_Date"].ToString(),
                                update_by = reader["Update_By"].ToString(),
                                remark = reader["Remark"].ToString()

                            });
                        }
                    }
                }

                //Apply pagination
                int issueCount = issueList.issues.Count;
                int totalPages = (int)Math.Ceiling((double)issueCount / pageSize);
                var paginatedIssueList = issueList.issues.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                // Optionally return the list as JSON for the frontend
                return Json(new
                {
                    success = true,
                    data = paginatedIssueList,
                    totalRecords = issueCount,
                    currentPage = page,
                    totalPages = totalPages,
                    pageSize = pageSize
                });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetIssueData(int Id)
        {
            IssueManagementViewModel issueList = new IssueManagementViewModel();
            IssueManagement issue = new IssueManagement();
            string conn = _configuration.GetValue<string>("ConnectString_CDB:FullNameConnection_cdb");
            using (MySqlConnection connection = new MySqlConnection(conn))
            {
                await connection.OpenAsync();
                string query = "SELECT * FROM issuesCategory WHERE Id=@id";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@id", Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (reader["Open_Date"] != DBNull.Value)
                            {
                                DateTime xValue = Convert.ToDateTime(reader["Open_Date"]);
                                issue.open_date = xValue.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                            }
                            else
                            {
                                issue.open_date = "-";
                            }
                            issue.job_id = reader["Job_ID"].ToString();
                            issue.contract_no = reader["Contract_No"].ToString();
                            issue.mainproblem_name = reader["MainProblem_Name"].ToString();
                            issue.problem_inform = reader["Problem_Inform"].ToString();
                            issue.problem_solution = reader["Problem_Solution"].ToString();
                            issue.mainsolution_name = reader["MainSolution_Name"].ToString();
                            issue.subproblem_name = reader["SubProblem_Name"].ToString();
                            issue.subsolution_name = reader["SubSolution_Name"].ToString();
                        }
                    }
                    issueList.issueManagement = issue;
                }
            }

            return PartialView("_PartialIssueManagement", issueList);
        }
        [HttpGet]
        public IActionResult ExportIssueToExcel(string bankCode, string? fromDate, string? toDate, string? mainProblem)
        {
            using (var connection = new MySqlConnection(_configuration.GetValue<string>("ConnectString_CDB:FullNameConnection_cdb")))
            {
                connection.Open();
                string query = "SELECT * FROM IssuesCategory i JOIN ContractMaster c ON c.Contract_No = i.Contract_No WHERE 1=1";
                if (!(string.IsNullOrEmpty(bankCode)))
                {
                    query += " AND c.Bank_Name = @bankName";
                }
                if (!(string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate)))
                {
                    query += " AND i.Open_Date >=@fromDate AND i.Open_Date < @toDate";
                }
                if (!string.IsNullOrEmpty(mainProblem))
                {
                    query += " AND i.MainProblem_Name=@mainproblem";
                }
                using (var cmd = new MySqlCommand(query, connection))
                {
                    if (!(string.IsNullOrEmpty(bankCode)))
                    {
                        cmd.Parameters.AddWithValue("@bankName", bankCode.ToUpper());
                    }
                    if (!(string.IsNullOrEmpty(fromDate) && string.IsNullOrEmpty(toDate)))
                    {
                        cmd.Parameters.AddWithValue("@fromDate", fromDate + " 00:00:00");
                        cmd.Parameters.AddWithValue("@toDate", toDate + " 23:59:59");
                    }
                    if (!string.IsNullOrEmpty(mainProblem))
                    {
                        cmd.Parameters.AddWithValue("@mainproblem", mainProblem);
                    }
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    using (var reader = cmd.ExecuteReader())
                    {
                        using (var package = new ExcelPackage())
                        {
                            var worksheet = package.Workbook.Worksheets.Add("IssueExport");

                            // Add headers
                            worksheet.Cells[1, 1].Value = "Open Date";
                            worksheet.Cells[1, 2].Value = "Job ID";
                            worksheet.Cells[1, 3].Value = "Contract No";
                            worksheet.Cells[1, 4].Value = "Main Problem Name";
                            worksheet.Cells[1, 5].Value = "Problem Inform";
                            worksheet.Cells[1, 6].Value = "Problem Solution";
                            worksheet.Cells[1, 7].Value = "Main Solution Name";
                            worksheet.Cells[1, 8].Value = "Sub Problem Name";
                            worksheet.Cells[1, 9].Value = "Sub Solution Name";

                            using (var range = worksheet.Cells[1, 1, 1, 9]) // Apply to all header cells
                            {
                                range.Style.Font.Bold = true; // Bold font
                                range.Style.Font.Size = 14; // Larger font size
                                range.Style.Fill.PatternType = ExcelFillStyle.Solid; // Set fill pattern to solid
                                range.Style.Fill.BackgroundColor.SetColor(Color.Orange); // Set background color
                                range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center; // Center text
                                range.Style.VerticalAlignment = ExcelVerticalAlignment.Center; // Center vertically
                            }
                            int row = 2;
                            while (reader.Read())
                            {
                                worksheet.Cells[row, 1].Value = reader["Open_Date"] != DBNull.Value ? reader.GetDateTime("Open_Date").ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture) : null;
                                worksheet.Cells[row, 2].Value = reader["Job_ID"] != DBNull.Value ? reader["Job_ID"] : null;
                                worksheet.Cells[row, 3].Value = reader["Contract_No"] != DBNull.Value ? reader["Contract_No"] : null;
                                worksheet.Cells[row, 4].Value = reader["MainProblem_Name"] != DBNull.Value ? reader["MainProblem_Name"] : null;
                                worksheet.Cells[row, 5].Value = reader["Problem_Inform"] != DBNull.Value ? reader["Problem_Inform"] : null;
                                worksheet.Cells[row, 6].Value = reader["Problem_Solution"] != DBNull.Value ? reader["Problem_Solution"] : null;
                                worksheet.Cells[row, 7].Value = reader["MainSolution_Name"] != DBNull.Value ? reader["MainSolution_Name"] : null;
                                worksheet.Cells[row, 8].Value = reader["SubProblem_Name"] != DBNull.Value ? reader["SubProblem_Name"] : null;
                                worksheet.Cells[row, 9].Value = reader["SubSolution_Name"] != DBNull.Value ? reader["SubSolution_Name"] : null;
                                row++;
                            }

                            worksheet.Cells.AutoFitColumns();

                            string excelName = bankCode.ToUpper() + " IssueList_" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx";

                            var stream = new MemoryStream();
                            package.SaveAs(stream);
                            stream.Position = 0;

                            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
