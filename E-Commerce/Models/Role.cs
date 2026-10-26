namespace E_Commerce.Models
{
    public class Role
    {
        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection<UserRole> UserRoles { get; set; }
            = new List<UserRole>();
    }
}
