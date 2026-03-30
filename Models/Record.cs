using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vet_Master.Models;

public class Record
{
    public int Id { get; set; }
    public int AnimalCardId { get; set; }

    public DateTime VisitDate { get; set; }
    public string Complaints { get; set; } = string.Empty;      // Скарги
    public string Diagnosis { get; set; } = string.Empty;       // Діагноз
    public string Recommendations { get; set; } = string.Empty; // Рекомендації

    public DateTime? TreatmentStart { get; set; }
    public DateTime? TreatmentEnd { get; set; }

    // Статус визначається автоматично: якщо TreatmentEnd < сьогодні або null
    public bool IsActive => TreatmentEnd == null || TreatmentEnd >= DateTime.Today;

    // Навігація
    public AnimalCard AnimalCard { get; set; } = null!;
}