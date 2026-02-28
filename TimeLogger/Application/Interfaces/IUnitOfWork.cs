using SkillAllocationTracker.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace SkillAllocationTracker.Application.Interfaces
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IGenericRepository<Topic> TopicRepository { get; }
        IGenericRepository<WeeklyConfig> WeeklyConfigRepository { get; }
        IGenericRepository<TimeLog> TimeLogRepository { get; }
        IGenericRepository<Note> NoteRepository { get; }
        Task<int> SaveChangesAsync();
    }
}