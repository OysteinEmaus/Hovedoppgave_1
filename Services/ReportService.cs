using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hovedoppgave.Api.Data;
using Hovedoppgave.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Hovedoppgave.Api.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Report>> GetAllReportsAsync(int userId)
        {
            return await _context.Reports
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Report> GetReportByIdAsync(int reportId, int userId)
        {
            var report = await _context.Reports
                .FirstOrDefaultAsync(r => r.Id == reportId && r.UserId == userId);

            if (report == null)
            {
                throw new Exception("Report not found");
            }

            return report;
        }

        public async Task<Report> CreateReportAsync(Report report)
        {
            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<Report> UpdateReportAsync(int reportId, Report reportUpdate, int userId)
        {
            var report = await _context.Reports
                .FirstOrDefaultAsync(r => r.Id == reportId && r.UserId == userId);

            if (report == null)
            {
                throw new Exception("Report not found");
            }

            report.Title = reportUpdate.Title;
            report.Content = reportUpdate.Content;
            report.UpdatedAt = DateTime.UtcNow;

            _context.Reports.Update(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task DeleteReportAsync(int reportId, int userId)
        {
            var report = await _context.Reports
                .FirstOrDefaultAsync(r => r.Id == reportId && r.UserId == userId);

            if (report == null)
            {
                throw new Exception("Report not found");
            }

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();
        }
    }
}