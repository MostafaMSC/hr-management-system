using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Application.Settings.Commands;

public record UpdateWorkSettingsCommand(Dictionary<string, object> Settings) : IRequest<bool>;

public class UpdateWorkSettingsCommandHandler : IRequestHandler<UpdateWorkSettingsCommand, bool>
{
    private readonly ISettingsRepository _settingsRepository;

    public UpdateWorkSettingsCommandHandler(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository;
    }

    public async Task<bool> Handle(UpdateWorkSettingsCommand request, CancellationToken cancellationToken)
    {
        var stringSettings = request.Settings.ToDictionary(k => k.Key, v => v.Value?.ToString() ?? string.Empty);
        await _settingsRepository.UpdateSettingsAsync("Work", stringSettings, cancellationToken);

        return true;
    }
}
