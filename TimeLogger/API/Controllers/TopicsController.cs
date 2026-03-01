using Microsoft.AspNetCore.Mvc;
using SkillAllocationTracker.Application.DTOs;
using SkillAllocationTracker.Application.Interfaces;
using SkillAllocationTracker.Application.Services;
using SkillAllocationTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TimeLogger.Models.ViewModels;

namespace TimeLogger.API.Controllers
{
    public class TopicsController : Controller
    {
        private readonly ITopicService _topicService;
        private readonly IUnitOfWork _uow;
        private readonly DayOfWeek _startDay;

        public TopicsController(ITopicService topicService, IUnitOfWork uow)
        {
            _topicService = topicService;
            _uow = uow;
            _startDay = DayOfWeek.Sunday;
        }

        public async Task<IActionResult> Index()
        {
            var topics = (await _topicService.GetAllAsync()).ToList();
            var weekly = (await _uow.WeeklyConfigRepository.GetAllAsync()).FirstOrDefault();
            var totalHours = weekly?.TotalWeeklyHours ?? 0;
            var logs = (await _uow.TimeLogRepository.GetAllAsync()).ToList();

            var ci = CultureInfo.InvariantCulture;
            var calendar = ci.Calendar;
            var rule = CalendarWeekRule.FirstFourDayWeek;
            var currentWeek = calendar.GetWeekOfYear(DateTime.UtcNow, rule, _startDay);
            var currentYear = DateTime.UtcNow.Year;

            var models = new List<TopicViewModel>();
            foreach (var t in topics)
            {
                var minutesThisWeek = logs.Where(l =>
                {
                    var w = calendar.GetWeekOfYear(l.LogDate.ToLocalTime(), rule, _startDay);
                    return l.TopicId == t.Id && w == currentWeek && l.LogDate.ToLocalTime().Year == currentYear;
                }).Sum(l => l.DurationMinutes);

                var minutesAll = logs.Where(l => l.TopicId == t.Id).Sum(l => l.DurationMinutes);

                var planned = Math.Round((t.Percentage / 100.0) * totalHours, 2);
                var actualThisWeek = Math.Round(minutesThisWeek / 60.0, 2);
                var actualAll = Math.Round(minutesAll / 60.0, 2);
                var diff = Math.Round(actualThisWeek - planned, 2);
                double eff = planned == 0 ? 0 : Math.Round((actualThisWeek / planned) * 100, 2);

                models.Add(new TopicViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Percentage = t.Percentage,
                    CalculatedWeeklyHours = Math.Round(t.CalculatedWeeklyHours(totalHours), 2),
                    PlannedWeeklyHours = planned,
                    TotalHoursThisWeek = actualThisWeek,
                    TotalHoursAllTime = actualAll,
                    DifferenceHours = diff,
                    EfficiencyPercent = eff
                });
            }

            ViewBag.TotalWeeklyHours = totalHours;

            // Aggregates for footer
            ViewBag.AggregatePlanned = Math.Round(models.Sum(m => m.PlannedWeeklyHours), 2);
            ViewBag.AggregateThisWeek = Math.Round(models.Sum(m => m.TotalHoursThisWeek), 2);
            ViewBag.AggregateAllTime = Math.Round(models.Sum(m => m.TotalHoursAllTime), 2);

            return View(models);
        }

        public IActionResult Create() => View("CreateEdit", new CreateEditTopicModel());

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

            // populate topic base model
            var model = new CreateEditTopicModel { Id = t.Id, Name = t.Name, Percentage = t.Percentage };

            // load timelogs for this topic
            var logs = (await _uow.TimeLogRepository.FindAsync(l => l.TopicId == id)).ToList();

            model.TimeLogs = logs.Select(l => new TimeLogViewModel
            {
                TopicId = l.TopicId,
                DurationMinutes = l.DurationMinutes,
                Note = l.Note,
                LogDate = l.LogDate.ToLocalTime(),
                // include Id in a hidden property via dynamic binding -- TimeLogViewModel doesn't have Id property; add via ViewData or create new model if needed
            }).ToList();

            // If you need the log Id on actions, we will use a small anonymous list for the view.
            // For convenience pass a list of simple objects to the ViewBag containing Id and fields.
            ViewBag.TimeLogRows = logs.Select(l => new
            {
                l.Id,
                l.DurationMinutes,
                l.Note,
                LogDateLocal = l.LogDate.ToLocalTime().ToString("yyyy-MM-ddTHH:mm"),
                l.TopicId,
                l.CreatedAt
            }).ToList();

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
                return RedirectToAction(nameof(Edit), new { id = model.TopicId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var topics = (await _topicService.GetAllAsync()).Select(t => new { t.Id, t.Name }).ToList();
                model.Topics = topics;
                return View(model);
            }
        }

        // GET partial for edit timelog (AJAX)
        [HttpGet]
        public async Task<IActionResult> EditTimeLog(Guid id)
        {
            var tl = await _uow.TimeLogRepository.GetByIdAsync(id);
            if (tl == null) return NotFound();

            var model = new TimeLogViewModel
            {
                TopicId = tl.TopicId,
                DurationMinutes = tl.DurationMinutes,
                Note = tl.Note,
                LogDate = tl.LogDate.ToLocalTime()
            };

            ViewBag.TimeLogId = tl.Id;
            return PartialView("_EditTimeLogPartial", model);
        }

