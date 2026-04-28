using Microsoft.EntityFrameworkCore;
using MusicApp.Domain.Entities;

namespace MusicApp.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new Role { Id = 1, Nome = "Admin", Descrizione = "Amministratore piattaforma" },
                new Role { Id = 2, Nome = "Insegnante", Descrizione = "Insegnante di musica" },
                new Role { Id = 3, Nome = "Utente Pro", Descrizione = "Utente con piano Pro attivo" },
                new Role { Id = 4, Nome = "Utente", Descrizione = "Utente standard" });
            await context.SaveChangesAsync();
        }

        if (!await context.Plans.AnyAsync())
        {
            context.Plans.AddRange(
                new Plan { Id = 1, Nome = "Standard", Descrizione = "Piano gratuito", PrezzoMensile = 0.00m },
                new Plan { Id = 2, Nome = "Pro", Descrizione = "Piano Pro mensile", PrezzoMensile = 9.99m });
            await context.SaveChangesAsync();
        }

        if (!await context.ExerciseTypes.AnyAsync())
        {
            context.ExerciseTypes.AddRange(
                new ExerciseType { Id = 1, Nome = "Lettura Note", Descrizione = "Riconoscimento visivo di note sul pentagramma" },
                new ExerciseType { Id = 2, Nome = "Lettura Accordi", Descrizione = "Riconoscimento visivo di accordi sul pentagramma" },
                new ExerciseType { Id = 3, Nome = "Ritmo", Descrizione = "Esercizi di lettura ed esecuzione ritmica" });
            await context.SaveChangesAsync();
        }

        if (!await context.Clefs.AnyAsync())
        {
            context.Clefs.AddRange(
                new Clef { Id = 1, Nome = "Chiave di Sol" },
                new Clef { Id = 2, Nome = "Chiave di Basso" });
            await context.SaveChangesAsync();
        }

        if (!await context.Levels.AnyAsync())
        {
            context.Levels.AddRange(
                new Level
                {
                    Id = 1,
                    ExerciseTypeId = 1,
                    ClefId = 1,
                    NumeroLivello = 1,
                    Nome = "Note centrali",
                    Descrizione = "Do4-Sol4, chiave di Sol",
                    PunteggioMinimoSblocco = 70
                },
                new Level
                {
                    Id = 2,
                    ExerciseTypeId = 1,
                    ClefId = 1,
                    NumeroLivello = 2,
                    Nome = "Rigo completo",
                    Descrizione = "Estensione righe supplementari, chiave di Sol completa",
                    PunteggioMinimoSblocco = 75
                },
                new Level
                {
                    Id = 3,
                    ExerciseTypeId = 1,
                    ClefId = 2,
                    NumeroLivello = 3,
                    Nome = "Chiave di Basso e lettura mista",
                    Descrizione = "Chiave di Basso, poi lettura combinata Sol e Basso",
                    PunteggioMinimoSblocco = 80
                },
                new Level
                {
                    Id = 4,
                    ExerciseTypeId = 2,
                    ClefId = 1,
                    NumeroLivello = 1,
                    Nome = "Triadi fondamentali",
                    Descrizione = "Triadi maggiori e minori in posizione fondamentale",
                    PunteggioMinimoSblocco = 70
                },
                new Level
                {
                    Id = 5,
                    ExerciseTypeId = 2,
                    ClefId = 1,
                    NumeroLivello = 2,
                    Nome = "Accordi di settima",
                    Descrizione = "Maj7, Dom7, Min7",
                    PunteggioMinimoSblocco = 75
                },
                new Level
                {
                    Id = 6,
                    ExerciseTypeId = 2,
                    ClefId = 1,
                    NumeroLivello = 3,
                    Nome = "Inversioni",
                    Descrizione = "Inversioni di triadi e accordi di settima",
                    PunteggioMinimoSblocco = 80
                });
            await context.SaveChangesAsync();
        }
    }
}
