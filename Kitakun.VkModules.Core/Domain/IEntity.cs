namespace Kitakun.VkModules.Core.Domain
{
    using System.ComponentModel.DataAnnotations;

    public interface IEntity
    {
        [Key]
        int Id { get; set; }
    }
}
