using CostAccounting.Data;
using CostAccounting.Models;
using CostAccounting.Services.PaycodeService;
using Microsoft.EntityFrameworkCore;

namespace CostAccounting.Services.Paycodeservice
{
    public class Paycodeservice : IPaycodeService
    {
        private readonly ApplicationDbContext _context;

        public Paycodeservice(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<PaycodeIndexVM> GetPagedAsync(PaycodeIndexVM q)
{
    var query = _context.Paycodes.AsNoTracking().AsQueryable();

    if (!string.IsNullOrWhiteSpace(q.SearchTerm))
    {
        var s = q.SearchTerm.Trim();
        query = query.Where(p =>
            p.Code.ToString().Contains(s) ||
            (p.Description != null && p.Description.Contains(s)) ||
            (p.Abbrev != null && p.Abbrev.Contains(s)));
    }

    if (q.FilterActive.HasValue)
        query = q.FilterActive.Value
            ? query.Where(p => p.Active == true)
            : query.Where(p => p.Active != true);

    q.TotalCount = await query.CountAsync();

    bool desc = string.Equals(q.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
    query = q.SortColumn switch
    {
        "Description" => desc ? query.OrderByDescending(p => p.Description) : query.OrderBy(p => p.Description),
        "Abbrev"      => desc ? query.OrderByDescending(p => p.Abbrev)      : query.OrderBy(p => p.Abbrev),
        "Calculation" => desc ? query.OrderByDescending(p => p.Calculation) : query.OrderBy(p => p.Calculation),
        "OutOfClass"  => desc ? query.OrderByDescending(p => p.OutOfClass)  : query.OrderBy(p => p.OutOfClass),
        "ShiftDiff"   => desc ? query.OrderByDescending(p => p.ShiftDiff)   : query.OrderBy(p => p.ShiftDiff),
        "OPP"         => desc ? query.OrderByDescending(p => p.OPP)         : query.OrderBy(p => p.OPP),
        "Active"      => desc ? query.OrderByDescending(p => p.Active)      : query.OrderBy(p => p.Active),
        _             => desc ? query.OrderByDescending(p => p.Code)        : query.OrderBy(p => p.Code)
    };

    if (q.CurrentPage < 1) q.CurrentPage = 1;
    if (q.CurrentPage > q.TotalPages) q.CurrentPage = q.TotalPages;

    q.Paycodes = await query
        .Skip((q.CurrentPage - 1) * q.PageSize)
        .Take(q.PageSize)
        .Select(p => new PaycodeVM
        {
            Code = p.Code,
            Description = p.Description,
            Abbrev = p.Abbrev,
            Active = p.Active ?? false,
            OutOfClass = p.OutOfClass ?? false,
            ShiftDiff = p.ShiftDiff ?? false,
            OPP = p.OPP ?? false,
            Calculation = p.Calculation
        })
        .ToListAsync();

    return q;
}

    public async Task<(bool ok, string message)> CreateAsync(PaycodeVM vm, string user)
    {
        if (await _context.Paycodes.AnyAsync(p => p.Code == vm.Code))
            return (false, $"Paycode '{vm.Code}' already exists.");

        _context.Paycodes.Add(new Paycode
        {
            Code = vm.Code,
            Description = vm.Description?.Trim(),
            Abbrev = vm.Abbrev?.Trim(),
            Active = vm.Active,
            OutOfClass = vm.OutOfClass,
            ShiftDiff = vm.ShiftDiff,
            OPP = vm.OPP,
            Calculation = vm.Calculation,
            EnteredDate = DateTime.Now,      // hidden from UI, set automatically
            EnteredByUser = user
        });

        await _context.SaveChangesAsync();
        return (true, "Paycode created.");
    }

    public async Task<(bool ok, string message)> UpdateAsync(PaycodeVM vm)
    {
        var entity = await _context.Paycodes.FindAsync(vm.Code);
        if (entity == null) return (false, "Paycode not found.");

        entity.Description = vm.Description?.Trim();
        entity.Abbrev = vm.Abbrev?.Trim();
        entity.Active = vm.Active;
        entity.OutOfClass = vm.OutOfClass;
        entity.ShiftDiff = vm.ShiftDiff;
        entity.OPP = vm.OPP;
        entity.Calculation = vm.Calculation;
        // EnteredDate / EnteredByUser intentionally untouched

        await _context.SaveChangesAsync();
        return (true, "Paycode updated.");
    }
    }
}