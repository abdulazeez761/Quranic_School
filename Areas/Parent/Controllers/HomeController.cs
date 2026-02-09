using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Hifz.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParentModel = Hifz.Models.Parent;

namespace Hifz.Areas.Parent.Controllers
{
    [Authorize(Roles = "Parent")]
    [Area("Parent")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IParentService _parentService;
        private readonly IStudentService _studentService;

        public HomeController(
            ILogger<HomeController> logger,
            IParentService parentService,
            IStudentService studentService
        )
        {
            _logger = logger;
            _parentService = parentService;
            _studentService = studentService;
        }

        public async Task<IActionResult> Index()
        {
            Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            ParentModel? parent = await _parentService.GetParentByUserIdAsync(userId);

            if (parent == null)
            {
                return NotFound("Parent profile not found.");
            }

            // Get children (students) for the parent
            var children = await _parentService.GetParentChildrenAsync(userId);
            var childrenList = children.ToList();

            // Get children with active meetings
            var childrenWithActiveMeetings = childrenList
                .Where(c => c.Classes != null && c.Classes.Any(cl => cl.IsMeetingActive))
                .ToList();

            // Calculate aggregated stats across all children
            int totalAttendanceRecords = 0;
            int presentCount = 0;
            int totalWirds = 0;
            int completedWirds = 0;
            int totalMemorizedJuz = 0;
            var recentActivities = new List<dynamic>();

            foreach (var child in childrenList)
            {
                var attendance = await _studentService.GetStudentAttendanceAsync(child.UserId);
                var attendanceList = attendance.ToList();
                totalAttendanceRecords += attendanceList.Count;
                presentCount += attendanceList.Count(a =>
                    a.Status == Hifz.Models.AttendanceStatus.Present
                    || a.Status == Hifz.Models.AttendanceStatus.Late
                );

                var wirds = await _studentService.GetStudentWirdsAsync(child.UserId);
                var wirdsList = wirds.ToList();
                totalWirds += wirdsList.Count;
                completedWirds += wirdsList.Count(w => w.IsCompleted);

                totalMemorizedJuz += child.MemorizedJuz;

                // Collect recent attendance for activity feed
                foreach (var att in attendanceList.OrderByDescending(a => a.Date).Take(3))
                {
                    recentActivities.Add(
                        new
                        {
                            Type = "attendance",
                            ChildName = $"{child.StudentInfo.FirstName} {child.StudentInfo.SecondName}",
                            Date = att.Date,
                            Status = att.Status.ToString(),
                            ClassName = att.Class?.Name ?? "",
                        }
                    );
                }

                // Collect recent wirds for activity feed
                foreach (var w in wirdsList.OrderByDescending(w => w.AssignedDate).Take(3))
                {
                    recentActivities.Add(
                        new
                        {
                            Type = "wird",
                            ChildName = $"{child.StudentInfo.FirstName} {child.StudentInfo.SecondName}",
                            Date = w.AssignedDate,
                            Status = w.IsCompleted ? "Completed" : "Pending",
                            WirdType = w.Type.ToString(),
                        }
                    );
                }
            }

            double attendanceRate =
                totalAttendanceRecords > 0
                    ? Math.Round((double)presentCount / totalAttendanceRecords * 100, 1)
                    : 0;

            ViewBag.Parent = parent;
            ViewBag.Children = childrenList;
            ViewBag.ChildrenWithActiveMeetings = childrenWithActiveMeetings;
            ViewBag.TotalAttendanceRecords = totalAttendanceRecords;
            ViewBag.AttendanceRate = attendanceRate;
            ViewBag.TotalWirds = totalWirds;
            ViewBag.CompletedWirds = completedWirds;
            ViewBag.TotalMemorizedJuz = totalMemorizedJuz;
            ViewBag.RecentActivities = recentActivities
                .OrderByDescending(a => (DateTime)a.Date)
                .Take(10)
                .ToList();

            return View(parent);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }
    }
}
