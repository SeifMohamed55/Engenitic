namespace GraduationProject.Domain.Models
{

    public interface IEntity<T>
    {
        T Id { get; set; }
    }
}
