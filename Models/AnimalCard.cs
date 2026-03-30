using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
namespace Vet_Master.Models;

public class AnimalCard
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;   // Вид: Кіт, Пес...
    public string Breed { get; set; } = string.Empty;     // Порода
    public int Age { get; set; }
    public string Sex { get; set; } = string.Empty;       // Стать
    public string OwnerName { get; set; } = string.Empty;

    [RegularExpression(@"^\+380\d{9}$",
            ErrorMessage = "Телефон має бути у форматі +380XXXXXXXXX")]
    public string OwnerPhone { get; set; } = string.Empty;
    public string MediaPath { get; set; } = string.Empty; // Шлях/URL файлу

    // Навігаційні властивості
    public ICollection<Record> Records { get; set; } = new List<Record>();
    public ICollection<Vaccination> Vaccinations { get; set; } = new List<Vaccination>();
}