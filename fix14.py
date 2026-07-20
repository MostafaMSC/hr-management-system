import os

def fix14():
    # 1. LeaveBalanceDto
    p = r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetLeaveBalanceQueryHandler.cs"
    with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
    c = c.replace("""                    AnnualDays: 30,
                    RemainingAnnualDays: 30);""", """                    TotalDays: 30,
                    RemainingDays: 30,
                    TotalHours: 24,
                    RemainingHours: 24);""")
    c = c.replace("""                balance.AnnualDays,
                balance.RemainingAnnualDays,
                (balance.AnnualDays * 8),
                (balance.RemainingAnnualDays * 8)""", """                balance.TotalAllowed,
                balance.Remaining,
                (balance.TotalAllowed * 8),
                (balance.Remaining * 8)""")
    with open(p, 'w', encoding='utf-8') as f: f.write(c)

    # 2. GetFingerprintsCountQueryHandler.cs
    p = r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Fingerprints\Queries\GetFingerprintsCountQueryHandler.cs"
    with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
    c = c.replace("await _fingerprintRepository.GetCountAsync(cancellationToken);", "(await _fingerprintRepository.GetAllAsync(cancellationToken)).Count();")
    with open(p, 'w', encoding='utf-8') as f: f.write(c)

    # 3. GetAllFingerprintsQueryHandler.cs
    p = r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Fingerprints\Queries\GetAllFingerprintsQueryHandler.cs"
    with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
    c = c.replace("HRs.Count);", "HRs.Count());")
    c = c.replace("return HRs;", "return HRs.Select(h => new HRDto { UserID = h.UserId.ToString(), Fid = h.FingerIndex, Template = h.Template }).ToList();")
    with open(p, 'w', encoding='utf-8') as f: f.write(c)

    # 4. LeaveRequestResponseDto CreatedAt
    import re
    files = [
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetPendingManagerApprovalsQueryHandler.cs",
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetPendingHRApprovalsQueryHandler.cs",
        r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetLeaveRequestByIdQueryHandler.cs"
    ]
    for p in files:
        with open(p, 'r', encoding='utf-8-sig') as f: c = f.read()
        c = re.sub(r'null\s*\/\*\s*RejectedAt\s*\*\/\s*\)\)', 'null /* RejectedAt */, lr.CreatedAt))', c)
        with open(p, 'w', encoding='utf-8') as f: f.write(c)

fix14()
print("Fix 14 done.")
