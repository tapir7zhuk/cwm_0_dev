using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vet_Master.Data;
using Vet_Master.Models;

namespace Vet_Master.Services;

public class VaccinationService(AppDbContext db)
{
    public Task<List<Vaccination>> GetByAnimalAsync(int animalCardId) =>
        db.Vaccinations
          .Where(v => v.AnimalCardId == animalCardId)
          .OrderByDescending(v => v.VaccinationDate)
          .ToListAsync();

    public async Task AddAsync(Vaccination vaccination)
    {
        db.Vaccinations.Add(vaccination);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var v = await db.Vaccinations.FindAsync(id);
        if (v is not null)
        {
            db.Vaccinations.Remove(v);
            await db.SaveChangesAsync();
        }
    }
}