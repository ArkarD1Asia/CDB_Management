namespace CDB_Management.Models.Management
{
    public class IssueManagementViewModel
    {
        public IssueManagement issueManagement { get; set; }
        public List<IssueManagement> issues { get; set; } = new List<IssueManagement>();
    }
    public class IssueManagement
    {
        public int Id { get; set; }
        public string? open_date { get; set; }
        public string? job_id { get; set; }
        public string? contract_no { get; set; }
        public string? mainproblem_name { get; set; }
        public string? problem_inform { get; set; }
        public string? problem_solution { get; set; }
        public string? mainsolution_name { get; set; }
        public string? subproblem_name { get; set; }
        public string? subsolution_name { get; set; }
        public string? update_date { get; set; }
        public string? update_by { get; set; }
        public string? remark { get; set; }
    }
}
