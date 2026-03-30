using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vet_Master.Models;

public class Vaccination
{
    public int Id { get; set; }
    public int AnimalCardId { get; set; }

    public string VaccineName { get; set; } = string.Empty;  // Назва вакцини
    public DateTime VaccinationDate { get; set; }

    public AnimalCard AnimalCard { get; set; } = null!;
}