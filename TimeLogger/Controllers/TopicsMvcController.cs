using Microsoft.AspNetCore.Mvc;
using SkillAllocationTracker.Application.DTOs;
using SkillAllocationTracker.Application.Services;
using SkillAllocationTracker.Application.Interfaces;
using SkillAllocationTracker.Domain.Entities;
using TimeLogger.Models.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace TimeLogger.Controllers
{
    public class TopicsMvcController : Controller
    {
        private readonly ITopicService _topicService;
        private readonly IUnitOfWork _uow;

        public TopicsMvcController(ITopicService topicService, IUnitOfWork uow)
        {
            _topicService = topicService;
            _uow = uow;
        }

        public async Task<IActionResult> Index()
        {
            var topics = (await _topicService.GetAllAsync()).ToList();
            var weekly = (await _uow.WeeklyConfigRepository.GetAllAsync()).FirstOrDefault();
            var totalHours = weekly?.TotalWeeklyHours ?? 0;

            var model = topics.Select(t => new TopicViewModel
            {
                Id = t.Id,
                Name = t.Name,
                Percentage = t.Percentage,
                CalculatedWeeklyHours = Math.Round(t.CalculatedWeeklyHours(totalHours), 2)
            }).ToList();

            ViewBag.TotalWeeklyHours = totalHours;
            return View(model);
        }

        public IActionResult Create()
        {
            return View("CreateEdit", new CreateEditTopicModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEditTopicModel model)
        {
            if (!ModelState.IsValid) return View("CreateEdit", model);

            try
            {
                var dto = new TopicDto { Name = model.Name, Percentage = model.Percentage };
                await _topicService.CreateAsync(dto);
                TempData["Success"] = "Topic created.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("CreateEdit", model);
            }
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var t = await _topicService.GetByIdAsync(id);
            if (t == null) return NotFound();
            var model = new CreateEditTopicModel { Id = t.Id, Name = t.Name, Percentage = t.Percentage };
            return View("CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CreateEditTopicModel model)
        {
            if (!ModelState.IsValid) return View("CreateEdit", model);

            try
            {
                var dto = new TopicDto { Name = model.Name, Percentage = model.Percentage };
                await _topicService.UpdateAsync(model.Id, dto);
                TempData["Success"] = "Topic updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View("CreateEdit", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _topicService.DeleteAsync(id);
                TempData["Success"] = "Topic deleted.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AddTimeLog(Guid? topicId)
        {
            var topics = (await _topicService.GetAllAsync()).Select(t => new { t.Id, t.Name }).ToList();
            var model = new TimeLogViewModel { TopicId = topicId ?? Guid.Empty, Topics = topics };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTimeLog(TimeLogViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var topics = (await _topicService.GetAllAsync()).Select(t => new { t.Id, t.Name }).ToList();
                model.Topics = topics;
                return View(model);
            }

            try
            {
                var timelog = new TimeLog
                {
                    Id = Guid.NewGuid(),
                    TopicId = model.TopicId,
                    DurationMinutes = model.DurationMinutes,
                    Note = model.Note,
                    LogDate = model.LogDate.ToUniversalTime(),
                    CreatedAt = DateTime.UtcNow
                };

                await _uow.TimeLogRepository.AddAsync(timelog);
                await _uow.SaveChangesAsync();

                TempData["Success"] = "Time log added.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var topics = (await _topicService.GetAllAsync()).Select(t => new { t.Id, t.Name }).ToList();
                model.Topics = topics;
                return View(model);
            }
        }

        public async Task<IActionResult> Summary()
        {
            var weekly = (await _uow.WeeklyConfigRepository.GetAllAsync()).FirstOrDefault();
            var totalPlanned = weekly?.TotalWeeklyHours ?? 0;

            var topics = (await _topicService.GetAllAsync()).ToList();
            var logs = (await _uow.TimeLogRepository.GetAllAsync()).ToList();

            // current ISO week
            var ci = CultureInfo.InvariantCulture;
            var calendar = ci.Calendar;
            var rule = CalendarWeekRule.FirstFourDayWeek;
            var weekNum = calendar.GetWeekOfYear(DateTime.UtcNow, rule, DayOfWeek.Monday);

            var thisWeekLogs = logs.Where(l =>
            {
                var w = calendar.GetWeekOfYear(l.LogDate.ToLocalTime(), rule, DayOfWeek.Monday);
                return w == weekNum && l.LogDate.ToLocalTime().Year == DateTime.Now.Year;
            }).ToList();

            var totalLoggedMinutes = thisWeekLogs.Sum(x => x.DurationMinutes);
            var totalLoggedHours = Math.Round(totalLoggedMinutes / 60.0, 2);

            double completion = totalPlanned == 0 ? 0 : Math.Round((totalLoggedHours / totalPlanned) * 100, 2);

            var mostFocused = thisWeekLogs.GroupBy(l => l.TopicId)
                .Select(g => new { TopicId = g.Key, Minutes = g.Sum(x => x.DurationMinutes) })
                .OrderByDescending(x => x.Minutes)
                .FirstOrDefault();

            string mostFocusedName = "—";
            if (mostFocused != null)
            {
                var top = topics.FirstOrDefault(t => t.Id == mostFocused.TopicId);
                mostFocusedName = top?.Name ?? "—";
            }

            var model = new SummaryViewModel
            {
                TotalPlannedHours = totalPlanned,
                TotalLoggedHours = totalLoggedHours,
                CompletionPercentage = completion,
                MostFocusedTopic = mostFocusedName
            };

            return View(model);
        }

        public async Task<IActionResult> Analytics()
        {
            var weekly = (await _uow.WeeklyConfigRepository.GetAllAsync()).FirstOrDefault();
            var totalHours = weekly?.TotalWeeklyHours ?? 0;
            var topics = (await _topicService.GetAllAsync()).ToList();
            var logs = (await _uow.TimeLogRepository.GetAllAsync()).ToList();

            // labels & planned
            var labels = topics.Select(t => t.Name).ToArray();
            var planned = topics.Select(t => Math.Round((t.Percentage / 100.0) * totalHours, 2)).ToArray();

            // actual per topic this week
            var ci = CultureInfo.InvariantCulture;
            var calendar = ci.Calendar;
            var rule = CalendarWeekRule.FirstFourDayWeek;
            var weekNum = calendar.GetWeekOfYear(DateTime.UtcNow, rule, DayOfWeek.Monday);

            var actual = topics.Select(t =>
            {
                var minutes = logs.Where(l =>
                {
                    var w = calendar.GetWeekOfYear(l.LogDate.ToLocalTime(), rule, DayOfWeek.Monday);
                    return l.TopicId == t.Id && w == weekNum && l.LogDate.ToLocalTime().Year == DateTime.Now.Year;
                }).Sum(l => l.DurationMinutes);

                return Math.Round(minutes / 60.0, 2);
            }).ToArray();

            // weekly trend last 8 weeks total logged hours
            var trendLabels = Enumerable.Range(0, 8).Select(i =>
            {
                var dt = DateTime.UtcNow.AddDays(-7 * i);
                var w = calendar.GetWeekOfYear(dt, rule, DayOfWeek.Monday);
                return $"W{w}";
            }).Reverse().ToArray();

            var trendValues = trendLabels.Select(lbl =>
            {
                // extract week number int
                var wnum = int.Parse(lbl.TrimStart('W'));
                var minutes = logs.Where(l =>
                {
                    var w = calendar.GetWeekOfYear(l.LogDate.ToLocalTime(), rule, DayOfWeek.Monday);
                    return w == wnum && l.LogDate.ToLocalTime().Year == DateTime.Now.Year;
                }).Sum(l => l.DurationMinutes);
                return Math.Round(minutes / 60.0, 2);
            }).ToArray();

            var model = new AnalyticsViewModel
            {
                Labels = labels,
                Planned = planned,
                Actual = actual,
                TrendLabels = trendLabels,
                TrendValues = trendValues
            };

            return View(model);
        }
    }
}