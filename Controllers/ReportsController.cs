using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Hovedoppgave.Models;
using Hovedoppgave.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hovedoppgave.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly ReportService _reportService;

        public ReportsController(ReportService reportService)
        {
            _reportService = reportService;
        }

        private int GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                throw new Exception("User ID not found in token");
            }
            return int.Parse(userIdClaim.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();
            var reports = await _reportService.GetAllReportsAsync(userId);
            return Ok(reports);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var userId = GetUserId();
                var report = await _reportService.GetReportByIdAsync(id, userId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Report report)
        {
            var userId = GetUserId();
            report.UserId = userId;

            var createdReport = await _reportService.CreateReportAsync(report);
            return CreatedAtAction(nameof(GetById), new { id = createdReport.Id }, createdReport);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Report reportUpdate)
        {
            try
            {
                var userId = GetUserId();
                var updatedReport = await _reportService.UpdateReportAsync(id, reportUpdate, userId);
                return Ok(updatedReport);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetUserId();
                await _reportService.DeleteReportAsync(id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}