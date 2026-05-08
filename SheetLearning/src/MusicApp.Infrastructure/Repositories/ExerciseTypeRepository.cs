using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class ExerciseTypeRepository : IExerciseTypeRepository
{
    private readonly AppDbContext _context;

    public ExerciseTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ExerciseType>> GetAllAsync()
    {
        return await _context.ExerciseTypes
            .AsNoTracking()
            .OrderBy(exerciseType => exerciseType.Id)
            .ToListAsync();
    }

    public async Task<ExerciseType?> GetByIdAsync(int id)
    {
        return await _context.ExerciseTypes.FirstOrDefaultAsync(exerciseType => exerciseType.Id == id);
    }
}
