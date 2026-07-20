import os

files = [
    r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetLeaveBalanceQueryHandler.cs",
    r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetPendingHRApprovalsQueryHandler.cs",
    r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Commands\CreateLeaveRequestCommandHandler.cs",
    r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetPendingManagerApprovalsQueryHandler.cs",
    r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetLeaveRequestByIdQueryHandler.cs"
]

for p in files:
    with open(p, 'a', encoding='utf-8') as f:
        f.write("\n}\n")

print("Added missing braces.")
