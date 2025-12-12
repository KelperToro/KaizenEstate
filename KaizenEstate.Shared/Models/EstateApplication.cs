using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KaizenEstate.Shared.Models
{
    public class EstateApplication
    {
        [Key]
        public int Id { get; set; }

        // Связь: Кто оставил заявку
        public int UserId { get; set; }

        // Связь: На какую квартиру
        public int ApartmentId { get; set; }

        // Статус заявки ("Новая", "Одобрена", "Отклонена")
        public string Status { get; set; } = "Новая";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Эти поля нужны, чтобы C# сам подтягивал данные (имя юзера, адрес квартиры)
        // [JsonIgnore] мы не ставим тут, но будем аккуратны в контроллере
        public User? User { get; set; }
        public Apartment? Apartment { get; set; }
    }
}