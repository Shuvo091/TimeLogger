using Microsoft.EntityFrameworkCore;
using SkillAllocationTracker.Application.DTOs;
using SkillAllocationTracker.Application.Interfaces;
using SkillAllocationTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SkillAllocationTracker.Application.Services
{
    public interface ITopicService
    {
        Task<IEnumerable<Topic>> GetAllAsync();
        Task<Topic?> GetByIdAsync(Guid id);
        Task<Topic> CreateAsync(TopicDto dto);
        Task<Topic> UpdateAsync(Guid id, TopicDto dto);
        Task DeleteAsync(Guid id);
    }

    public class TopicService : ITopicService
    {
        private readonly IUnitOfWork _uow;

        public TopicService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Topic> CreateAsync(TopicDto dto)
        {
            // Prevent duplicates
            var existing = (await _uow.TopicRepository.FindAsync(t => t.Name.ToLower() == dto.Name.ToLower())).FirstOrDefault();
            if (existing != null) throw new InvalidOperationException("Topic with same name exists.");

            // Validate sum of percentages
            var topics = (await _uow.TopicRepository.GetAllAsync()).ToList();
            var sum = topics.Sum(t => t.Percentage) + dto.Percentage;
            if (sum != 100) throw new InvalidOperationException($"Sum of all topic percentages must equal 100. Current would be {sum}.");

            var entity = new Topic
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Percentage = dto.Percentage,
                Readiness = dto.Readiness ?? string.Empty,
                TotalTargetHoursAllTime = dto.TotalTargetHoursAllTime,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.TopicRepository.AddAsync(entity);
            await _uow.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync(Guid id)
        {
            var existing = await _uow.TopicRepository.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException("Topic not found.");
            await EnsureAfterDeletePercentageConstraint(existing);
            _uow.TopicRepository.Remove(existing);
            await _uow.SaveChangesAsync();
        }

        private async Task EnsureAfterDeletePercentageConstraint(Topic removing)
        {
            var topics = (await _uow.TopicRepository.GetAllAsync()).ToList();
            var sumRemaining = topics.Sum(t => t.Percentage) - removing.Percentage;
            if (sumRemaining != 100 && topics.Count > 1)
            {
                throw new InvalidOperationException("Deleting this topic would cause total percentage != 100. Adjust allocations first.");
            }
        }

        public async Task<IEnumerable<Topic>> GetAllAsync() => await _uow.TopicRepository.GetAllAsync();

        public async Task<Topic?> GetByIdAsync(Guid id) => await _uow.TopicRepository.GetByIdAsync(id);

        public async Task<Topic> UpdateAsync(Guid id, TopicDto dto)
        {
            var existing = await _uow.TopicRepository.GetByIdAsync(id);
            if (existing == null) throw new KeyNotFoundException("Topic not found.");

            // Prevent duplicate names (excluding self)
            var duplicates = (await _uow.TopicRepository.FindAsync(t => t.Name.ToLower() == dto.Name.ToLower() && t.Id != id)).Any();
            if (duplicates) throw new InvalidOperationException("Topic with same name exists.");

            // Validate percentages sum
            var topics = (await _uow.TopicRepository.GetAllAsync()).ToList();
            var sumOthers = topics.Where(t => t.Id != id).Sum(t => t.Percentage);
            var sum = sumOthers + dto.Percentage;
            if (sum != 100) throw new InvalidOperationException($"Sum of all topic percentages must equal 100. Current would be {sum}.");

            existing.Name = dto.Name;
            existing.Percentage = dto.Percentage;
            existing.Readiness = dto.Readiness ?? string.Empty;
            existing.TotalTargetHoursAllTime = dto.TotalTargetHoursAllTime;

            _uow.TopicRepository.Update(existing);
            await _uow.SaveChangesAsync();
            return existing;
        }
    }
}