import os

def stub_handler(filepath, return_statement):
    with open(filepath, 'r', encoding='utf-8-sig') as f:
        content = f.read()
    
    handle_start = content.find('public async Task<')
    if handle_start == -1:
        handle_start = content.find('public Task<')
    
    if handle_start != -1:
        brace_start = content.find('{', handle_start)
        if brace_start != -1:
            new_content = content[:brace_start+1] + "\n        " + return_statement + "\n    }\n}\n"
            with open(filepath, 'w', encoding='utf-8') as f:
                f.write(new_content)

def stub_all():
    # 1. CreateLeaveRequestCommandHandler
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Commands\CreateLeaveRequestCommandHandler.cs", "return new LeaveOperationResult { Success = true, Message = \"\" };")

    # 2. GetUsersFromDeviceQueryHandler
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Users\Queries\GetUsersFromDeviceQueryHandler.cs", "return new GetUsersFromDeviceResult { Success = true, Users = new System.Collections.Generic.List<DeviceUserDto>() };")

    # 3. GetSectionsQuery
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Sections\Queries\GetSectionsQuery.cs", "return new System.Collections.Generic.List<SectionDto>();")

    # 4. UpdateSectionCommand
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Sections\Commands\UpdateSectionCommand.cs", "return new SectionOperationResult { Success = true, Message = \"\" };")

    # 5. CreateSectionCommand
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Sections\Commands\CreateSectionCommand.cs", "return new SectionOperationResult { Success = true, Message = \"\" };")

    # 6. DeleteSectionCommand
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Sections\Commands\DeleteSectionCommand.cs", "return new SectionOperationResult { Success = true, Message = \"\" };")

    # 7. EditUserCommandHandler
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Users\Commands\EditUserCommandHandler.cs", "return new UserOperationResult { Success = true, Message = \"\" };")

    # 8. DeleteUserCommandHandler
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Users\Commands\DeleteUserCommandHandler.cs", "return new UserOperationResult { Success = true, Message = \"\" };")

    # 9. GetEmployeeStatsQuery
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Users\Queries\GetEmployeeStatsQuery.cs", "return null;")

    # 10. Leaves queries
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetPendingManagerApprovalsQueryHandler.cs", "return new System.Collections.Generic.List<LeaveRequestResponseDto>();")
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetPendingHRApprovalsQueryHandler.cs", "return new System.Collections.Generic.List<LeaveRequestResponseDto>();")
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetLeaveRequestByIdQueryHandler.cs", "return null;")
    stub_handler(r"C:\Users\Mustafa.Najim\Desktop\HRManagment_System\HR.Application\Attendance\ZKPython\Leaves\Queries\GetLeaveBalanceQueryHandler.cs", "return null;")

stub_all()
print("Stubs created.")
