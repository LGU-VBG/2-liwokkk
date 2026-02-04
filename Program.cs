// Практическая работа №2
// Вариант: "Солнечная панель и Инженер"
// Панель генерирует 2–8 кВт⋅ч в час. 
// Аккумулятор — 50 кВт⋅ч (начально 30).
// >45 кВт⋅ч — событие "Переключить на сеть".
// Эффективность падает каждые 72 часа на 10–20%.
// Ниже 60% — событие "Очистить панель".
// Инвертор проверяется каждые 500 кВт⋅ч, счётчик начинается с 460.
// Инженер выполняет действия по событиям.

using System;

namespace SolarPanelApp
{
    class SolarPanel
    {
        public event Action? OnSwitchToGrid;
        public event Action? OnCleanPanel;
        public event Action? OnCheckInverter;

        private Random rnd = new Random();

        public double Battery { get; private set; } = 30.0;
        public double Capacity { get; private set; } = 50.0;
        public double Efficiency { get; private set; } = 0.75;
        public double TotalEnergyProduced { get; private set; } = 460.0;
        private int hoursWorked = 0;

        public void WorkOneHour()
        {
            hoursWorked++;

            // Генерация электроэнергии с учётом эффективности
            double generated = rnd.Next(2, 9) * Efficiency;
            Battery += generated;
            if (Battery > Capacity) Battery = Capacity;

            TotalEnergyProduced += generated;

            // Каждые 24 часа — краткий отчёт
            if (hoursWorked % 24 == 0)
            {
                Console.WriteLine($"\nПрошло {hoursWorked / 24} дней:");
                Console.WriteLine($"Аккумулятор = {Battery:F2} кВт⋅ч, эффективность = {Efficiency * 100:F1}%");
            }

            // Проверка событий
            if (Battery > 45)
                OnSwitchToGrid?.Invoke();

            if (TotalEnergyProduced >= 500)
                OnCheckInverter?.Invoke();

            // Каждые 72 часа — загрязнение
            if (hoursWorked % 72 == 0)
            {
                double loss = rnd.Next(10, 21) / 100.0;
                Efficiency -= loss;
                if (Efficiency < 0.6)
                    OnCleanPanel?.Invoke();
            }
        }

        public void ResetEfficiency() => Efficiency = 1.0;
        public void ResetInverter() => TotalEnergyProduced = 0;

        public void DischargeToGrid()
        {
            double excess = Battery - 45;
            if (excess > 0)
            {
                Battery -= excess;
                Console.WriteLine($"→ Отдано в сеть {excess:F2} кВт⋅ч. Остаток: {Battery:F2}");
            }
        }
    }

    class Engineer
    {
        public void SwitchToGrid(SolarPanel p)
        {
            Console.WriteLine("Событие: Переключить на сеть — инженер отдает излишки энергии.");
            p.DischargeToGrid();
        }

        public void CleanPanel(SolarPanel p)
        {
            Console.WriteLine("Событие: Очистить панель — инженер восстанавливает эффективность до 100%.");
            p.ResetEfficiency();
        }

        public void CheckInverter(SolarPanel p)
        {
            Console.WriteLine("Событие: Проверить инвертор — инженер выполняет проверку и сбрасывает счетчик.");
            p.ResetInverter();
        }
    }

    class Program
    {
        static void Main()
        {
            SolarPanel panel = new SolarPanel();
            Engineer eng = new Engineer();

            // Подписка на события
            panel.OnSwitchToGrid += () => eng.SwitchToGrid(panel);
            panel.OnCleanPanel += () => eng.CleanPanel(panel);
            panel.OnCheckInverter += () => eng.CheckInverter(panel);

            Console.WriteLine("=== Симуляция работы солнечной панели на 72 часа ===\n");

            for (int i = 0; i < 72; i++) // три дня
                panel.WorkOneHour();

            Console.WriteLine("\nСимуляция завершена.");
        }
    }
}
