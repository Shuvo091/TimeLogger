using SkillAllocationTracker.Application.Interfaces;
using SkillAllocationTracker.Domain.Entities;
using SkillAllocationTracker.Infrastructure.DbContexts;
using SkillAllocationTracker.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;

namespace SkillAllocationTracker.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _db;
        public IGenericRepository<Topic> TopicRepository { get; }
        public IGenericRepository<WeeklyConfig> WeeklyConfigRepository { get; }
        public IGenericRepository<TimeLog> TimeLogRepository { get; }
        public IGenericRepository<Note> NoteRepository { get; }

        public UnitOfWork(AppDbContext db)
        {
            _db = db;
            TopicRepository = new GenericRepository<Topic>(_db);
            WeeklyConfigRepository = new GenericRepository<WeeklyConfig>(_db);
            TimeLogRepository = new GenericRepository<TimeLog>(_db);
            NoteRepository = new GenericRepository<Note>(_db);
        }

        public async Task<int> SaveChangesAsync() => await _db.SaveChangesAsync();
        public async ValueTask DisposeAsync() => await _db.DisposeAsync();
    }
}