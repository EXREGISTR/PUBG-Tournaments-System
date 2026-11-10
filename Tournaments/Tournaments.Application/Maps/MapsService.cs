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
    public async Task Create(MapEditModel editModel, CancellationToken token = default) {
        var exists = await repository.Exists(editModel.Title, token);

        if (exists) {
            throw ExceptionFactories.Map.Conflict("MapAlreadyExists", "Map with same title already exists");
        }
        
        var map = new Map(editModel.Title, editModel.LogotypeUrl);
        
        repository.Insert(map);

        await unitOfWork.SaveChanges(token);
    }

    public async Task<IReadOnlyCollection<MapView>> GetActiveMaps(CancellationToken token = default) {
        var maps = await repository.GetActiveMaps(token);

        return maps
            .Select(map => new MapView(map.Id, map.Title, map.LogotypeUrl))
            .ToArray();
    }

    public async Task Edit(int mapId, MapEditModel editModel, CancellationToken token = default) {
        var map = await repository.GetById(mapId, token);

        if (map == null) {
            throw ExceptionFactories.Map.NotFound();
        }
        
        map.Edit(editModel.Title, editModel.LogotypeUrl);
        
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