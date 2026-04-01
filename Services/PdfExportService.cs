using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Vet_Master.Models;
using QColors = QuestPDF.Helpers.Colors;
using QContainer = QuestPDF.Infrastructure.IContainer;

namespace Vet_Master.Services;

public class PdfExportService
{
    public PdfExportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // ═══════════════════════════════════
    // 1. Статистика
    // ═══════════════════════════════════
    public void ExportStatistics(
        int activeCount,
        int finishedLastMonth,
        int totalAnimals,
        int vaccinatedCount,
        List<Record> activeRecords,
        List<(string Species, int Count)> bySpecies,
        string outputPath)
    {
        Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(11).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader("Статистика клініки")(c));
                page.Footer().Element(c => ComposeFooter()(c));

                page.Content().Column(col =>
                {
                    col.Item().Text($"Звіт сформовано: {DateTime.Now:dd.MM.yyyy HH:mm}")
                       .FontSize(9).FontColor(QColors.Grey.Medium);
                    col.Item().PaddingBottom(12);

                    // Картки з цифрами
                    col.Item().Text("Загальні показники")
                       .FontSize(13).SemiBold().FontColor(QColors.Grey.Darken2);
                    col.Item().PaddingBottom(6);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                            c.RelativeColumn();
                        });

                        StatCard(table, "ЗАРАЗ ЛІКУЮТЬСЯ", activeCount.ToString(), "#185FA5");
                        StatCard(table, "ЗАВЕРШИЛИ МИН. МІСЯЦЬ", finishedLastMonth.ToString(), "#0F6E56");
                        StatCard(table, "ВСЬОГО ТВАРИН", totalAnimals.ToString(), "#444444");
                        StatCard(table, "ВАКЦИНОВАНІ", vaccinatedCount.ToString(), "#7B3FA0");
                    });

                    col.Item().PaddingBottom(16);

                    // По видах
                    col.Item().Text("Розподіл по видах")
                       .FontSize(13).SemiBold().FontColor(QColors.Grey.Darken2);
                    col.Item().PaddingBottom(6);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Background(QColors.Grey.Lighten3).Padding(6)
                             .Text("Вид").SemiBold();
                            h.Cell().Background(QColors.Grey.Lighten3).Padding(6)
                             .Text("Кількість").SemiBold();
                        });

                        foreach (var (species, count) in bySpecies.OrderByDescending(x => x.Count))
                        {
                            table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2)
                                 .Padding(6).Text(species);
                            table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2)
                                 .Padding(6).Text(count.ToString());
                        }
                    });

                    col.Item().PaddingBottom(16);

                    // Хто лікується
                    col.Item().Text("Хто зараз лікується")
                       .FontSize(13).SemiBold().FontColor(QColors.Grey.Darken2);
                    col.Item().PaddingBottom(6);

                    if (!activeRecords.Any())
                    {
                        col.Item().Text("Немає активних випадків.")
                           .FontColor(QColors.Grey.Medium);
                    }
                    else
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(2);
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                foreach (var title in new[] { "Тварина", "Діагноз", "Початок", "Кінець" })
                                    h.Cell().Background(QColors.Grey.Lighten3)
                                     .Padding(6).Text(title).SemiBold();
                            });

                            foreach (var r in activeRecords)
                            {
                                table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2)
                                     .Padding(6).Text(r.AnimalCard?.Name ?? "—");
                                table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2)
                                     .Padding(6).Text(r.Diagnosis);
                                table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2)
                                     .Padding(6).Text(r.TreatmentStart?.ToString("dd.MM.yyyy") ?? "—");
                                table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2)
                                     .Padding(6).Text(r.TreatmentEnd?.ToString("dd.MM.yyyy") ?? "—");
                            }
                        });
                    }
                });
            });
        })
        .GeneratePdf(outputPath);
    }

    // ═══════════════════════════════════
    // 2. Конкретний випадок хвороби
    // ═══════════════════════════════════
    public void ExportRecord(AnimalCard animal, Record record, string outputPath)
    {
        Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(11).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader("Медичний запис")(c));
                page.Footer().Element(c => ComposeFooter()(c));

                page.Content().Column(col =>
                {
                    col.Item().Text("Дані тварини та власника")
                       .FontSize(13).SemiBold().FontColor(QColors.Grey.Darken2);
                    col.Item().PaddingBottom(8);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });

                        InfoRow(table, "Ім'я", animal.Name, "Вид", animal.Species);
                        InfoRow(table, "Порода", animal.Breed, "Вік", $"{animal.Age} р.");
                        InfoRow(table, "Стать", animal.Sex, "", "");
                        InfoRow(table, "Власник", animal.OwnerName, "Телефон", animal.OwnerPhone);
                    });

                    col.Item().PaddingBottom(16);

                    col.Item().Text("Медичний запис")
                       .FontSize(13).SemiBold().FontColor(QColors.Grey.Darken2);
                    col.Item().PaddingBottom(8);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1);
                            c.RelativeColumn(3);
                        });

                        LabelValue(table, "Дата прийому", record.VisitDate.ToString("dd.MM.yyyy"));
                        LabelValue(table, "Діагноз", record.Diagnosis);
                        LabelValue(table, "Скарги", string.IsNullOrWhiteSpace(record.Complaints)
                                                                   ? "—" : record.Complaints);
                        LabelValue(table, "Початок лікування", record.TreatmentStart?.ToString("dd.MM.yyyy") ?? "—");
                        LabelValue(table, "Кінець лікування", record.TreatmentEnd?.ToString("dd.MM.yyyy") ?? "—");
                        LabelValue(table, "Статус", record.IsActive ? "Активне" : "Завершено");
                        LabelValue(table, "Рекомендації", string.IsNullOrWhiteSpace(record.Recommendations)
                                                                   ? "—" : record.Recommendations);
                    });
                });
            });
        })
        .GeneratePdf(outputPath);
    }

    // ═══════════════════════════════════
    // 3. Щеплення тварини
    // ═══════════════════════════════════
    public void ExportVaccinations(AnimalCard animal, string outputPath)
    {
        Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(t => t.FontSize(11).FontFamily("Arial"));

                page.Header().Element(c => ComposeHeader("Щеплення")(c));
                page.Footer().Element(c => ComposeFooter()(c));

                page.Content().Column(col =>
                {
                    col.Item().Text("Дані тварини та власника")
                       .FontSize(13).SemiBold().FontColor(QColors.Grey.Darken2);
                    col.Item().PaddingBottom(8);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });

                        InfoRow(table, "Ім'я", animal.Name, "Вид", animal.Species);
                        InfoRow(table, "Порода", animal.Breed, "Вік", $"{animal.Age} р.");
                        InfoRow(table, "Стать", animal.Sex, "", "");
                        InfoRow(table, "Власник", animal.OwnerName, "Телефон", animal.OwnerPhone);
                    });

                    col.Item().PaddingBottom(16);

                    col.Item().Text("Щеплення")
                       .FontSize(13).SemiBold().FontColor(QColors.Grey.Darken2);
                    col.Item().PaddingBottom(8);

                    if (!animal.Vaccinations.Any())
                    {
                        col.Item().Text("Щеплень не зафіксовано.")
                           .FontColor(QColors.Grey.Medium);
                    }
                    else
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(40);
                                c.RelativeColumn(3);
                                c.RelativeColumn(2);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background(QColors.Grey.Lighten3).Padding(6).Text("#").SemiBold();
                                h.Cell().Background(QColors.Grey.Lighten3).Padding(6).Text("Назва вакцини").SemiBold();
                                h.Cell().Background(QColors.Grey.Lighten3).Padding(6).Text("Дата").SemiBold();
                            });

                            var list = animal.Vaccinations
                                .OrderByDescending(v => v.VaccinationDate)
                                .ToList();

                            for (int i = 0; i < list.Count; i++)
                            {
                                var v = list[i];
                                table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2)
                                     .Padding(6).Text((i + 1).ToString());
                                table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2)
                                     .Padding(6).Text(v.VaccineName);
                                table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2)
                                     .Padding(6).Text(v.VaccinationDate.ToString("dd.MM.yyyy"));
                            }
                        });

                        col.Item().PaddingTop(8)
                           .Text($"Всього щеплень: {animal.Vaccinations.Count}")
                           .FontSize(10).FontColor(QColors.Grey.Medium);
                    }
                });
            });
        })
        .GeneratePdf(outputPath);
    }

    // ═══════════════════════════════════
    // Допоміжні методи
    // ═══════════════════════════════════

    private Action<QContainer> ComposeHeader(string title) => container =>
    {
        container.BorderBottom(1).BorderColor(QColors.Grey.Lighten2)
                 .PaddingBottom(8).Row(row =>
                 {
                     row.RelativeItem().Text("Vet Master")
                        .FontSize(10).FontColor(QColors.Grey.Medium);
                     row.RelativeItem().AlignRight().Text(title)
                        .FontSize(16).SemiBold();
                 });
    };

    private Action<QContainer> ComposeFooter() => container =>
    {
        container.BorderTop(1).BorderColor(QColors.Grey.Lighten2)
                 .PaddingTop(6).Row(row =>
                 {
                     row.RelativeItem()
                        .Text($"Сформовано: {DateTime.Now:dd.MM.yyyy HH:mm}")
                        .FontSize(9).FontColor(QColors.Grey.Medium);
                     row.RelativeItem().AlignRight()
                        .Text(x =>
                        {
                            x.Span("Сторінка ").FontSize(9).FontColor(QColors.Grey.Medium);
                            x.CurrentPageNumber().FontSize(9).FontColor(QColors.Grey.Medium);
                            x.Span(" з ").FontSize(9).FontColor(QColors.Grey.Medium);
                            x.TotalPages().FontSize(9).FontColor(QColors.Grey.Medium);
                        });
                 });
    };

    private void StatCard(TableDescriptor table, string label, string value, string color)
    {
        table.Cell().Border(1).BorderColor(QColors.Grey.Lighten2)
             .Padding(10).Column(c =>
             {
                 c.Item().Text(label).FontSize(8).FontColor(QColors.Grey.Medium);
                 c.Item().Text(value).FontSize(22).SemiBold().FontColor(color);
             });
    }

    private void InfoRow(TableDescriptor table,
        string label1, string val1,
        string label2, string val2)
    {
        table.Cell().Background(QColors.Grey.Lighten4).Padding(6)
             .Text(label1).SemiBold().FontSize(10);
        table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2)
             .Padding(6).Text(val1);
        table.Cell().Background(QColors.Grey.Lighten4).Padding(6)
             .Text(label2).SemiBold().FontSize(10);
        table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2)
             .Padding(6).Text(val2);
    }

    private void LabelValue(TableDescriptor table, string label, string value)
    {
        table.Cell().Background(QColors.Grey.Lighten4).Padding(8)
             .Text(label).SemiBold().FontSize(10);
        table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2)
             .Padding(8).Text(value);
    }
}