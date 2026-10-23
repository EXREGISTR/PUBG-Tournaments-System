using Application.Common;
using Application.Common.Repositories;
using Application.Maps.DTOs;
using Domain.Common;
using Domain.Common.Extensions;
using Domain.Maps;

namespace Application.Maps;

public class MapsService(
    IMapsRepository repository, 
    IUnitOfWork unitOfWork) {
    public async Task Create(CreateMapRequest request, CancellationToken token = default) {
        var exists = await repository.Exists(request.Title, token);

        if (exists) {
            throw ExceptionFactories.Map.Conflict("MapAlreadyExists", "Map with same title already exists");
        }
        
        var map = new Map(request.Title, request.LogotypeUrl);
        
        repository.Insert(map);

        await unitOfWork.SaveChanges(token);
    }

    public async Task Edit(EditMapRequest request, CancellationToken token = default) {
        var map = await repository.GetById(request.MapId, token);

        if (map == null) {
            throw ExceptionFactories.Map.NotFound();
        }
        
        map.Edit(request.Title, request.LogotypeUrl);
        
        await unitOfWork.SaveChanges(token);
    }
    
    public async Task ToggleVisibility(int mapId, CancellationToken token = default) {
        var map = await repository.GetById(mapId, token);
        
        if (map == null) {
            throw ExceptionFactories.Map.NotFound();
        }
        
        map.ToggleVisibility();
        
        await unitOfWork.SaveChanges(token);
    }
}