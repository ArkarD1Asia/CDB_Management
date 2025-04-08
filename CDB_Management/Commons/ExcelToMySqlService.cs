using MySql.Data.MySqlClient;
using OfficeOpenXml;

namespace CDB_Management.Commons
{
    public class ExcelToMySqlService
    {
        private readonly string? _connectionString;
        public ExcelToMySqlService(string? connectionString)
        {
            _connectionString = connectionString;
        }
        #region Issue Excel
        public async Task ImportIssuelDataAsync(Stream excelStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(excelStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;


                for (int row = 2; row <= rowCount; row++) // Assuming the first row is the header
                {
                    try
                    {
                        using (var connection = new MySqlConnection(_connectionString))
                        {
                            await connection.OpenAsync();
                            var columnEValue = worksheet.Cells[row, 5].Text; // Column E (5th column)
                            var columnDValue = worksheet.Cells[row, 4].Text; // Column D (4th column)


                            // Example condition: If column E has value "Yes", update column D value
                            if (columnEValue.IndexOf("Offline", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                columnDValue = "Software"; // Update value in column D
                            }
                            else if (columnEValue.IndexOf("Offline", StringComparison.OrdinalIgnoreCase) >= 0 && columnEValue.IndexOf("Cancel", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                columnDValue = "-";
                            }
                            else if (columnEValue.IndexOf("Disconnect", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                columnDValue = "Network";
                            }
                            else if (columnEValue.IndexOf("เข้า Online", StringComparison.OrdinalIgnoreCase) >= 0 || columnEValue.IndexOf("Online เครื่อง", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                columnDValue = "Other";
                            }
                            else if ((columnEValue.IndexOf("HW Cash Dispenser Error", StringComparison.OrdinalIgnoreCase) >= 0 && columnDValue.IndexOf("Customer", StringComparison.OrdinalIgnoreCase) >= 0) || (columnEValue.IndexOf("Card Reader Error", StringComparison.OrdinalIgnoreCase) >= 0 && columnDValue.IndexOf("Customer", StringComparison.OrdinalIgnoreCase) >= 0))
                            {
                                columnDValue = "Customer";
                            }
                            else if (columnEValue.IndexOf("HW Cash Dispenser Error", StringComparison.OrdinalIgnoreCase) >= 0 || columnEValue.IndexOf("Card Reader Error", StringComparison.OrdinalIgnoreCase) >= 0 || columnEValue.IndexOf("ชุดจ่ายขัดข้อง", StringComparison.OrdinalIgnoreCase) >= 0 || columnEValue.IndexOf("EPP", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                columnDValue = "Hardware";
                            }
                            else
                            {
                                columnDValue = "";
                            }

                            var cellValue_opendate = worksheet.Cells[row, 1].Value;
                            DateTime? openDate = null;

                            if (cellValue_opendate != null)
                            {
                                if (cellValue_opendate is double)
                                {
                                    openDate = DateTime.FromOADate((double)cellValue_opendate);
                                }
                                else
                                {
                                    openDate = DateTime.Parse(cellValue_opendate.ToString());
                                }
                            }

                            //var opendate = Convert.ToInt64(worksheet.Cells[row, 1].Value);
                            var jobId = worksheet.Cells[row, 2].Value?.ToString();
                            var contractNo = worksheet.Cells[row, 3].Value?.ToString();
                            var mainproblemName = columnDValue;
                            var problemInform = worksheet.Cells[row, 5].Value?.ToString();
                            var problemSolution = worksheet.Cells[row, 6].Value?.ToString();
                            var mainsolutionName = worksheet.Cells[row, 7].Value?.ToString();
                            var subproblemName = worksheet.Cells[row, 8].Value?.ToString();
                            var subsolutionName = worksheet.Cells[row, 9].Value?.ToString();
                            var updateDate = DateTime.Now;
                            var updateBy = "System";
                            var remark = "Imported from Excel";

                            var query = @"
                        INSERT INTO IssuesCategory 
                        (Job_ID, Contract_No, Problem_Inform, MainProblem_Name, Problem_Solution, MainSolution_Name, SubProblem_Name, SubSolution_Name,Open_Date,Update_Date, Update_By, Remark) 
                        VALUES 
                        (@JobID, @ContractNo, @ProblemInform, @MainProblemName, @ProblemSolution, @MainSolutionName, @SubProblemName, @SubSolutionName, @OpenDate, @UpdateDate, @UpdateBy, @Remark)
                        ON DUPLICATE KEY UPDATE 
                            MainProblem_Name = @MainProblemName,
                            Problem_Solution = @ProblemSolution,
                            MainSolution_Name = @MainSolutionName,
                            SubProblem_Name = @SubProblemName,
                            SubSolution_Name = @SubSolutionName,
                            Open_Date = @OpenDate,
                            Update_Date = @UpdateDate,
                            Update_By = @UpdateBy,
                            Remark =  @Remark";

                            using (var command = new MySqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@OpenDate", openDate);
                                command.Parameters.AddWithValue("@JobID", jobId);
                                command.Parameters.AddWithValue("@ContractNo", contractNo);
                                command.Parameters.AddWithValue("@MainProblemName", mainproblemName);
                                command.Parameters.AddWithValue("@ProblemInform", problemInform);
                                command.Parameters.AddWithValue("@ProblemSolution", problemSolution);
                                command.Parameters.AddWithValue("@MainSolutionName", mainsolutionName);
                                command.Parameters.AddWithValue("@SubProblemName", subproblemName);
                                command.Parameters.AddWithValue("@SubSolutionName", subsolutionName);
                                command.Parameters.AddWithValue("@UpdateDate", updateDate);
                                command.Parameters.AddWithValue("@UpdateBy", updateBy);
                                command.Parameters.AddWithValue("@Remark", remark);

                                await command.ExecuteNonQueryAsync();
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        // Log the error or handle it accordingly
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }

            }
        }
        #endregion
    }
}
