using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vet_Master.Data;
using Vet_Master.Models;

namespace Vet_Master.Services;

public class AnimalCardService(AppDbContext db)
{
    public Task<List<AnimalCard>> GetAllAsync() =>
        db.AnimalCards
          .Include(a => a.Records)
          .Include(a => a.Vaccinations)
          .OrderBy(a => a.Name)
          .ToListAsync();

    public Task<AnimalCard?> GetByIdAsync(int id) =>
        db.AnimalCards
          .Include(a => a.Records)
          .Include(a => a.Vaccinations)
          .FirstOrDefaultAsync(a => a.Id == id);

    public async Task AddAsync(AnimalCard animal)
    {
        db.AnimalCards.Add(animal);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AnimalCard animal)
    {
        db.AnimalCards.Update(animal);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var animal = await db.AnimalCards.FindAsync(id);
        if (animal is not null)
        {
            db.AnimalCards.Remove(animal);
            await db.SaveChangesAsync();
        }
    }

}