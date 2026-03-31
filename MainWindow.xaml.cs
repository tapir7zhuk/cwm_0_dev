using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Vet_Master.Models;
using Vet_Master.Services;
using Vet_Master.Dialogs;

namespace Vet_Master;

public partial class MainWindow : Window
{
    private readonly AnimalCardService _animalService;
    private readonly RecordService _recordService;

    private List<AnimalCard> _allAnimals = new();
    private AnimalCard? _selectedAnimal;

    public MainWindow(AnimalCardService animalService, RecordService recordService)
    {
        InitializeComponent();
        _animalService = animalService;
        _recordService = recordService;
        Loaded += async (_, _) => await RefreshAllAsync();
    }

    // ═══════════════════════════════════
    // Завантаження / пошук
    // ═══════════════════════════════════

    private async Task RefreshAllAsync()
    {
        _allAnimals = await _animalService.GetAllAsync();
        ApplySearch(SearchBox.Text);
        await RefreshStatisticsAsync();
    }

    private void ApplySearch(string query)
    {
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allAnimals
            : _allAnimals.Where(a =>
                a.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                a.Species.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                a.Breed.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        AnimalListBox.ItemsSource = filtered;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        => ApplySearch(SearchBox.Text);

    // ═══════════════════════════════════
    // Вибір тварини → заповнення деталей
    // ═══════════════════════════════════

    private async void AnimalListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AnimalListBox.SelectedItem is not AnimalCard animal) return;

        _selectedAnimal = await _animalService.GetByIdAsync(animal.Id);
        if (_selectedAnimal is null) return;

        ShowDetail(_selectedAnimal);
    }

    private void ShowDetail(AnimalCard a)
    {
        NoSelectionHint.Visibility = Visibility.Collapsed;
        DetailPanel.Visibility = Visibility.Visible;

        DetailName.Text = a.Name;
        DetailMeta.Text = $"{a.Species} · {a.Breed} · {a.Age} р. · {a.OwnerName}";

        InfoSpecies.Text = a.Species;
        InfoBreed.Text = a.Breed;
        InfoAge.Text = $"{a.Age} р.";
        InfoSex.Text = a.Sex;
        InfoOwner.Text = a.OwnerName;
        InfoPhone.Text = a.OwnerPhone;
        InfoMedia.Text = string.IsNullOrWhiteSpace(a.MediaPath) ? "— не вказано" : a.MediaPath;

        RecordListBox.ItemsSource = a.Records.OrderByDescending(r => r.VisitDate).ToList();
        VaccinationGrid.ItemsSource = a.Vaccinations.OrderByDescending(v => v.VaccinationDate).ToList();
    }

    // ═══════════════════════════════════
    // Тварини: Додати / Редагувати / Видалити
    // ═══════════════════════════════════

    private async void BtnAddAnimal_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new AnimalDialog();
        if (dlg.ShowDialog() != true) return;