        // POST edit timelog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTimeLog(Guid id, TimeLogViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.TimeLogId = id;
                return PartialView("_EditTimeLogPartial", model);
            }

            var existing = await _uow.TimeLogRepository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.DurationMinutes = model.DurationMinutes;
            existing.Note = model.Note;
            existing.LogDate = model.LogDate.ToUniversalTime();

            _uow.TimeLogRepository.Update(existing);
            await _uow.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = existing.TopicId });
        }

        // Delete timelog
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTimeLog(Guid id, Guid topicId)
        {
            var existing = await _uow.TimeLogRepository.GetByIdAsync(id);
            if (existing != null)
            {
                _uow.TimeLogRepository.Remove(existing);
                await _uow.SaveChangesAsync();
                TempData["Success"] = "Time log deleted.";
            }
            else
            {
                TempData["Error"] = "Time log not found.";
            }

            return RedirectToAction(nameof(Edit), new { id = topicId });
        }

        public async Task<IActionResult> Summary()
        {
            var weekly = (await _uow.WeeklyConfigRepository.GetAllAsync()).FirstOrDefault();
            var totalPlanned = weekly?.TotalWeeklyHours ?? 0;

            var topics = (await _topicService.GetAllAsync()).ToList();
            var logs = (await _uow.TimeLogRepository.GetAllAsync()).ToList();

            var ci = CultureInfo.InvariantCulture;
            var calendar = ci.Calendar;
            var rule = CalendarWeekRule.FirstFourDayWeek;
            var weekNum = calendar.GetWeekOfYear(DateTime.UtcNow, rule, _startDay);

            var thisWeekLogs = logs.Where(l =>
            {
                var w = calendar.GetWeekOfYear(l.LogDate.ToLocalTime(), rule, _startDay);
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

        // AnalyticsDetailed remains unchanged
        public async Task<IActionResult> AnalyticsDetailed()
        {
            var weekly = (await _uow.WeeklyConfigRepository.GetAllAsync()).FirstOrDefault();
            var totalHours = weekly?.TotalWeeklyHours ?? 0;
            var topics = (await _topicService.GetAllAsync()).ToList();
            var logs = (await _uow.TimeLogRepository.GetAllAsync()).ToList();

            var ci = CultureInfo.InvariantCulture;
            var calendar = ci.Calendar;
            var rule = CalendarWeekRule.FirstFourDayWeek;
            var currentWeek = calendar.GetWeekOfYear(DateTime.UtcNow, rule, _startDay);
            var currentYear = DateTime.UtcNow.Year;

            var topicNames = topics.Select(t => t.Name).ToArray();
            var planned = topics.Select(t => Math.Round((t.Percentage / 100.0) * totalHours, 2)).ToArray();

            var actualThisWeek = topics.Select(t =>
            {
                var minutes = logs.Where(l =>
                {
                    var w = calendar.GetWeekOfYear(l.LogDate.ToLocalTime(), rule, _startDay);
                    return l.TopicId == t.Id && w == currentWeek && l.LogDate.ToLocalTime().Year == currentYear;
                }).Sum(l => l.DurationMinutes);
                return Math.Round(minutes / 60.0, 2);
            }).ToArray();

            var under = new List<(string Name, double Planned, double Actual, double Eff)>();
            var over = new List<(string Name, double Planned, double Actual, double Eff)>();

            for (int i = 0; i < topics.Count; i++)
            {
                var p = planned[i];
                var a = actualThisWeek[i];
                var eff = p == 0 ? 0 : Math.Round((a / p) * 100, 2);
                if (p == 0) continue;
                if (a < p * 0.9) under.Add((topics[i].Name, p, a, eff));
                else if (a > p * 1.1) over.Add((topics[i].Name, p, a, eff));
            }

            var weeks = Enumerable.Range(0, 8).Select(i =>
            {
                var dt = DateTime.UtcNow.AddDays(-7 * i);
                var w = calendar.GetWeekOfYear(dt, rule, _startDay);
                var y = dt.Year;
                return (WeekNum: w, Year: y, Label: $"W{w}");
            }).Reverse().ToArray();

            var trendLabels = weeks.Select(w => w.Label).ToArray();
            var trendValuesPerTopic = new List<double[]>();

            foreach (var t in topics)
            {
                var values = weeks.Select(w =>
                {
                    var minutes = logs.Where(l =>
                    {
                        var lw = calendar.GetWeekOfYear(l.LogDate.ToLocalTime(), rule, _startDay);
                        return l.TopicId == t.Id && lw == w.WeekNum && l.LogDate.ToLocalTime().Year == w.Year;
                    }).Sum(l => l.DurationMinutes);
                    return Math.Round(minutes / 60.0, 2);
                }).ToArray();

                trendValuesPerTopic.Add(values);
            }

            var model = new Models.ViewModels.AnalyticsDetailedViewModel
            {
                TopicNames = topicNames,
                Planned = planned,
                ActualThisWeek = actualThisWeek,
                UnderPerforming = under.Select(x => new Models.ViewModels.UnderOverItem { Name = x.Name, Planned = x.Planned, Actual = x.Actual, Efficiency = x.Eff }).ToList(),
                OverPerforming = over.Select(x => new Models.ViewModels.UnderOverItem { Name = x.Name, Planned = x.Planned, Actual = x.Actual, Efficiency = x.Eff }).ToList(),
                TrendLabels = trendLabels,
                TrendValuesPerTopic = trendValuesPerTopic
            };

            return View(model);
        }
    }
}