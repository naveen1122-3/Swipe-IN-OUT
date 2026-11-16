public class SwipeLog
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int ManagerId { get; set; }

    public int ProjectId { get; set; }

    public DateTime SwipeTime { get; set; }

    public string SwipeType { get; set; } = string.Empty;
}