        await _animalService.AddAsync(dlg.Result!);
        await RefreshAllAsync();
    }

    private async void BtnEditAnimal_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAnimal is null) return;

        var dlg = new AnimalDialog(_selectedAnimal);
        if (dlg.ShowDialog() != true) return;

        await _animalService.UpdateAsync(dlg.Result!);
        await RefreshAllAsync();
    }

    private async void BtnDeleteAnimal_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAnimal is null) return;

        var confirm = MessageBox.Show(
            $"Видалити «{_selectedAnimal.Name}» та всі її записи?",
            "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        await _animalService.DeleteAsync(_selectedAnimal.Id);
        NoSelectionHint.Visibility = Visibility.Visible;
        DetailPanel.Visibility = Visibility.Collapsed;
        _selectedAnimal = null;
        await RefreshAllAsync();
    }

    // ═══════════════════════════════════
    // Медіа
    // ═══════════════════════════════════

    private void BtnOpenMedia_Click(object sender, RoutedEventArgs e)
    {
        var path = _selectedAnimal?.MediaPath;
        if (string.IsNullOrWhiteSpace(path)) { MessageBox.Show("Шлях не вказано."); return; }

        try { Process.Start(new ProcessStartInfo(path) { UseShellExecute = true }); }
        catch { MessageBox.Show("Не вдалося відкрити файл."); }
    }

    // ═══════════════════════════════════
    // Записи хвороб
    // ═══════════════════════════════════

    private async void BtnAddRecord_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAnimal is null) return;

        var dlg = new RecordDialog();
        if (dlg.ShowDialog() != true) return;

        dlg.Result!.AnimalCardId = _selectedAnimal.Id;
        await _recordService.AddAsync(dlg.Result);
        _selectedAnimal = await _animalService.GetByIdAsync(_selectedAnimal.Id);
        ShowDetail(_selectedAnimal!);
    }

    private async void BtnEditRecord_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not int id) return;
        var record = _selectedAnimal?.Records.FirstOrDefault(r => r.Id == id);
        if (record is null) return;

        var dlg = new RecordDialog(record);
        if (dlg.ShowDialog() != true) return;

        await _recordService.UpdateAsync(dlg.Result!);
        _selectedAnimal = await _animalService.GetByIdAsync(_selectedAnimal!.Id);
        ShowDetail(_selectedAnimal!);
    }

    private async void BtnDeleteRecord_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not int id) return;

        var confirm = MessageBox.Show("Видалити цей запис?", "Підтвердження",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (confirm != MessageBoxResult.Yes) return;

        await _recordService.DeleteAsync(id);
        _selectedAnimal = await _animalService.GetByIdAsync(_selectedAnimal!.Id);
        ShowDetail(_selectedAnimal!);
    }

    private void RecordListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

    // ═══════════════════════════════════
    // Щеплення
    // ═══════════════════════════════════

    private async void BtnAddVaccination_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAnimal is null) return;

        var dlg = new VaccinationDialog();
        if (dlg.ShowDialog() != true) return;

        dlg.Result!.AnimalCardId = _selectedAnimal.Id;

        // Додаємо через той самий сервіс або окремий — для простоти через DB напряму
        var scope = ((App)Application.Current)._host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
        db.Vaccinations.Add(dlg.Result);
        await db.SaveChangesAsync();

        _selectedAnimal = await _animalService.GetByIdAsync(_selectedAnimal.Id);
        ShowDetail(_selectedAnimal!);
    }

    private async void BtnDeleteVaccination_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as Button)?.Tag is not int id) return;

        var scope = ((App)Application.Current)._host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
        var v = await db.Vaccinations.FindAsync(id);
        if (v is not null) { db.Vaccinations.Remove(v); await db.SaveChangesAsync(); }

        _selectedAnimal = await _animalService.GetByIdAsync(_selectedAnimal!.Id);
        ShowDetail(_selectedAnimal!);
    }

    // ═══════════════════════════════════
    // Статистика
    // ═══════════════════════════════════

    private async Task RefreshStatisticsAsync()
    {
        var active = await _recordService.GetActiveAsync();
        var finished = await _recordService.GetFinishedLastMonthAsync();
        var total = _allAnimals.Count;

        StatActive.Text = active.Count.ToString();
        StatActiveSub.Text = $"з {total} тварин";
        StatFinished.Text = finished.Count.ToString();
        StatFinishedSub.Text = DateTime.Today.AddMonths(-1).ToString("MMMM yyyy");
        StatTotal.Text = total.ToString();

        // По видах
        var bySpecies = _allAnimals
            .GroupBy(a => a.Species)
            .Select(g => new { Species = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        double maxCount = bySpecies.Any() ? bySpecies.Max(x => x.Count) : 1;

        StatBySpecies.ItemsSource = bySpecies.Select(x => new
        {
            x.Species,
            x.Count,
            BarWidth = (x.Count / maxCount) * 300
        }).ToList();

        StatActiveGrid.ItemsSource = active;
    }
}