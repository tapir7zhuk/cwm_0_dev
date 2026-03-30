using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vet_Master.Data;
using Vet_Master.Models;

namespace Vet_Master.Services;

public class RecordService(AppDbContext db)
{
    // Всі записи конкретної тварини
    public Task<List<Record>> GetByAnimalAsync(int animalCardId) =>
        db.Records
          .Where(r => r.AnimalCardId == animalCardId)
          .OrderByDescending(r => r.VisitDate)
          .ToListAsync();

    // Зараз лікуються (для статистики)
    public Task<List<Record>> GetActiveAsync() =>
        db.Records
          .Include(r => r.AnimalCard)
          .Where(r => r.TreatmentEnd == null || r.TreatmentEnd >= DateTime.Today)
          .ToListAsync();

    // Завершили лікування минулого місяця (для статистики)
    public Task<List<Record>> GetFinishedLastMonthAsync()
    {
        var firstDay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        return db.Records
                 .Include(r => r.AnimalCard)
                 .Where(r => r.TreatmentEnd >= firstDay && r.TreatmentEnd <= lastDay)
                 .ToListAsync();
    }

    public async Task AddAsync(Record record)
    {
        db.Records.Add(record);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Record record)
    {
        db.Records.Update(record);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var record = await db.Records.FindAsync(id);
        if (record is not null)
        {
            db.Records.Remove(record);
            await db.SaveChangesAsync();
        }
    }
}