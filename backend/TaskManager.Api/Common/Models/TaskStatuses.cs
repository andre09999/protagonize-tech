namespace TaskManager.Api.Common.Models;

public static class TaskStatuses
{
    public const string Pendente = "Pendente";
    public const string Concluida = "Concluida";

    public static readonly string[] All = [Pendente, Concluida];

    public static string Normalize(string status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return string.Empty;
        }

        var trimmed = status.Trim();

        return All.FirstOrDefault(allowed =>
            string.Equals(allowed, trimmed, StringComparison.OrdinalIgnoreCase)) ?? trimmed;
    }
}
