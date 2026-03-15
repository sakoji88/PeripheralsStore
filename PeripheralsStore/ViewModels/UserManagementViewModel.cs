namespace PeripheralsStore.ViewModels;

public class UserManagementViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public bool IsBanned { get; set; }
    public DateTime CreatedAt { get; set; }
}
