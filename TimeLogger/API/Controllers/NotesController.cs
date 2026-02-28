using Microsoft.AspNetCore.Mvc;
using SkillAllocationTracker.Application.Interfaces;
using SkillAllocationTracker.Domain.Entities;
using TimeLogger.Models;

namespace TimeLogger.API.Controllers
{
    public class NotesController : Controller
    {
        private readonly IUnitOfWork _uow;

        public NotesController(IUnitOfWork uow) => _uow = uow;

        public async Task<IActionResult> Index(NoteScope? scope, DateTime? date)
        {
            var notes = (await _uow.NoteRepository.GetAllAsync()).AsQueryable();

            if (scope.HasValue)
                notes = notes.Where(n => n.Scope == scope.Value);

            if (date.HasValue)
            {
                var dt = date.Value.Date;
                notes = notes.Where(n => n.OccurrenceDate.Date == dt);
            }

            var model = notes
                .OrderByDescending(n => n.OccurrenceDate)
                .ThenByDescending(n => n.CreatedAt)
                .Select(n => new NoteListItemViewModel
                {
                    Id = n.Id,
                    Title = n.Title,
                    Body = n.Body,
                    Scope = n.Scope,
                    OccurrenceDate = n.OccurrenceDate,
                    CreatedAt = n.CreatedAt
                }).ToList();

            ViewBag.ScopeFilter = scope;
            ViewBag.DateFilter = date?.ToString("yyyy-MM-dd") ?? string.Empty;

            return View(model);
        }

        public IActionResult Create()
        {
            var vm = new NoteEditViewModel { OccurrenceDate = DateTime.UtcNow };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NoteEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var note = new Note
            {
                Id = Guid.NewGuid(),
                Title = vm.Title,
                Body = vm.Body,
                Scope = vm.Scope,
                OccurrenceDate = vm.OccurrenceDate.ToUniversalTime(),
                CreatedAt = DateTime.UtcNow
            };

            await _uow.NoteRepository.AddAsync(note);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "Note created.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var note = await _uow.NoteRepository.GetByIdAsync(id);
            if (note == null) return NotFound();

            var vm = new NoteEditViewModel
            {
                Id = note.Id,
                Title = note.Title,
                Body = note.Body,
                Scope = note.Scope,
                OccurrenceDate = note.OccurrenceDate.ToLocalTime()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, NoteEditViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var note = await _uow.NoteRepository.GetByIdAsync(id);
            if (note == null) return NotFound();

            note.Title = vm.Title;
            note.Body = vm.Body;
            note.Scope = vm.Scope;
            note.OccurrenceDate = vm.OccurrenceDate.ToUniversalTime();
            note.UpdatedAt = DateTime.UtcNow;

            _uow.NoteRepository.Update(note);
            await _uow.SaveChangesAsync();

            TempData["Success"] = "Note updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var note = await _uow.NoteRepository.GetByIdAsync(id);
            if (note != null)
            {
                _uow.NoteRepository.Remove(note);
                await _uow.SaveChangesAsync();
                TempData["Success"] = "Note deleted.";
            }
            else
            {
                TempData["Error"] = "Note not found.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}