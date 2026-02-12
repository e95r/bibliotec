namespace Bibliotec.Models;

/// <summary>
/// Модель жанра книги.
/// Добавлено русскоязычное алиас-свойство для совместимости
/// с кодом, который обращается к полю "Название".
/// </summary>
public sealed class Genres
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Название
    {
        get => Name;
        set => Name = value;
    }
}
